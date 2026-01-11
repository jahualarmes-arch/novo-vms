using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using VMS_AlarmesJahu.App.Data;
using VMS_AlarmesJahu.App.IA;
using VMS_AlarmesJahu.App.Services;

namespace VMS_AlarmesJahu.App.Views;

public partial class SettingsView : UserControl
{
    private SettingsService? _settings;
    private StorageService? _storage;

    public SettingsView()
    {
        InitializeComponent();
        BtnSave.Click += (s, e) => Save();
        BtnTestIa.Click += async (s, e) => await TestIa();
        BtnBrowseSound.Click += (s, e) => BrowseSound();
        BtnTestTelegram.Click += async (s, e) => await TestTelegram();
        BtnCleanOld.Click += (s, e) => CleanOldData();
    }

    public void Initialize(SettingsService settings, StorageService storage)
    {
        _settings = settings;
        _storage = storage;
        Load();
    }

    private void Load()
    {
        if (_settings == null) return;
        var s = _settings.Settings;

        TxtIaUrl.Text = s.IaServer.BaseUrl;
        TxtInterval.Text = s.IaServer.AnalysisIntervalMs.ToString();
        TxtTimeout.Text = s.IaServer.TimeoutMs.ToString();

        TxtFullscreenDuration.Text = s.Alarm.FullscreenDurationSec.ToString();
        TxtBeepCount.Text = s.Alarm.BeepCount.ToString();
        ChkSoundEnabled.IsChecked = s.Notifications.EnableSound;
        TxtCustomSound.Text = s.Alarm.CustomSoundPath ?? "";

        ChkTelegramEnabled.IsChecked = s.Notifications.EnableTelegram;
        TxtTelegramToken.Text = s.Notifications.TelegramBotToken;
        TxtTelegramChat.Text = s.Notifications.TelegramChatId;

        TxtMaxEventAge.Text = s.Storage.MaxEventAgeDays.ToString();
    }

    private void Save()
    {
        if (_settings == null) return;

        _settings.Update(s =>
        {
            s.IaServer.BaseUrl = TxtIaUrl.Text;
            s.IaServer.AnalysisIntervalMs = int.TryParse(TxtInterval.Text, out var i) ? i : 800;
            s.IaServer.TimeoutMs = int.TryParse(TxtTimeout.Text, out var t) ? t : 5000;

            s.Alarm.FullscreenDurationSec = int.TryParse(TxtFullscreenDuration.Text, out var f) ? f : 30;
            s.Alarm.BeepCount = int.TryParse(TxtBeepCount.Text, out var b) ? b : 3;
            s.Alarm.CustomSoundPath = string.IsNullOrWhiteSpace(TxtCustomSound.Text) ? null : TxtCustomSound.Text;

            s.Notifications.EnableSound = ChkSoundEnabled.IsChecked == true;
            s.Notifications.EnableTelegram = ChkTelegramEnabled.IsChecked == true;
            s.Notifications.TelegramBotToken = TxtTelegramToken.Text;
            s.Notifications.TelegramChatId = TxtTelegramChat.Text;

            s.Storage.MaxEventAgeDays = int.TryParse(TxtMaxEventAge.Text, out var m) ? m : 90;
        });

        MessageBox.Show("Configurações salvas!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private async System.Threading.Tasks.Task TestIa()
    {
        var client = new IaClient(TxtIaUrl.Text);
        var result = await client.HealthCheckAsync();
        
        if (result?.status == "ok")
            MessageBox.Show($"✅ Conexão OK!\nModelo: {result.model}\nGPU: {result.gpu}", "IA Online");
        else
            MessageBox.Show("❌ Não foi possível conectar ao servidor de IA.", "IA Offline");
        
        client.Dispose();
    }

    private void BrowseSound()
    {
        var dialog = new OpenFileDialog
        {
            Filter = "Arquivos de Áudio (*.wav;*.mp3)|*.wav;*.mp3"
        };
        if (dialog.ShowDialog() == true)
            TxtCustomSound.Text = dialog.FileName;
    }

    private async System.Threading.Tasks.Task TestTelegram()
    {
        if (string.IsNullOrEmpty(TxtTelegramToken.Text) || string.IsNullOrEmpty(TxtTelegramChat.Text))
        {
            MessageBox.Show("Preencha o Token e Chat ID.");
            return;
        }

        try
        {
            using var client = new System.Net.Http.HttpClient();
            var url = $"https://api.telegram.org/bot{TxtTelegramToken.Text}/sendMessage";
            var content = new System.Net.Http.FormUrlEncodedContent(new[]
            {
                new System.Collections.Generic.KeyValuePair<string, string>("chat_id", TxtTelegramChat.Text),
                new System.Collections.Generic.KeyValuePair<string, string>("text", "🔔 Teste VMS Alarmes Jahu - Conexão OK!")
            });
            var response = await client.PostAsync(url, content);
            
            if (response.IsSuccessStatusCode)
                MessageBox.Show("✅ Mensagem enviada com sucesso!", "Telegram OK");
            else
                MessageBox.Show($"❌ Erro: {response.StatusCode}", "Telegram Erro");
        }
        catch (System.Exception ex)
        {
            MessageBox.Show($"❌ Erro: {ex.Message}", "Telegram Erro");
        }
    }

    private void CleanOldData()
    {
        var days = int.TryParse(TxtMaxEventAge.Text, out var d) ? d : 90;
        Database.CleanOldEvents(days);
        _storage?.CleanOldFiles(days);
        MessageBox.Show("Dados antigos removidos!", "Limpeza Concluída");
    }
}
