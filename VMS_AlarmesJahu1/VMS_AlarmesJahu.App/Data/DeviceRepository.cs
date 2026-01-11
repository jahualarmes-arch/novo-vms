using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using VMS_AlarmesJahu.App.Models;

namespace VMS_AlarmesJahu.App.Data;

public class DeviceRepository
{
    public List<Device> GetAll()
    {
        var list = new List<Device>();
        using var conn = Database.GetConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT * FROM devices ORDER BY name";
        
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            list.Add(MapDevice(reader));
        }
        return list;
    }

    public Device? GetById(long id)
    {
        using var conn = Database.GetConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT * FROM devices WHERE id = @id";
        cmd.Parameters.AddWithValue("@id", id);
        
        using var reader = cmd.ExecuteReader();
        return reader.Read() ? MapDevice(reader) : null;
    }

    public long Insert(Device device)
    {
        using var conn = Database.GetConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO devices (name, host, port, serial_number, connection_type, user, password_hash, channel_count, enabled)
            VALUES (@name, @host, @port, @serial, @conntype, @user, @pass, @channels, @enabled);
            SELECT last_insert_rowid();";
        
        cmd.Parameters.AddWithValue("@name", device.Name);
        cmd.Parameters.AddWithValue("@host", device.Host);
        cmd.Parameters.AddWithValue("@port", device.Port);
        cmd.Parameters.AddWithValue("@serial", device.SerialNumber ?? "");
        cmd.Parameters.AddWithValue("@conntype", (int)device.ConnectionType);
        cmd.Parameters.AddWithValue("@user", device.User);
        cmd.Parameters.AddWithValue("@pass", device.PasswordHash);
        cmd.Parameters.AddWithValue("@channels", device.ChannelCount);
        cmd.Parameters.AddWithValue("@enabled", device.Enabled ? 1 : 0);
        
        return (long)cmd.ExecuteScalar()!;
    }

    public void Update(Device device)
    {
        using var conn = Database.GetConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            UPDATE devices SET 
                name = @name, host = @host, port = @port,
                serial_number = @serial, connection_type = @conntype,
                user = @user, password_hash = @pass, 
                channel_count = @channels, enabled = @enabled
            WHERE id = @id";
        
        cmd.Parameters.AddWithValue("@id", device.Id);
        cmd.Parameters.AddWithValue("@name", device.Name);
        cmd.Parameters.AddWithValue("@host", device.Host);
        cmd.Parameters.AddWithValue("@port", device.Port);
        cmd.Parameters.AddWithValue("@serial", device.SerialNumber ?? "");
        cmd.Parameters.AddWithValue("@conntype", (int)device.ConnectionType);
        cmd.Parameters.AddWithValue("@user", device.User);
        cmd.Parameters.AddWithValue("@pass", device.PasswordHash);
        cmd.Parameters.AddWithValue("@channels", device.ChannelCount);
        cmd.Parameters.AddWithValue("@enabled", device.Enabled ? 1 : 0);
        
        cmd.ExecuteNonQuery();
    }

    public void UpdateLastConnected(long id)
    {
        using var conn = Database.GetConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "UPDATE devices SET last_connected_at = @now WHERE id = @id";
        cmd.Parameters.AddWithValue("@id", id);
        cmd.Parameters.AddWithValue("@now", DateTime.Now.ToString("o"));
        cmd.ExecuteNonQuery();
    }

    public void Delete(long id)
    {
        using var conn = Database.GetConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM devices WHERE id = @id";
        cmd.Parameters.AddWithValue("@id", id);
        cmd.ExecuteNonQuery();
    }

    public int Count()
    {
        using var conn = Database.GetConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM devices";
        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    private static Device MapDevice(SqliteDataReader reader)
    {
        return new Device
        {
            Id = reader.GetInt64(reader.GetOrdinal("id")),
            Name = reader.GetString(reader.GetOrdinal("name")),
            Host = reader.GetString(reader.GetOrdinal("host")),
            Port = reader.GetInt32(reader.GetOrdinal("port")),
            SerialNumber = reader.IsDBNull(reader.GetOrdinal("serial_number")) ? "" : reader.GetString(reader.GetOrdinal("serial_number")),
            ConnectionType = (ConnectionType)(reader.IsDBNull(reader.GetOrdinal("connection_type")) ? 0 : reader.GetInt32(reader.GetOrdinal("connection_type"))),
            User = reader.GetString(reader.GetOrdinal("user")),
            PasswordHash = reader.IsDBNull(reader.GetOrdinal("password_hash")) ? "" : reader.GetString(reader.GetOrdinal("password_hash")),
            ChannelCount = reader.GetInt32(reader.GetOrdinal("channel_count")),
            Enabled = reader.GetInt32(reader.GetOrdinal("enabled")) == 1,
            CreatedAt = DateTime.TryParse(reader.GetString(reader.GetOrdinal("created_at")), out var ca) ? ca : DateTime.Now,
            LastConnectedAt = reader.IsDBNull(reader.GetOrdinal("last_connected_at")) ? null : 
                DateTime.TryParse(reader.GetString(reader.GetOrdinal("last_connected_at")), out var lc) ? lc : null
        };
    }
}
