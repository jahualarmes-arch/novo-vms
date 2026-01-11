using System;
using System.IO;
using Microsoft.Data.Sqlite;
using Serilog;

namespace VMS_AlarmesJahu.App.Data;

public static class Database
{
    private static string? _connectionString;
    
    public static string ConnectionString
    {
        get
        {
            if (_connectionString == null)
            {
                var appData = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "VMS_AlarmesJahu");
                Directory.CreateDirectory(appData);
                var dbPath = Path.Combine(appData, "vms.db");
                _connectionString = $"Data Source={dbPath}";
            }
            return _connectionString;
        }
    }

    public static SqliteConnection GetConnection()
    {
        var conn = new SqliteConnection(ConnectionString);
        conn.Open();
        return conn;
    }

    public static void Initialize()
    {
        Log.Information("Inicializando banco de dados...");
        
        using var conn = GetConnection();
        using var cmd = conn.CreateCommand();
        
        cmd.CommandText = @"
            -- Dispositivos (DVRs)
            CREATE TABLE IF NOT EXISTS devices (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                name TEXT NOT NULL,
                host TEXT NOT NULL,
                port INTEGER NOT NULL DEFAULT 37777,
                user TEXT NOT NULL DEFAULT 'admin',
                password_hash TEXT,
                channel_count INTEGER DEFAULT 16,
                enabled INTEGER DEFAULT 1,
                created_at TEXT DEFAULT CURRENT_TIMESTAMP,
                last_connected_at TEXT
            );

            -- Regras de IA
            CREATE TABLE IF NOT EXISTS ai_rules (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                device_id INTEGER NOT NULL,
                channel INTEGER NOT NULL,
                name TEXT NOT NULL,
                type TEXT NOT NULL DEFAULT 'Line',
                points_json TEXT,
                classes_json TEXT,
                direction TEXT DEFAULT 'Both',
                confidence REAL DEFAULT 0.35,
                cooldown_sec INTEGER DEFAULT 10,
                enabled INTEGER DEFAULT 1,
                color TEXT DEFAULT '#FF0000',
                created_at TEXT DEFAULT CURRENT_TIMESTAMP,
                FOREIGN KEY (device_id) REFERENCES devices(id) ON DELETE CASCADE
            );

            -- Eventos de IA (histórico de alarmes)
            CREATE TABLE IF NOT EXISTS ai_events (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                device_id INTEGER NOT NULL,
                device_name TEXT,
                channel INTEGER NOT NULL,
                rule_id INTEGER,
                rule_name TEXT,
                class_name TEXT,
                confidence REAL,
                event_type TEXT,
                timestamp TEXT DEFAULT CURRENT_TIMESTAMP,
                snapshot_path TEXT,
                clip_path TEXT,
                acknowledged INTEGER DEFAULT 0,
                acknowledged_by TEXT,
                acknowledged_at TEXT,
                notes TEXT,
                FOREIGN KEY (device_id) REFERENCES devices(id) ON DELETE CASCADE
            );

            -- Configurações da aplicação
            CREATE TABLE IF NOT EXISTS app_config (
                key TEXT PRIMARY KEY,
                value TEXT
            );

            -- Índices para performance
            CREATE INDEX IF NOT EXISTS idx_events_timestamp ON ai_events(timestamp);
            CREATE INDEX IF NOT EXISTS idx_events_device ON ai_events(device_id, channel);
            CREATE INDEX IF NOT EXISTS idx_rules_device ON ai_rules(device_id, channel);
        ";
        cmd.ExecuteNonQuery();
        
        // Executar migrações
        MigrateToVersion2(conn);
        
        Log.Information("Banco de dados inicializado com sucesso");
    }

    private static void MigrateToVersion2(SqliteConnection conn)
    {
        try
        {
            using var cmd = conn.CreateCommand();
            
            // Verificar se já existe a coluna serial_number
            cmd.CommandText = "PRAGMA table_info(devices)";
            using var reader = cmd.ExecuteReader();
            var hasSerialNumber = false;
            var hasConnectionType = false;
            
            while (reader.Read())
            {
                var columnName = reader.GetString(1);
                if (columnName == "serial_number") hasSerialNumber = true;
                if (columnName == "connection_type") hasConnectionType = true;
            }
            reader.Close();

            // Adicionar coluna serial_number se não existir
            if (!hasSerialNumber)
            {
                Log.Information("Migrando banco: adicionando coluna serial_number");
                cmd.CommandText = "ALTER TABLE devices ADD COLUMN serial_number TEXT DEFAULT ''";
                cmd.ExecuteNonQuery();
            }

            // Adicionar coluna connection_type se não existir
            if (!hasConnectionType)
            {
                Log.Information("Migrando banco: adicionando coluna connection_type");
                cmd.CommandText = "ALTER TABLE devices ADD COLUMN connection_type INTEGER DEFAULT 0";
                cmd.ExecuteNonQuery();
            }
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Erro ao executar migração do banco de dados");
        }
    }

    public static void CleanOldEvents(int maxAgeDays)
    {
        using var conn = GetConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            DELETE FROM ai_events 
            WHERE datetime(timestamp) < datetime('now', @days || ' days')";
        cmd.Parameters.AddWithValue("@days", -maxAgeDays);
        var deleted = cmd.ExecuteNonQuery();
        if (deleted > 0)
            Log.Information("Removidos {Count} eventos antigos (>{Days} dias)", deleted, maxAgeDays);
    }
}
