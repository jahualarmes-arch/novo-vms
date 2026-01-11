using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
using VMS_AlarmesJahu.App.Data;
using VMS_AlarmesJahu.App.Models;
using VMS_AlarmesJahu.App.Sdk;
using VMS_AlarmesJahu.App.Services.P2P;

namespace VMS_AlarmesJahu.App.Services;

public class ConnectionManager : IDisposable
{
    private readonly ConcurrentDictionary<long, DeviceConnection> _connections = new();
    private readonly DeviceRepository _deviceRepo;
    private readonly Timer _healthCheckTimer;
    private bool _disposed;

    public event Action<long, DeviceStatus>? DeviceStatusChanged;

    public ConnectionManager(DeviceRepository deviceRepo)
    {
        _deviceRepo = deviceRepo;
        _healthCheckTimer = new Timer(HealthCheck, null, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(30));
    }

    public IntPtr GetLogin(long deviceId)
    {
        if (_connections.TryGetValue(deviceId, out var conn) && conn.LoginHandle != IntPtr.Zero)
            return conn.LoginHandle;

        return Connect(deviceId);
    }

    public IntPtr Connect(long deviceId)
    {
        var device = _deviceRepo.GetById(deviceId);
        if (device == null || !device.Enabled)
            return IntPtr.Zero;

        try
        {
            UpdateStatus(deviceId, DeviceStatus.Connecting);
            
            var password = device.GetPassword();
            IntPtr login;
            int channelCount;
            int? p2pLocalPort = null;

            // Escolhe o método de login baseado no tipo de conexão
            if (device.ConnectionType == ConnectionType.P2PCloud)
            {
                if (string.IsNullOrWhiteSpace(device.SerialNumber))
                {
                    Log.Error("Número de série não configurado para {Device}", device.Name);
                    UpdateStatus(deviceId, DeviceStatus.Error);
                    return IntPtr.Zero;
                }

                Log.Information("Conectando via P2P Cloud: {Device} (SN: {SerialNumber}, Porta: {Port})", 
                    device.Name, device.SerialNumber, device.Port);
                
                // Abrir túnel P2P
                p2pLocalPort = P2PTunnelManager.Instance.OpenTunnel(device.SerialNumber, device.Port);
                if (p2pLocalPort == -1)
                {
                    Log.Error("Falha ao abrir túnel P2P para {Device} ({Serial}). Dispositivo pode estar offline ou indisponível.", 
                        device.Name, device.SerialNumber);
                    UpdateStatus(deviceId, DeviceStatus.Error);
                    return IntPtr.Zero;
                }

                Log.Information("Túnel P2P aberto para {Device}: usando 127.0.0.1:{LocalPort}", 
                    device.Name, p2pLocalPort.Value);

                // Fazer login via IP local (127.0.0.1)
                login = IntelbrasSdk.Login("127.0.0.1", p2pLocalPort.Value, device.User, password, out channelCount);
                
                if (login == IntPtr.Zero)
                {
                    Log.Error("Falha ao fazer login no dispositivo {Device} via túnel P2P (127.0.0.1:{LocalPort})", 
                        device.Name, p2pLocalPort.Value);
                    
                    // Fechar túnel em caso de falha de login
                    P2PTunnelManager.Instance.CloseTunnel(device.SerialNumber);
                    
                    UpdateStatus(deviceId, DeviceStatus.Error);
                    return IntPtr.Zero;
                }
            }
            else
            {
                if (string.IsNullOrWhiteSpace(device.Host))
                {
                    Log.Error("Host/IP não configurado para {Device}", device.Name);
                    UpdateStatus(deviceId, DeviceStatus.Error);
                    return IntPtr.Zero;
                }

                Log.Information("Conectando via IP direto: {Device} ({Host}:{Port})", 
                    device.Name, device.Host, device.Port);
                login = IntelbrasSdk.Login(device.Host, device.Port, device.User, password, out channelCount);
                
                if (login == IntPtr.Zero)
                {
                    UpdateStatus(deviceId, DeviceStatus.Error);
                    Log.Warning("Falha ao conectar em {Device} ({ConnectionInfo})", 
                        device.Name, device.GetConnectionInfo());
                    return IntPtr.Zero;
                }
            }

            var conn = new DeviceConnection
            {
                DeviceId = deviceId,
                LoginHandle = login,
                ChannelCount = channelCount > 0 ? channelCount : device.ChannelCount,
                ConnectedAt = DateTime.Now,
                P2PLocalPort = p2pLocalPort
            };

            _connections[deviceId] = conn;
            _deviceRepo.UpdateLastConnected(deviceId);
            UpdateStatus(deviceId, DeviceStatus.Connected);
            
            Log.Information("Conectado em {Device} ({ConnectionInfo}) - {Channels} canais", 
                device.Name, device.GetConnectionInfo(), conn.ChannelCount);

            return login;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Erro ao conectar em {DeviceId}", deviceId);
            UpdateStatus(deviceId, DeviceStatus.Error);
            return IntPtr.Zero;
        }
    }

