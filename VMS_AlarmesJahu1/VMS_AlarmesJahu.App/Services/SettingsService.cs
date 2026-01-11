using System;
using System.IO;
using System.Text.Json;
using Serilog;
using VMS_AlarmesJahu.App.Models;

namespace VMS_AlarmesJahu.App.Services;

public class SettingsService
{
    private readonly string _settingsPath;
    private AppSettings _settings;

    public AppSettings Settings => _settings;

    public event Action? SettingsChanged;

    public SettingsService()
    {
        var appData = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "VMS_AlarmesJahu");
        Directory.CreateDirectory(appData);
        _settingsPath = Path.Combine(appData, "settings.json");
        
        _settings = Load();
    }

    public AppSettings Load()
    {
        try
        {
            if (File.Exists(_settingsPath))
            {
                var json = File.ReadAllText(_settingsPath);
                return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Erro ao carregar configurações");
        }
        return new AppSettings();
    }

    public void Save()
    {
        try
        {
            var json = JsonSerializer.Serialize(_settings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_settingsPath, json);
            Log.Information("Configurações salvas");
            SettingsChanged?.Invoke();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Erro ao salvar configurações");
        }
    }

    public void Update(Action<AppSettings> updateAction)
    {
        updateAction(_settings);
        Save();
    }
}
