using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
using VMS_AlarmesJahu.App.Data;
using VMS_AlarmesJahu.App.Models;
using VMS_AlarmesJahu.App.Services;
using VMS_AlarmesJahu.App.Sdk;

namespace VMS_AlarmesJahu.App.IA;

public class AiEngine : IDisposable
{
    private readonly IaClient _client;
    private readonly ConnectionManager _connectionManager;
    private readonly AiRuleRepository _ruleRepo;
    private readonly AiEventRepository _eventRepo;
    private readonly DeviceRepository _deviceRepo;
    private readonly StorageService _storageService;
    private readonly NotificationService _notificationService;
    private readonly SettingsService _settings;

    private readonly ConcurrentDictionary<string, DateTime> _cooldowns = new();
    private CancellationTokenSource? _mainCts;
    private bool _running;
    private DateTime _startTime;

    public event Action<AiEvent>? OnAlarm;
    public event Action<bool>? OnStatusChanged;

    public bool IsRunning => _running;
    public TimeSpan Uptime => _running ? DateTime.Now - _startTime : TimeSpan.Zero;

    public AiEngine(
        ConnectionManager connectionManager,
        AiRuleRepository ruleRepo,
        AiEventRepository eventRepo,
        DeviceRepository deviceRepo,
        StorageService storageService,
        NotificationService notificationService,
        SettingsService settings)
    {
        _connectionManager = connectionManager;
        _ruleRepo = ruleRepo;
        _eventRepo = eventRepo;
        _deviceRepo = deviceRepo;
        _storageService = storageService;
        _notificationService = notificationService;
        _settings = settings;
        _client = new IaClient(settings.Settings.IaServer.BaseUrl, settings.Settings.IaServer.TimeoutMs);
    }

    public void Start()
    {
        if (_running) return;
        _running = true;
        _startTime = DateTime.Now;
        _mainCts = new CancellationTokenSource();
        StartLoop(_mainCts.Token);
        OnStatusChanged?.Invoke(true);
        Log.Information("AI Engine iniciada");
    }

    public void Stop()
    {
        if (!_running) return;
        _running = false;
        _mainCts?.Cancel();
        OnStatusChanged?.Invoke(false);
        Log.Information("AI Engine parada");
    }

    private void StartLoop(CancellationToken ct)
    {
        Task.Run(async () =>
        {
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    await AnalyzeAsync(ct);
                    await Task.Delay(_settings.Settings.IaServer.AnalysisIntervalMs, ct);
                }
                catch (TaskCanceledException) { break; }
                catch (Exception ex)
                {
                    Log.Error(ex, "Erro no loop IA");
                    await Task.Delay(2000, ct);
                }
            }
        }, ct);
    }

    private async Task AnalyzeAsync(CancellationToken ct)
    {
        var plays = _connectionManager.GetAllActivePlays();
        if (plays.Count == 0) return;

        var rules = _ruleRepo.GetEnabledRules();
        if (rules.Count == 0) return;

        var ruleMap = rules.GroupBy(r => (r.DeviceId, r.Channel)).ToDictionary(g => g.Key, g => g.ToList());

        foreach (var (devId, ch, play) in plays)
        {
            if (!ruleMap.TryGetValue((devId, ch), out var chRules)) continue;
            await AnalyzeChannelAsync(devId, ch, play, chRules, ct);
        }
    }

    private async Task AnalyzeChannelAsync(long devId, int ch, IntPtr play, List<AiRule> rules, CancellationToken ct)
    {
        var jpeg = IntelbrasSdk.SnapshotJpegBytesFromPlay(play, 70);
        if (jpeg == null || jpeg.Length < 2000) return;

        var minConf = rules.Min(r => r.Confidence);
        var res = await _client.DetectAsync(jpeg, minConf, ct);
        if (res?.ok != true) return;

        foreach (var rule in rules)
        {
            foreach (var det in res.detections)
            {
                if (!RuleEvaluator.Match(rule, det, res.w, res.h)) continue;

                var coolKey = $"{devId}:{ch}:{rule.Id}:{det.class_name}";
                if (_cooldowns.TryGetValue(coolKey, out var until) && DateTime.UtcNow < until) continue;
                _cooldowns[coolKey] = DateTime.UtcNow.AddSeconds(Math.Max(3, rule.CooldownSec));

                await TriggerAlarmAsync(devId, ch, rule, det, jpeg);
                break;
            }
        }
    }

    private async Task TriggerAlarmAsync(long devId, int ch, AiRule rule, IaDetection det, byte[] jpeg)
    {
        var device = _deviceRepo.GetById(devId);
        var snapshotPath = await _storageService.SaveSnapshotAsync(jpeg, devId, ch);

        var ev = new AiEvent
        {
            DeviceId = devId,
            DeviceName = device?.Name ?? "DVR",
            Channel = ch,
            RuleId = rule.Id,
            RuleName = rule.Name,
            ClassName = det.class_name,
            Confidence = det.conf,
            EventType = rule.Type == AiRuleType.Polygon ? "AREA" : "LINE",
            Timestamp = DateTime.Now,
            SnapshotPath = snapshotPath
        };

        ev.Id = _eventRepo.Insert(ev);
        Log.Information("Alarme: {Device} CH{Ch} {Class} ({Conf:P0}) - {Rule}", ev.DeviceName, ch + 1, det.class_name, det.conf, rule.Name);

        _ = _notificationService.PlayAlarmSoundAsync();
        _ = _notificationService.SendTelegramAsync(ev, snapshotPath);

        OnAlarm?.Invoke(ev);
    }

    public async Task<bool> CheckHealthAsync()
    {
        var result = await _client.HealthCheckAsync();
        return result?.status == "ok";
    }

    public void Dispose()
    {
        Stop();
        _client.Dispose();
    }
}