    public void Disconnect(long deviceId)
    {
        var device = _deviceRepo.GetById(deviceId);
        
        if (_connections.TryRemove(deviceId, out var conn))
        {
            try
            {
                if (conn.LoginHandle != IntPtr.Zero)
                {
                    // Stop all play handles first
                    foreach (var play in conn.PlayHandles.Values.ToList())
                    {
                        IntelbrasSdk.StopRealPlay(play);
                    }
                    conn.PlayHandles.Clear();
                    
                    IntelbrasSdk.Logout(conn.LoginHandle);
                }

                // Fechar túnel P2P se foi usado
                if (device != null && device.ConnectionType == ConnectionType.P2PCloud && 
                    !string.IsNullOrWhiteSpace(device.SerialNumber))
                {
                    P2PTunnelManager.Instance.CloseTunnel(device.SerialNumber);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Erro ao desconectar {DeviceId}", deviceId);
            }
            finally
            {
                UpdateStatus(deviceId, DeviceStatus.Disconnected);
            }
        }
    }

    public IntPtr StartPlay(long deviceId, int channel, IntPtr windowHandle)
    {
        var login = GetLogin(deviceId);
        if (login == IntPtr.Zero) return IntPtr.Zero;

        try
        {
            var playHandle = IntelbrasSdk.RealPlay(login, channel, windowHandle, 0);
            if (playHandle != IntPtr.Zero && _connections.TryGetValue(deviceId, out var conn))
            {
                conn.PlayHandles[channel] = playHandle;
            }
            return playHandle;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Erro ao iniciar play {DeviceId} CH{Channel}", deviceId, channel);
            return IntPtr.Zero;
        }
    }

    public void StopPlay(long deviceId, int channel)
    {
        if (_connections.TryGetValue(deviceId, out var conn))
        {
            if (conn.PlayHandles.TryRemove(channel, out var playHandle))
            {
                try
                {
                    IntelbrasSdk.StopRealPlay(playHandle);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Erro ao parar play {DeviceId} CH{Channel}", deviceId, channel);
                }
            }
        }
    }

    public IntPtr GetPlayHandle(long deviceId, int channel)
    {
        if (_connections.TryGetValue(deviceId, out var conn))
        {
            return conn.PlayHandles.TryGetValue(channel, out var handle) ? handle : IntPtr.Zero;
        }
        return IntPtr.Zero;
    }

    public DeviceStatus GetStatus(long deviceId)
    {
        if (_connections.TryGetValue(deviceId, out var conn) && conn.LoginHandle != IntPtr.Zero)
            return DeviceStatus.Connected;
        return DeviceStatus.Disconnected;
    }

    public int GetConnectedCount()
    {
        return _connections.Count(c => c.Value.LoginHandle != IntPtr.Zero);
    }

    public int GetChannelCount(long deviceId)
    {
        if (_connections.TryGetValue(deviceId, out var conn))
            return conn.ChannelCount;
        
        var device = _deviceRepo.GetById(deviceId);
        return device?.ChannelCount ?? 16;
    }

    public List<(long DeviceId, int Channel, IntPtr PlayHandle)> GetAllActivePlays()
    {
        var result = new List<(long, int, IntPtr)>();
        foreach (var conn in _connections.Values)
        {
            foreach (var (channel, handle) in conn.PlayHandles)
            {
                if (handle != IntPtr.Zero)
                    result.Add((conn.DeviceId, channel, handle));
            }
        }
        return result;
    }

    private void HealthCheck(object? state)
    {
        foreach (var deviceId in _connections.Keys.ToList())
        {
            if (_connections.TryGetValue(deviceId, out var conn))
            {
                // Simple health check - try to verify connection is still valid
                // In a real implementation, you might send a keep-alive command
                if (conn.LoginHandle == IntPtr.Zero)
                {
                    Log.Warning("Conexão perdida com device {DeviceId}, tentando reconectar...", deviceId);
                    Connect(deviceId);
                }
            }
        }
    }

    private void UpdateStatus(long deviceId, DeviceStatus status)
    {
        DeviceStatusChanged?.Invoke(deviceId, status);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        _healthCheckTimer.Dispose();
        
        foreach (var deviceId in _connections.Keys.ToList())
        {
            Disconnect(deviceId);
        }
        
        IntelbrasSdk.CLIENT_Cleanup();
    }
}

public class DeviceConnection
{
    public long DeviceId { get; set; }
    public IntPtr LoginHandle { get; set; }
    public int ChannelCount { get; set; }
    public DateTime ConnectedAt { get; set; }
    public int? P2PLocalPort { get; set; }
    public ConcurrentDictionary<int, IntPtr> PlayHandles { get; } = new();
}
