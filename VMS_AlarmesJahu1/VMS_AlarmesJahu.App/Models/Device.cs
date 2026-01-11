using System;
using System.Security.Cryptography;
using System.Text;

namespace VMS_AlarmesJahu.App.Models;

public class Device
{
    public long Id { get; set; }
    public string Name { get; set; } = "";
    
    // Campos para conexão direta (IP/Porta)
    public string Host { get; set; } = "";
    public int Port { get; set; } = 37777;
    
    // Campos para conexão P2P Cloud (Número de Série)
    public string SerialNumber { get; set; } = "";
    public ConnectionType ConnectionType { get; set; } = ConnectionType.Direct;
    
    public string User { get; set; } = "admin";
    public string PasswordHash { get; set; } = "";
    public int ChannelCount { get; set; } = 16;
    public bool Enabled { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? LastConnectedAt { get; set; }
    public DeviceStatus Status { get; set; } = DeviceStatus.Disconnected;

    public void SetPassword(string plainPassword)
    {
        var bytes = Encoding.UTF8.GetBytes(plainPassword);
        PasswordHash = Convert.ToBase64String(ProtectedData.Protect(bytes, null, DataProtectionScope.CurrentUser));
    }

    public string GetPassword()
    {
        if (string.IsNullOrEmpty(PasswordHash)) return "";
        try
        {
            var protectedBytes = Convert.FromBase64String(PasswordHash);
            var bytes = ProtectedData.Unprotect(protectedBytes, null, DataProtectionScope.CurrentUser);
            return Encoding.UTF8.GetString(bytes);
        }
        catch { return ""; }
    }

    /// <summary>
    /// Retorna informações de conexão formatadas (mostra porta também no P2P)
    /// </summary>
    public string GetConnectionInfo()
    {
        return ConnectionType == ConnectionType.P2PCloud
            ? $"P2P: {SerialNumber} (Porta: {Port})"
            : $"{Host}:{Port}";
    }
}

public enum ConnectionType
{
    Direct = 0,      // Conexão direta via IP:Porta
    P2PCloud = 1     // Conexão via Cloud P2P usando número de série
}

public enum DeviceStatus
{
    Disconnected,
    Connecting,
    Connected,
    Error
}
