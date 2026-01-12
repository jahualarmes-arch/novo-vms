using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace VMS_AlarmesJahu1.Services.P2P
{
    /// <summary>
    /// P2P tunnel management class for Intelbras devices
    /// Handles device connection, tunnel creation and management with secure credentials storage
    /// </summary>
    public class IntelbrasP2P : IDisposable
    {
        private readonly ILogger<IntelbrasP2P> _logger;
        private readonly Dictionary<string, P2PDeviceContext> _activeDevices;
        private readonly ICredentialsStorage _credentialsStorage;
        private readonly P2PConfiguration _configuration;
        private readonly object _syncLock = new object();
        private bool _disposed = false;

        /// <summary>
        /// Configuration settings for P2P tunnel management
        /// </summary>
        public class P2PConfiguration
        {
            public int ConnectionTimeoutMs { get; set; } = 30000;
            public int ReconnectionAttempts { get; set; } = 3;
            public int ReconnectionDelayMs { get; set; } = 5000;
            public int KeepAliveIntervalMs { get; set; } = 60000;
            public bool EnableCompression { get; set; } = true;
            public bool EnableEncryption { get; set; } = true;
        }

        /// <summary>
        /// Device context for maintaining connection state and tunnel information
        /// </summary>
        public class P2PDeviceContext
        {
            public string DeviceId { get; set; }
            public string DeviceIp { get; set; }
            public int DevicePort { get; set; }
            public P2PConnectionState ConnectionState { get; set; }
            public DateTime LastConnectionAttempt { get; set; }
            public DateTime LastKeepAlive { get; set; }
            public int ConnectionAttempts { get; set; }
            public string TunnelId { get; set; }
            public NetworkStream TunnelStream { get; set; }
            public CancellationTokenSource CancellationTokenSource { get; set; }
            public DeviceCredentials Credentials { get; set; }
        }

        /// <summary>
        /// P2P connection state enumeration
        /// </summary>
        public enum P2PConnectionState
        {
            Disconnected,
            Connecting,
            Connected,
            Authenticated,
            TunnelEstablished,
            Error,
            Reconnecting
        }

        /// <summary>
        /// Device credentials for authentication
        /// </summary>
        public class DeviceCredentials
        {
            public string Username { get; set; }
            public string Password { get; set; }
            public string Token { get; set; }
            public DateTime TokenExpirationTime { get; set; }
            public bool IsEncrypted { get; set; }

            public bool IsTokenValid => DateTime.UtcNow < TokenExpirationTime;
        }

        /// <summary>
        /// Credentials storage interface
        /// </summary>
        public interface ICredentialsStorage
        {
            Task<DeviceCredentials> GetCredentialsAsync(string deviceId);
            Task StoreCredentialsAsync(string deviceId, DeviceCredentials credentials);
            Task<bool> DeleteCredentialsAsync(string deviceId);
            Task<bool> ValidateCredentialsAsync(string deviceId);
            Task RefreshTokenAsync(string deviceId);
        }

        /// <summary>
        /// P2P tunnel event arguments
        /// </summary>
        public class P2PTunnelEventArgs : EventArgs
        {
            public string DeviceId { get; set; }
            public P2PConnectionState State { get; set; }
            public string Message { get; set; }
            public Exception Exception { get; set; }
            public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        }

        /// <summary>
        /// Events
        /// </summary>
        public event EventHandler<P2PTunnelEventArgs> ConnectionStateChanged;
        public event EventHandler<P2PTunnelEventArgs> TunnelEstablished;
        public event EventHandler<P2PTunnelEventArgs> ConnectionError;
        public event EventHandler<P2PTunnelEventArgs> DataReceived;

        /// <summary>
        /// Constructor
        /// </summary>
        public IntelbrasP2P(ILogger<IntelbrasP2P> logger, ICredentialsStorage credentialsStorage, P2PConfiguration configuration = null)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _credentialsStorage = credentialsStorage ?? throw new ArgumentNullException(nameof(credentialsStorage));
            _configuration = configuration ?? new P2PConfiguration();
            _activeDevices = new Dictionary<string, P2PDeviceContext>();
        }

        /// <summary>
        /// Register and connect a device
        /// </summary>
        public async Task<bool> RegisterDeviceAsync(string deviceId, string deviceIp, int devicePort, DeviceCredentials credentials)
        {
            if (string.IsNullOrWhiteSpace(deviceId))
                throw new ArgumentException("Device ID cannot be null or empty", nameof(deviceId));

            if (string.IsNullOrWhiteSpace(deviceIp))
                throw new ArgumentException("Device IP cannot be null or empty", nameof(deviceIp));

            if (devicePort <= 0 || devicePort > 65535)
                throw new ArgumentException("Invalid device port", nameof(devicePort));

            try
            {
                lock (_syncLock)
                {
                    if (_activeDevices.ContainsKey(deviceId))
                    {
                        _logger.LogWarning($"Device {deviceId} is already registered");
                        return false;
                    }
                }

                // Validate IP address format
                if (!IPAddress.TryParse(deviceIp, out _))
                {
                    _logger.LogError($"Invalid IP address format for device {deviceId}: {deviceIp}");
                    return false;
                }

                // Store credentials securely
                await _credentialsStorage.StoreCredentialsAsync(deviceId, credentials);

                // Create device context
                var deviceContext = new P2PDeviceContext
                {
                    DeviceId = deviceId,
                    DeviceIp = deviceIp,
                    DevicePort = devicePort,
                    Credentials = credentials,
                    ConnectionState = P2PConnectionState.Disconnected,
                    CancellationTokenSource = new CancellationTokenSource()
                };

                lock (_syncLock)
                {
                    _activeDevices[deviceId] = deviceContext;
                }

                _logger.LogInformation($"Device {deviceId} registered successfully at {deviceIp}:{devicePort}");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error registering device {deviceId}");
                RaiseConnectionError(deviceId, "Registration failed", ex);
                return false;
            }
        }

        /// <summary>
        /// Establish connection to a registered device
        /// </summary>
        public async Task<bool> ConnectAsync(string deviceId)
        {
            if (string.IsNullOrWhiteSpace(deviceId))
                throw new ArgumentException("Device ID cannot be null or empty", nameof(deviceId));

            P2PDeviceContext context = null;

            lock (_syncLock)
            {
                if (!_activeDevices.TryGetValue(deviceId, out context))
                {
                    _logger.LogError($"Device {deviceId} not found in active devices");
                    return false;
                }
            }

            try
            {
                context.LastConnectionAttempt = DateTime.UtcNow;
                context.ConnectionAttempts++;

                RaiseConnectionStateChanged(deviceId, P2PConnectionState.Connecting, "Attempting to connect to device");

                // Attempt connection with retries
                bool connected = await AttemptConnectionAsync(context);

                if (!connected && context.ConnectionAttempts < _configuration.ReconnectionAttempts)
                {
                    _logger.LogInformation($"Reconnection attempt {context.ConnectionAttempts} for device {deviceId}");
                    context.ConnectionState = P2PConnectionState.Reconnecting;
                    RaiseConnectionStateChanged(deviceId, P2PConnectionState.Reconnecting, $"Reconnection attempt {context.ConnectionAttempts}");

                    await Task.Delay(_configuration.ReconnectionDelayMs);
                    return await ConnectAsync(deviceId);
                }

                if (connected)
                {
                    context.ConnectionAttempts = 0;
                    _logger.LogInformation($"Device {deviceId} connected successfully");
                    RaiseConnectionStateChanged(deviceId, P2PConnectionState.Connected, "Device connected");
                    return true;
                }

                throw new InvalidOperationException("Failed to establish connection after all retry attempts");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Connection error for device {deviceId}");
                context.ConnectionState = P2PConnectionState.Error;
                RaiseConnectionError(deviceId, "Connection failed", ex);
                return false;
            }
        }

        /// <summary>
        /// Authenticate device with P2P tunnel
        /// </summary>
        public async Task<bool> AuthenticateAsync(string deviceId)
        {
            if (string.IsNullOrWhiteSpace(deviceId))
                throw new ArgumentException("Device ID cannot be null or empty", nameof(deviceId));

            P2PDeviceContext context = null;

            lock (_syncLock)
            {
                if (!_activeDevices.TryGetValue(deviceId, out context))
                {
                    _logger.LogError($"Device {deviceId} not found");
                    return false;
                }
            }

            try
            {
                if (context.ConnectionState != P2PConnectionState.Connected)
                {
                    _logger.LogError($"Device {deviceId} is not connected. Current state: {context.ConnectionState}");
                    return false;
                }

                // Validate stored credentials
                if (!await _credentialsStorage.ValidateCredentialsAsync(deviceId))
                {
                    _logger.LogError($"Credentials validation failed for device {deviceId}");
                    return false;
                }

                // Refresh token if needed
                if (!context.Credentials.IsTokenValid)
                {
                    await _credentialsStorage.RefreshTokenAsync(deviceId);
                    context.Credentials = await _credentialsStorage.GetCredentialsAsync(deviceId);
                }

                // Perform authentication handshake
                bool authenticated = await PerformAuthenticationAsync(context);

                if (authenticated)
                {
                    context.ConnectionState = P2PConnectionState.Authenticated;
                    _logger.LogInformation($"Device {deviceId} authenticated successfully");
                    RaiseConnectionStateChanged(deviceId, P2PConnectionState.Authenticated, "Device authenticated");
                    return true;
                }

                throw new InvalidOperationException("Authentication handshake failed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Authentication error for device {deviceId}");
                context.ConnectionState = P2PConnectionState.Error;
                RaiseConnectionError(deviceId, "Authentication failed", ex);
                return false;
            }
        }

        /// <summary>
        /// Create P2P tunnel to authenticated device
        /// </summary>
        public async Task<string> CreateTunnelAsync(string deviceId)
        {
            if (string.IsNullOrWhiteSpace(deviceId))
                throw new ArgumentException("Device ID cannot be null or empty", nameof(deviceId));

            P2PDeviceContext context = null;

            lock (_syncLock)
            {
                if (!_activeDevices.TryGetValue(deviceId, out context))
                {
                    _logger.LogError($"Device {deviceId} not found");
                    return null;
                }
            }

            try
            {
                if (context.ConnectionState != P2PConnectionState.Authenticated)
                {
                    _logger.LogError($"Device {deviceId} is not authenticated. Current state: {context.ConnectionState}");
                    return null;
                }

                // Generate tunnel ID
                string tunnelId = GenerateTunnelId();
                context.TunnelId = tunnelId;

                // Initialize tunnel
                bool tunnelCreated = await InitializeTunnelAsync(context);

                if (tunnelCreated)
                {
                    context.ConnectionState = P2PConnectionState.TunnelEstablished;
                    context.LastKeepAlive = DateTime.UtcNow;

                    _logger.LogInformation($"P2P tunnel {tunnelId} established for device {deviceId}");
                    RaiseConnectionStateChanged(deviceId, P2PConnectionState.TunnelEstablished, $"P2P tunnel {tunnelId} established");
                    RaiseTunnelEstablished(deviceId, tunnelId);

                    // Start keep-alive task
                    _ = KeepAliveTaskAsync(context);

                    return tunnelId;
                }

                throw new InvalidOperationException("Tunnel initialization failed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Tunnel creation error for device {deviceId}");
                context.ConnectionState = P2PConnectionState.Error;
                RaiseConnectionError(deviceId, "Tunnel creation failed", ex);
                return null;
            }
        }

        /// <summary>
        /// Send data through P2P tunnel
        /// </summary>
        public async Task<bool> SendDataAsync(string deviceId, byte[] data)
        {
            if (string.IsNullOrWhiteSpace(deviceId))
                throw new ArgumentException("Device ID cannot be null or empty", nameof(deviceId));

            if (data == null || data.Length == 0)
                throw new ArgumentException("Data cannot be null or empty", nameof(data));

            P2PDeviceContext context = null;

            lock (_syncLock)
            {
                if (!_activeDevices.TryGetValue(deviceId, out context))
                {
                    _logger.LogError($"Device {deviceId} not found");
                    return false;
                }
            }

            try
            {
                if (context.ConnectionState != P2PConnectionState.TunnelEstablished)
                {
                    _logger.LogError($"Device {deviceId} tunnel is not established. Current state: {context.ConnectionState}");
                    return false;
                }

                // Prepare data (encryption if enabled)
                byte[] sendData = data;
                if (_configuration.EnableEncryption && context.Credentials.IsEncrypted)
                {
                    sendData = EncryptData(data, context.Credentials.Token);
                }

                // Send through tunnel
                if (context.TunnelStream != null && context.TunnelStream.CanWrite)
                {
                    await context.TunnelStream.WriteAsync(sendData, 0, sendData.Length, context.CancellationTokenSource.Token);
                    await context.TunnelStream.FlushAsync();
                    return true;
                }

                _logger.LogError($"Tunnel stream for device {deviceId} is not available");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending data to device {deviceId}");
                RaiseConnectionError(deviceId, "Send data failed", ex);
                return false;
            }
        }

        /// <summary>
        /// Receive data from P2P tunnel
        /// </summary>
        public async Task<byte[]> ReceiveDataAsync(string deviceId, int timeoutMs = 5000)
        {
            if (string.IsNullOrWhiteSpace(deviceId))
                throw new ArgumentException("Device ID cannot be null or empty", nameof(deviceId));

            P2PDeviceContext context = null;

            lock (_syncLock)
            {
                if (!_activeDevices.TryGetValue(deviceId, out context))
                {
                    _logger.LogError($"Device {deviceId} not found");
                    return null;
                }
            }

            try
            {
                if (context.ConnectionState != P2PConnectionState.TunnelEstablished)
                {
                    _logger.LogError($"Device {deviceId} tunnel is not established");
                    return null;
                }

                if (context.TunnelStream == null || !context.TunnelStream.CanRead)
                {
                    _logger.LogError($"Tunnel stream for device {deviceId} is not available");
                    return null;
                }

                using (var cts = CancellationTokenSource.CreateLinkedTokenSource(context.CancellationTokenSource.Token))
                {
                    cts.CancelAfter(timeoutMs);

                    byte[] buffer = new byte[65536];
                    int bytesRead = await context.TunnelStream.ReadAsync(buffer, 0, buffer.Length, cts.Token);

                    if (bytesRead > 0)
                    {
                        Array.Resize(ref buffer, bytesRead);

                        // Decrypt if enabled
                        if (_configuration.EnableEncryption && context.Credentials.IsEncrypted)
                        {
                            buffer = DecryptData(buffer, context.Credentials.Token);
                        }

                        RaiseDataReceived(deviceId, buffer);
                        return buffer;
                    }

                    return null;
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning($"Receive operation timeout for device {deviceId}");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error receiving data from device {deviceId}");
                RaiseConnectionError(deviceId, "Receive data failed", ex);
                return null;
            }
        }

        /// <summary>
        /// Disconnect device
        /// </summary>
        public async Task<bool> DisconnectAsync(string deviceId)
        {
            if (string.IsNullOrWhiteSpace(deviceId))
                throw new ArgumentException("Device ID cannot be null or empty", nameof(deviceId));

            P2PDeviceContext context = null;

            lock (_syncLock)
            {
                if (!_activeDevices.TryGetValue(deviceId, out context))
                {
                    _logger.LogWarning($"Device {deviceId} not found in active devices");
                    return false;
                }
            }

            try
            {
                // Cancel any ongoing operations
                context.CancellationTokenSource?.Cancel();

                // Close tunnel stream
                context.TunnelStream?.Dispose();

                // Update state
                context.ConnectionState = P2PConnectionState.Disconnected;

                lock (_syncLock)
                {
                    _activeDevices.Remove(deviceId);
                }

                _logger.LogInformation($"Device {deviceId} disconnected");
                RaiseConnectionStateChanged(deviceId, P2PConnectionState.Disconnected, "Device disconnected");

                // Delete credentials
                await _credentialsStorage.DeleteCredentialsAsync(deviceId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error disconnecting device {deviceId}");
                return false;
            }
        }

        /// <summary>
        /// Get device connection status
        /// </summary>
        public P2PConnectionState GetDeviceStatus(string deviceId)
        {
            lock (_syncLock)
            {
                if (_activeDevices.TryGetValue(deviceId, out var context))
                {
                    return context.ConnectionState;
                }
            }

            return P2PConnectionState.Disconnected;
        }

        /// <summary>
        /// Get all active devices
        /// </summary>
        public IReadOnlyList<string> GetActiveDevices()
        {
            lock (_syncLock)
            {
                return _activeDevices.Keys.ToList().AsReadOnly();
            }
        }

        /// <summary>
        /// Private helper methods
        /// </summary>

        private async Task<bool> AttemptConnectionAsync(P2PDeviceContext context)
        {
            try
            {
                using (var client = new TcpClient())
                {
                    var connectTask = client.ConnectAsync(context.DeviceIp, context.DevicePort);
                    var completedTask = await Task.WhenAny(
                        connectTask,
                        Task.Delay(_configuration.ConnectionTimeoutMs)
                    );

                    if (completedTask != connectTask)
                    {
                        _logger.LogError($"Connection timeout for device {context.DeviceId}");
                        return false;
                    }

                    context.TunnelStream = client.GetStream();
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Connection attempt failed for device {context.DeviceId}");
                return false;
            }
        }

        private async Task<bool> PerformAuthenticationAsync(P2PDeviceContext context)
        {
            try
            {
                // Simulate authentication handshake
                string authData = $"{context.Credentials.Username}:{context.Credentials.Password}";
                byte[] authBytes = Encoding.UTF8.GetBytes(authData);
                byte[] hashedAuth = ComputeHash(authBytes);

                await context.TunnelStream.WriteAsync(hashedAuth, 0, hashedAuth.Length);
                await context.TunnelStream.FlushAsync();

                // Wait for authentication response
                byte[] responseBuffer = new byte[256];
                int bytesRead = await context.TunnelStream.ReadAsync(responseBuffer, 0, responseBuffer.Length);

                return bytesRead > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Authentication handshake failed for device {context.DeviceId}");
                return false;
            }
        }

        private async Task<bool> InitializeTunnelAsync(P2PDeviceContext context)
        {
            try
            {
                // Simulate tunnel initialization
                string tunnelInit = $"TUNNEL_INIT:{context.TunnelId}";
                byte[] initBytes = Encoding.UTF8.GetBytes(tunnelInit);

                await context.TunnelStream.WriteAsync(initBytes, 0, initBytes.Length);
                await context.TunnelStream.FlushAsync();

                byte[] responseBuffer = new byte[256];
                int bytesRead = await context.TunnelStream.ReadAsync(responseBuffer, 0, responseBuffer.Length);

                return bytesRead > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Tunnel initialization failed for device {context.DeviceId}");
                return false;
            }
        }

        private async Task KeepAliveTaskAsync(P2PDeviceContext context)
        {
            try
            {
                while (!context.CancellationTokenSource.Token.IsCancellationRequested)
                {
                    await Task.Delay(_configuration.KeepAliveIntervalMs, context.CancellationTokenSource.Token);

                    if (context.ConnectionState == P2PConnectionState.TunnelEstablished)
                    {
                        string keepAlive = "KEEP_ALIVE";
                        byte[] keepAliveBytes = Encoding.UTF8.GetBytes(keepAlive);

                        await context.TunnelStream.WriteAsync(keepAliveBytes, 0, keepAliveBytes.Length);
                        await context.TunnelStream.FlushAsync();
                        context.LastKeepAlive = DateTime.UtcNow;
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Expected when cancellation is requested
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Keep-alive task failed for device {context.DeviceId}");
            }
        }

        private byte[] EncryptData(byte[] data, string key)
        {
            try
            {
                using (var aes = Aes.Create())
                {
                    aes.Key = ComputeHash(Encoding.UTF8.GetBytes(key)).Take(32).ToArray();
                    aes.IV = new byte[16];

                    using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                    {
                        byte[] encryptedData = encryptor.TransformFinalBlock(data, 0, data.Length);
                        return encryptedData;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Data encryption failed");
                return data;
            }
        }

        private byte[] DecryptData(byte[] data, string key)
        {
            try
            {
                using (var aes = Aes.Create())
                {
                    aes.Key = ComputeHash(Encoding.UTF8.GetBytes(key)).Take(32).ToArray();
                    aes.IV = new byte[16];

                    using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
                    {
                        byte[] decryptedData = decryptor.TransformFinalBlock(data, 0, data.Length);
                        return decryptedData;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Data decryption failed");
                return data;
            }
        }

        private byte[] ComputeHash(byte[] data)
        {
            using (var sha256 = SHA256.Create())
            {
                return sha256.ComputeHash(data);
            }
        }

        private string GenerateTunnelId()
        {
            return Guid.NewGuid().ToString("N").Substring(0, 16);
        }

        private void RaiseConnectionStateChanged(string deviceId, P2PConnectionState state, string message)
        {
            ConnectionStateChanged?.Invoke(this, new P2PTunnelEventArgs
            {
                DeviceId = deviceId,
                State = state,
                Message = message
            });
        }

        private void RaiseTunnelEstablished(string deviceId, string tunnelId)
        {
            TunnelEstablished?.Invoke(this, new P2PTunnelEventArgs
            {
                DeviceId = deviceId,
                Message = $"Tunnel {tunnelId} established"
            });
        }

        private void RaiseConnectionError(string deviceId, string message, Exception ex)
        {
            ConnectionError?.Invoke(this, new P2PTunnelEventArgs
            {
                DeviceId = deviceId,
                Message = message,
                Exception = ex
            });
        }

        private void RaiseDataReceived(string deviceId, byte[] data)
        {
            DataReceived?.Invoke(this, new P2PTunnelEventArgs
            {
                DeviceId = deviceId,
                Message = $"Data received: {data.Length} bytes"
            });
        }

        /// <summary>
        /// Dispose resources
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
                return;

            lock (_syncLock)
            {
                foreach (var device in _activeDevices.Values)
                {
                    device.CancellationTokenSource?.Cancel();
                    device.TunnelStream?.Dispose();
                    device.CancellationTokenSource?.Dispose();
                }

                _activeDevices.Clear();
            }

            _disposed = true;
            _logger.LogInformation("IntelbrasP2P disposed");
        }
    }
}
