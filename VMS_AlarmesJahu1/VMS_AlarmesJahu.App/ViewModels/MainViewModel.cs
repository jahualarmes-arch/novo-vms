using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Serilog;
using VMS_AlarmesJahu.App.Data;
using VMS_AlarmesJahu.App.IA;
using VMS_AlarmesJahu.App.Models;
using VMS_AlarmesJahu.App.Services;

namespace VMS_AlarmesJahu.App.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly ConnectionManager _connectionManager;
    private readonly AiEngine _aiEngine;
    private readonly SettingsService _settings;
    private readonly DeviceRepository _deviceRepo;
    private readonly AiEventRepository _eventRepo;
    private readonly AiRuleRepository _ruleRepo;

    [ObservableProperty]
    private string _currentView = "Dashboard";

    [ObservableProperty]
    private bool _iaRunning;

    [ObservableProperty]
    private bool _iaConnected;

    [ObservableProperty]
    private int _connectedDevices;

    [ObservableProperty]
    private int _totalDevices;

    [ObservableProperty]
    private int _eventsToday;

    [ObservableProperty]
    private string _iaStatusText = "IA: Desconectada";

    public ObservableCollection<AiEvent> RecentEvents { get; } = new();

    public MainViewModel(
        ConnectionManager connectionManager,
        AiEngine aiEngine,
        SettingsService settings,
        DeviceRepository deviceRepo,
        AiEventRepository eventRepo,
        AiRuleRepository ruleRepo)
    {
        _connectionManager = connectionManager;
        _aiEngine = aiEngine;
        _settings = settings;
        _deviceRepo = deviceRepo;
        _eventRepo = eventRepo;
        _ruleRepo = ruleRepo;

        _aiEngine.OnAlarm += OnAiAlarm;
        _aiEngine.OnStatusChanged += OnAiStatusChanged;
        _connectionManager.DeviceStatusChanged += OnDeviceStatusChanged;

        LoadInitialData();
    }

    private void LoadInitialData()
    {
        TotalDevices = _deviceRepo.Count();
        ConnectedDevices = _connectionManager.GetConnectedCount();
        EventsToday = _eventRepo.CountToday();

        var recent = _eventRepo.GetRecent(20);
        foreach (var ev in recent)
            RecentEvents.Add(ev);
    }

    private void OnAiAlarm(AiEvent ev)
    {
        Application.Current?.Dispatcher.Invoke(() =>
        {
            RecentEvents.Insert(0, ev);
            if (RecentEvents.Count > 100)
                RecentEvents.RemoveAt(RecentEvents.Count - 1);
            EventsToday = _eventRepo.CountToday();
        });
    }

    private void OnAiStatusChanged(bool running)
    {
        IaRunning = running;
        IaStatusText = running ? $"IA: Rodando ({_settings.Settings.IaServer.BaseUrl})" : "IA: Parada";
    }

    private void OnDeviceStatusChanged(long deviceId, DeviceStatus status)
    {
        ConnectedDevices = _connectionManager.GetConnectedCount();
    }

    [RelayCommand]
    private async Task ToggleIaAsync()
    {
        if (_aiEngine.IsRunning)
        {
            _aiEngine.Stop();
        }
        else
        {
            SetBusy(true, "Verificando conexão com IA...");
            var connected = await _aiEngine.CheckHealthAsync();
            SetBusy(false);

            if (!connected)
            {
                IaConnected = false;
                IaStatusText = "IA: Servidor não encontrado";
                MessageBox.Show(
                    $"Não foi possível conectar ao servidor de IA em:\n{_settings.Settings.IaServer.BaseUrl}\n\nVerifique se o servidor está rodando.",
                    "IA Offline", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            IaConnected = true;
            _aiEngine.Start();
        }
    }

    [RelayCommand]
    private void NavigateTo(string view)
    {
        CurrentView = view;
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        SetBusy(true, "Atualizando...");
        await Task.Run(() =>
        {
            TotalDevices = _deviceRepo.Count();
            ConnectedDevices = _connectionManager.GetConnectedCount();
            EventsToday = _eventRepo.CountToday();
        });
        SetBusy(false);
    }
}
