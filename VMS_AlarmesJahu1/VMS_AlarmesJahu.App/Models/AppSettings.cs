namespace VMS_AlarmesJahu.App.Models;

public class AppSettings
{
    public IaServerSettings IaServer { get; set; } = new();
    public AlarmSettings Alarm { get; set; } = new();
    public StorageSettings Storage { get; set; } = new();
    public UiSettings Ui { get; set; } = new();
    public NotificationSettings Notifications { get; set; } = new();
}

public class IaServerSettings
{
    public string BaseUrl { get; set; } = "http://127.0.0.1:8099";
    public int AnalysisIntervalMs { get; set; } = 800;
    public int TimeoutMs { get; set; } = 5000;
}

public class AlarmSettings
{
    public int FullscreenDurationSec { get; set; } = 30;
    public int BeepCount { get; set; } = 3;
    public int BeepFrequency { get; set; } = 1200;
    public int BeepDurationMs { get; set; } = 250;
    public int ClipPreRollSec { get; set; } = 10;
    public int ClipPostRollSec { get; set; } = 10;
    public string? CustomSoundPath { get; set; }
}

public class StorageSettings
{
    public string SnapshotsPath { get; set; } = "Snapshots";
    public string ClipsPath { get; set; } = "Clips";
    public string LogsPath { get; set; } = "Logs";
    public int MaxEventAgeDays { get; set; } = 90;
}

public class UiSettings
{
    public string Theme { get; set; } = "Dark";
    public string DefaultMosaicLayout { get; set; } = "2x2";
    public string Language { get; set; } = "pt-BR";
}

public class NotificationSettings
{
    public bool EnableSound { get; set; } = true;
    public bool EnableTelegram { get; set; }
    public string TelegramBotToken { get; set; } = "";
    public string TelegramChatId { get; set; } = "";
}
