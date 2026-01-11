using System;
using System.Collections.Generic;
using System.IO;
using System.Media;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
using VMS_AlarmesJahu.App.Models;

namespace VMS_AlarmesJahu.App.Services;

public class NotificationService : IDisposable
{
    private readonly SettingsService _settings;
    private readonly HttpClient _httpClient;
    private SoundPlayer? _customSound;

    public NotificationService(SettingsService settings)
    {
        _settings = settings;
        _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
        LoadCustomSound();
    }

    private void LoadCustomSound()
    {
        var path = _settings.Settings.Alarm.CustomSoundPath;
        if (!string.IsNullOrEmpty(path) && File.Exists(path))
        {
            try
            {
                _customSound = new SoundPlayer(path);
                _customSound.Load();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Erro ao carregar som customizado: {Path}", path);
            }
        }
    }

    public async Task PlayAlarmSoundAsync()
    {
        if (!_settings.Settings.Notifications.EnableSound) return;

        try
        {
            if (_customSound != null)
            {
                await Task.Run(() =>
                {
                    for (int i = 0; i < _settings.Settings.Alarm.BeepCount; i++)
                    {
                        _customSound.PlaySync();
                        Thread.Sleep(120);
                    }
                });
            }
            else
            {
                await Task.Run(() =>
                {
                    var freq = _settings.Settings.Alarm.BeepFrequency;
                    var duration = _settings.Settings.Alarm.BeepDurationMs;
                    
                    for (int i = 0; i < _settings.Settings.Alarm.BeepCount; i++)
                    {
                        try
                        {
                            Console.Beep(freq, duration);
                        }
                        catch
                        {
                            SystemSounds.Exclamation.Play();
                        }
                        Thread.Sleep(120);
                    }
                });
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Erro ao reproduzir som de alarme");
        }
    }

    public async Task SendTelegramAsync(AiEvent ev, string? imagePath = null)
    {
        var cfg = _settings.Settings.Notifications;
        if (!cfg.EnableTelegram || string.IsNullOrEmpty(cfg.TelegramBotToken) || string.IsNullOrEmpty(cfg.TelegramChatId))
            return;

        try
        {
            var message = $"🚨 *ALERTA VMS*\n\n" +
                          $"📍 {ev.DeviceName} - CH{ev.Channel + 1}\n" +
                          $"🏷️ {ev.ClassName} ({ev.Confidence:P0})\n" +
                          $"📋 {ev.RuleName} ({ev.EventType})\n" +
                          $"🕐 {ev.Timestamp:dd/MM/yyyy HH:mm:ss}";

            if (!string.IsNullOrEmpty(imagePath) && File.Exists(imagePath))
            {
                await SendTelegramPhotoAsync(cfg.TelegramBotToken, cfg.TelegramChatId, imagePath, message);
            }
            else
            {
                await SendTelegramMessageAsync(cfg.TelegramBotToken, cfg.TelegramChatId, message);
            }
            
            Log.Information("Notificação Telegram enviada para evento {EventId}", ev.Id);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Erro ao enviar notificação Telegram");
        }
    }

    private async Task SendTelegramMessageAsync(string token, string chatId, string message)
    {
        var url = $"https://api.telegram.org/bot{token}/sendMessage";
        var content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("chat_id", chatId),
            new KeyValuePair<string, string>("text", message),
            new KeyValuePair<string, string>("parse_mode", "Markdown")
        });
        
        var response = await _httpClient.PostAsync(url, content);
        response.EnsureSuccessStatusCode();
    }

    private async Task SendTelegramPhotoAsync(string token, string chatId, string imagePath, string caption)
    {
        var url = $"https://api.telegram.org/bot{token}/sendPhoto";
        
        using var form = new MultipartFormDataContent();
        form.Add(new StringContent(chatId), "chat_id");
        form.Add(new StringContent(caption), "caption");
        form.Add(new StringContent("Markdown"), "parse_mode");
        
        var imageBytes = await File.ReadAllBytesAsync(imagePath);
        form.Add(new ByteArrayContent(imageBytes), "photo", "snapshot.jpg");
        
        var response = await _httpClient.PostAsync(url, form);
        response.EnsureSuccessStatusCode();
    }

    public void Dispose()
    {
        _customSound?.Dispose();
        _httpClient.Dispose();
    }
}
