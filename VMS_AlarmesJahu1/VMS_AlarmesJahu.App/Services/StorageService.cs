using System;
using System.IO;
using System.Threading.Tasks;
using Serilog;
using VMS_AlarmesJahu.App.Models;
using VMS_AlarmesJahu.App.Sdk;

namespace VMS_AlarmesJahu.App.Services;

public class StorageService
{
    private readonly SettingsService _settings;
    private readonly string _basePath;

    public StorageService(SettingsService settings)
    {
        _settings = settings;
        _basePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "VMS_AlarmesJahu");
    }

    public string GetSnapshotsPath()
    {
        var path = Path.Combine(_basePath, _settings.Settings.Storage.SnapshotsPath);
        Directory.CreateDirectory(path);
        return path;
    }

    public string GetClipsPath()
    {
        var path = Path.Combine(_basePath, _settings.Settings.Storage.ClipsPath);
        Directory.CreateDirectory(path);
        return path;
    }

    public async Task<string?> SaveSnapshotAsync(byte[] jpegData, long deviceId, int channel)
    {
        try
        {
            var folder = Path.Combine(GetSnapshotsPath(), DateTime.Now.ToString("yyyy-MM-dd"));
            Directory.CreateDirectory(folder);
            
            var filename = $"D{deviceId}_CH{channel}_{DateTime.Now:HHmmss_fff}.jpg";
            var path = Path.Combine(folder, filename);
            
            await File.WriteAllBytesAsync(path, jpegData);
            Log.Debug("Snapshot salvo: {Path}", path);
            
            return path;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Erro ao salvar snapshot");
            return null;
        }
    }

    public async Task<string?> SaveSnapshotFromPlayAsync(IntPtr playHandle, long deviceId, int channel)
    {
        try
        {
            var jpeg = IntelbrasSdk.SnapshotJpegBytesFromPlay(playHandle, 80);
            if (jpeg == null || jpeg.Length < 1000) return null;
            
            return await SaveSnapshotAsync(jpeg, deviceId, channel);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Erro ao capturar snapshot do play handle");
            return null;
        }
    }

    public void CleanOldFiles(int maxAgeDays)
    {
        CleanDirectory(GetSnapshotsPath(), maxAgeDays);
        CleanDirectory(GetClipsPath(), maxAgeDays);
    }

    private void CleanDirectory(string path, int maxAgeDays)
    {
        try
        {
            var cutoff = DateTime.Now.AddDays(-maxAgeDays);
            foreach (var file in Directory.GetFiles(path, "*.*", SearchOption.AllDirectories))
            {
                var info = new FileInfo(file);
                if (info.CreationTime < cutoff)
                {
                    try
                    {
                        File.Delete(file);
                        Log.Debug("Arquivo antigo removido: {File}", file);
                    }
                    catch { }
                }
            }

            // Remove empty directories
            foreach (var dir in Directory.GetDirectories(path))
            {
                if (Directory.GetFiles(dir).Length == 0 && Directory.GetDirectories(dir).Length == 0)
                {
                    try { Directory.Delete(dir); } catch { }
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Erro ao limpar arquivos antigos em {Path}", path);
        }
    }
}
