using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VMS_AlarmesJahu.App.Data;
using VMS_AlarmesJahu.App.Models;
using VMS_AlarmesJahu.App.Services;
using VMS_AlarmesJahu.App.IA;

namespace VMS_AlarmesJahu.App.ViewModels;

public partial class DashboardViewModel : ViewModelBase
{
    private readonly AiEventRepository _eventRepo;
    private readonly DeviceRepository _deviceRepo;
    private readonly AiRuleRepository _ruleRepo;
    private readonly ConnectionManager _connectionManager;
    private readonly AiEngine _aiEngine;

    [ObservableProperty] private int _eventsToday;
    [ObservableProperty] private int _eventsWeek;
    [ObservableProperty] private int _eventsMonth;
    [ObservableProperty] private int _connectedDevices;
    [ObservableProperty] private int _totalDevices;
    [ObservableProperty] private int _activeRules;
    [ObservableProperty] private string _lastEventTime = "-";
    [ObservableProperty] private string _uptime = "00:00:00";
    [ObservableProperty] private bool _iaRunning;

    public ObservableCollection<HourlyCount> HourlyData { get; } = new();
    public ObservableCollection<ChannelCount> TopChannels { get; } = new();
    public ObservableCollection<ClassCount> TopClasses { get; } = new();
    public ObservableCollection<AiEvent> RecentEvents { get; } = new();

    public DashboardViewModel(
        AiEventRepository eventRepo,
        DeviceRepository deviceRepo,
        AiRuleRepository ruleRepo,
        ConnectionManager connectionManager,
        AiEngine aiEngine)
    {
        _eventRepo = eventRepo;
        _deviceRepo = deviceRepo;
        _ruleRepo = ruleRepo;
        _connectionManager = connectionManager;
        _aiEngine = aiEngine;

        _aiEngine.OnAlarm += ev => 
        {
            System.Windows.Application.Current?.Dispatcher.Invoke(() => RefreshCommand.Execute(null));
        };

        _ = RefreshAsync();
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        SetBusy(true);

        await Task.Run(() =>
        {
            EventsToday = _eventRepo.CountToday();
            EventsWeek = _eventRepo.CountThisWeek();
            EventsMonth = _eventRepo.CountThisMonth();
            TotalDevices = _deviceRepo.Count();
            ConnectedDevices = _connectionManager.GetConnectedCount();
            ActiveRules = _ruleRepo.CountEnabled();
            IaRunning = _aiEngine.IsRunning;

            var lastTime = _eventRepo.GetLastEventTime();
            LastEventTime = lastTime?.ToString("dd/MM HH:mm:ss") ?? "-";

            if (_aiEngine.IsRunning)
                Uptime = _aiEngine.Uptime.ToString(@"hh\:mm\:ss");
        });

        // Dados para gráficos
        HourlyData.Clear();
        foreach (var h in _eventRepo.GetHourlyCountsToday())
            HourlyData.Add(h);

        TopChannels.Clear();
        foreach (var c in _eventRepo.GetTopChannels(5))
            TopChannels.Add(c);

        TopClasses.Clear();
        foreach (var c in _eventRepo.GetTopClasses(5))
            TopClasses.Add(c);

        RecentEvents.Clear();
        foreach (var ev in _eventRepo.GetRecent(10))
            RecentEvents.Add(ev);

        SetBusy(false);
    }
}
