using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using VMS_AlarmesJahu.App.Models;

namespace VMS_AlarmesJahu.App.Data;

public class AiEventRepository
{
    public List<AiEvent> GetRecent(int limit = 100)
    {
        var list = new List<AiEvent>();
        using var conn = Database.GetConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT * FROM ai_events ORDER BY timestamp DESC LIMIT @limit";
        cmd.Parameters.AddWithValue("@limit", limit);
        
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            list.Add(MapEvent(reader));
        }
        return list;
    }

    public List<AiEvent> GetByDateRange(DateTime start, DateTime end, long? deviceId = null, int? channel = null)
    {
        var list = new List<AiEvent>();
        using var conn = Database.GetConnection();
        using var cmd = conn.CreateCommand();
        
        var sql = "SELECT * FROM ai_events WHERE datetime(timestamp) BETWEEN @start AND @end";
        if (deviceId.HasValue) sql += " AND device_id = @deviceId";
        if (channel.HasValue) sql += " AND channel = @channel";
        sql += " ORDER BY timestamp DESC";
        
        cmd.CommandText = sql;
        cmd.Parameters.AddWithValue("@start", start.ToString("o"));
        cmd.Parameters.AddWithValue("@end", end.ToString("o"));
        if (deviceId.HasValue) cmd.Parameters.AddWithValue("@deviceId", deviceId.Value);
        if (channel.HasValue) cmd.Parameters.AddWithValue("@channel", channel.Value);
        
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            list.Add(MapEvent(reader));
        }
        return list;
    }

    public long Insert(AiEvent ev)
    {
        using var conn = Database.GetConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO ai_events (device_id, device_name, channel, rule_id, rule_name, class_name, confidence, event_type, timestamp, snapshot_path, clip_path)
            VALUES (@deviceId, @deviceName, @channel, @ruleId, @ruleName, @className, @conf, @type, @timestamp, @snapshot, @clip);
            SELECT last_insert_rowid();";
        
        cmd.Parameters.AddWithValue("@deviceId", ev.DeviceId);
        cmd.Parameters.AddWithValue("@deviceName", ev.DeviceName ?? "");
        cmd.Parameters.AddWithValue("@channel", ev.Channel);
        cmd.Parameters.AddWithValue("@ruleId", ev.RuleId.HasValue ? ev.RuleId.Value : DBNull.Value);
        cmd.Parameters.AddWithValue("@ruleName", ev.RuleName ?? "");
        cmd.Parameters.AddWithValue("@className", ev.ClassName ?? "");
        cmd.Parameters.AddWithValue("@conf", ev.Confidence);
        cmd.Parameters.AddWithValue("@type", ev.EventType ?? "");
        cmd.Parameters.AddWithValue("@timestamp", ev.Timestamp.ToString("o"));
        cmd.Parameters.AddWithValue("@snapshot", ev.SnapshotPath ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@clip", ev.ClipPath ?? (object)DBNull.Value);
        
        return (long)cmd.ExecuteScalar()!;
    }

    public void Acknowledge(long id, string? acknowledgedBy = null, string? notes = null)
    {
        using var conn = Database.GetConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            UPDATE ai_events SET 
                acknowledged = 1, acknowledged_by = @by, acknowledged_at = @at, notes = @notes
            WHERE id = @id";
        
        cmd.Parameters.AddWithValue("@id", id);
        cmd.Parameters.AddWithValue("@by", acknowledgedBy ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@at", DateTime.Now.ToString("o"));
        cmd.Parameters.AddWithValue("@notes", notes ?? (object)DBNull.Value);
        
        cmd.ExecuteNonQuery();
    }

    public int CountToday()
    {
        using var conn = Database.GetConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM ai_events WHERE date(timestamp) = date('now', 'localtime')";
        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    public int CountThisWeek()
    {
        using var conn = Database.GetConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM ai_events WHERE datetime(timestamp) >= datetime('now', '-7 days')";
        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    public int CountThisMonth()
    {
        using var conn = Database.GetConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM ai_events WHERE datetime(timestamp) >= datetime('now', '-30 days')";
        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    public List<HourlyCount> GetHourlyCountsToday()
    {
        var list = new List<HourlyCount>();
        using var conn = Database.GetConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT strftime('%H', timestamp) as hour, COUNT(*) as count 
            FROM ai_events 
            WHERE date(timestamp) = date('now', 'localtime')
            GROUP BY hour ORDER BY hour";
        
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            list.Add(new HourlyCount
            {
                Hour = int.Parse(reader.GetString(0)),
                Count = reader.GetInt32(1)
            });
        }
        return list;
    }

    public List<ChannelCount> GetTopChannels(int limit = 5)
    {
        var list = new List<ChannelCount>();
        using var conn = Database.GetConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT device_name, channel, COUNT(*) as count 
            FROM ai_events 
            WHERE datetime(timestamp) >= datetime('now', '-7 days')
            GROUP BY device_id, channel 
            ORDER BY count DESC LIMIT @limit";
        cmd.Parameters.AddWithValue("@limit", limit);
        
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            list.Add(new ChannelCount
            {
                DeviceName = reader.IsDBNull(0) ? "DVR" : reader.GetString(0),
                Channel = reader.GetInt32(1),
                Count = reader.GetInt32(2)
            });
        }
        return list;
    }

    public List<ClassCount> GetTopClasses(int limit = 5)
    {
        var list = new List<ClassCount>();
        using var conn = Database.GetConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT class_name, COUNT(*) as count 
            FROM ai_events 
            WHERE datetime(timestamp) >= datetime('now', '-7 days')
            GROUP BY class_name 
            ORDER BY count DESC LIMIT @limit";
        cmd.Parameters.AddWithValue("@limit", limit);
        
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            list.Add(new ClassCount
            {
                ClassName = reader.IsDBNull(0) ? "unknown" : reader.GetString(0),
                Count = reader.GetInt32(1)
            });
        }
        return list;
    }

    public DateTime? GetLastEventTime()
    {
        using var conn = Database.GetConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT MAX(timestamp) FROM ai_events";
        var result = cmd.ExecuteScalar();
        if (result == DBNull.Value || result == null) return null;
        return DateTime.TryParse(result.ToString(), out var dt) ? dt : null;
    }

    private static AiEvent MapEvent(SqliteDataReader reader)
    {
        return new AiEvent
        {
            Id = reader.GetInt64(reader.GetOrdinal("id")),
            DeviceId = reader.GetInt64(reader.GetOrdinal("device_id")),
            DeviceName = reader.IsDBNull(reader.GetOrdinal("device_name")) ? "" : reader.GetString(reader.GetOrdinal("device_name")),
            Channel = reader.GetInt32(reader.GetOrdinal("channel")),
            RuleId = reader.IsDBNull(reader.GetOrdinal("rule_id")) ? null : reader.GetInt64(reader.GetOrdinal("rule_id")),
            RuleName = reader.IsDBNull(reader.GetOrdinal("rule_name")) ? "" : reader.GetString(reader.GetOrdinal("rule_name")),
            ClassName = reader.IsDBNull(reader.GetOrdinal("class_name")) ? "" : reader.GetString(reader.GetOrdinal("class_name")),
            Confidence = reader.IsDBNull(reader.GetOrdinal("confidence")) ? 0 : reader.GetDouble(reader.GetOrdinal("confidence")),
            EventType = reader.IsDBNull(reader.GetOrdinal("event_type")) ? "" : reader.GetString(reader.GetOrdinal("event_type")),
            Timestamp = DateTime.TryParse(reader.GetString(reader.GetOrdinal("timestamp")), out var ts) ? ts : DateTime.Now,
            SnapshotPath = reader.IsDBNull(reader.GetOrdinal("snapshot_path")) ? null : reader.GetString(reader.GetOrdinal("snapshot_path")),
            ClipPath = reader.IsDBNull(reader.GetOrdinal("clip_path")) ? null : reader.GetString(reader.GetOrdinal("clip_path")),
            Acknowledged = reader.GetInt32(reader.GetOrdinal("acknowledged")) == 1,
            AcknowledgedBy = reader.IsDBNull(reader.GetOrdinal("acknowledged_by")) ? null : reader.GetString(reader.GetOrdinal("acknowledged_by")),
            AcknowledgedAt = reader.IsDBNull(reader.GetOrdinal("acknowledged_at")) ? null : 
                DateTime.TryParse(reader.GetString(reader.GetOrdinal("acknowledged_at")), out var ack) ? ack : null,
            Notes = reader.IsDBNull(reader.GetOrdinal("notes")) ? null : reader.GetString(reader.GetOrdinal("notes"))
        };
    }
}
