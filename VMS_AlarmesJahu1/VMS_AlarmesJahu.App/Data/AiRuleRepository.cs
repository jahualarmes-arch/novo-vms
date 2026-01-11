using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using VMS_AlarmesJahu.App.Models;

namespace VMS_AlarmesJahu.App.Data;

public class AiRuleRepository
{
    public List<AiRule> GetAll()
    {
        var list = new List<AiRule>();
        using var conn = Database.GetConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT * FROM ai_rules ORDER BY device_id, channel, name";
        
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            list.Add(MapRule(reader));
        }
        return list;
    }

    public List<AiRule> GetByDevice(long deviceId)
    {
        var list = new List<AiRule>();
        using var conn = Database.GetConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT * FROM ai_rules WHERE device_id = @deviceId ORDER BY channel, name";
        cmd.Parameters.AddWithValue("@deviceId", deviceId);
        
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            list.Add(MapRule(reader));
        }
        return list;
    }

    public List<AiRule> GetByDeviceAndChannel(long deviceId, int channel)
    {
        var list = new List<AiRule>();
        using var conn = Database.GetConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT * FROM ai_rules WHERE device_id = @deviceId AND channel = @channel";
        cmd.Parameters.AddWithValue("@deviceId", deviceId);
        cmd.Parameters.AddWithValue("@channel", channel);
        
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            list.Add(MapRule(reader));
        }
        return list;
    }

    public List<AiRule> GetEnabledRules()
    {
        var list = new List<AiRule>();
        using var conn = Database.GetConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT * FROM ai_rules WHERE enabled = 1";
        
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            list.Add(MapRule(reader));
        }
        return list;
    }

    public AiRule? GetById(long id)
    {
        using var conn = Database.GetConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT * FROM ai_rules WHERE id = @id";
        cmd.Parameters.AddWithValue("@id", id);
        
        using var reader = cmd.ExecuteReader();
        return reader.Read() ? MapRule(reader) : null;
    }

    public long Insert(AiRule rule)
    {
        using var conn = Database.GetConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO ai_rules (device_id, channel, name, type, points_json, classes_json, direction, confidence, cooldown_sec, enabled, color)
            VALUES (@deviceId, @channel, @name, @type, @points, @classes, @direction, @conf, @cooldown, @enabled, @color);
            SELECT last_insert_rowid();";
        
        cmd.Parameters.AddWithValue("@deviceId", rule.DeviceId);
        cmd.Parameters.AddWithValue("@channel", rule.Channel);
        cmd.Parameters.AddWithValue("@name", rule.Name);
        cmd.Parameters.AddWithValue("@type", rule.Type.ToString());
        cmd.Parameters.AddWithValue("@points", rule.PointsJson);
        cmd.Parameters.AddWithValue("@classes", rule.ClassesJson);
        cmd.Parameters.AddWithValue("@direction", rule.Direction.ToString());
        cmd.Parameters.AddWithValue("@conf", rule.Confidence);
        cmd.Parameters.AddWithValue("@cooldown", rule.CooldownSec);
        cmd.Parameters.AddWithValue("@enabled", rule.Enabled ? 1 : 0);
        cmd.Parameters.AddWithValue("@color", rule.Color);
        
        return (long)cmd.ExecuteScalar()!;
    }

    public void Update(AiRule rule)
    {
        using var conn = Database.GetConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            UPDATE ai_rules SET 
                name = @name, type = @type, points_json = @points, classes_json = @classes,
                direction = @direction, confidence = @conf, cooldown_sec = @cooldown, 
                enabled = @enabled, color = @color
            WHERE id = @id";
        
        cmd.Parameters.AddWithValue("@id", rule.Id);
        cmd.Parameters.AddWithValue("@name", rule.Name);
        cmd.Parameters.AddWithValue("@type", rule.Type.ToString());
        cmd.Parameters.AddWithValue("@points", rule.PointsJson);
        cmd.Parameters.AddWithValue("@classes", rule.ClassesJson);
        cmd.Parameters.AddWithValue("@direction", rule.Direction.ToString());
        cmd.Parameters.AddWithValue("@conf", rule.Confidence);
        cmd.Parameters.AddWithValue("@cooldown", rule.CooldownSec);
        cmd.Parameters.AddWithValue("@enabled", rule.Enabled ? 1 : 0);
        cmd.Parameters.AddWithValue("@color", rule.Color);
        
        cmd.ExecuteNonQuery();
    }

    public void Delete(long id)
    {
        using var conn = Database.GetConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM ai_rules WHERE id = @id";
        cmd.Parameters.AddWithValue("@id", id);
        cmd.ExecuteNonQuery();
    }

    public int CountEnabled()
    {
        using var conn = Database.GetConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM ai_rules WHERE enabled = 1";
        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    private static AiRule MapRule(SqliteDataReader reader)
    {
        var rule = new AiRule
        {
            Id = reader.GetInt64(reader.GetOrdinal("id")),
            DeviceId = reader.GetInt64(reader.GetOrdinal("device_id")),
            Channel = reader.GetInt32(reader.GetOrdinal("channel")),
            Name = reader.GetString(reader.GetOrdinal("name")),
            Confidence = reader.GetDouble(reader.GetOrdinal("confidence")),
            CooldownSec = reader.GetInt32(reader.GetOrdinal("cooldown_sec")),
            Enabled = reader.GetInt32(reader.GetOrdinal("enabled")) == 1,
            Color = reader.IsDBNull(reader.GetOrdinal("color")) ? "#FF0000" : reader.GetString(reader.GetOrdinal("color"))
        };

        var typeStr = reader.GetString(reader.GetOrdinal("type"));
        rule.Type = Enum.TryParse<AiRuleType>(typeStr, out var t) ? t : AiRuleType.Line;

        var dirStr = reader.IsDBNull(reader.GetOrdinal("direction")) ? "Both" : reader.GetString(reader.GetOrdinal("direction"));
        rule.Direction = Enum.TryParse<AiDirection>(dirStr, out var d) ? d : AiDirection.Both;

        if (!reader.IsDBNull(reader.GetOrdinal("points_json")))
            rule.PointsJson = reader.GetString(reader.GetOrdinal("points_json"));

        if (!reader.IsDBNull(reader.GetOrdinal("classes_json")))
            rule.ClassesJson = reader.GetString(reader.GetOrdinal("classes_json"));

        return rule;
    }
}
