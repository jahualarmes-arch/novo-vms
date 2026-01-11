using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.Themes;
using Serilog;
using SkiaSharp;
using VMS_AlarmesJahu.App.Data;
using VMS_AlarmesJahu.App.Models;
using VMS_AlarmesJahu.App.Services;

namespace VMS_AlarmesJahu.App.ViewModels;

public partial class ModernDashboardViewModel : ViewModelBase
{
    private readonly ConnectionManager _connectionManager;
    private readonly DeviceRepository _deviceRepo;
    private readonly AiEventRepository _eventRepo;

    [ObservableProperty]
    private int connectedDevices;

    [ObservableProperty]
    private int totalDevices;

    [ObservableProperty]
    private int activeChannels;

    [ObservableProperty]
    private int eventsToday;

    [ObservableProperty]
    private double systemUptime = 99.5;

    public ObservableCollection<AiEvent> RecentEvents { get; } = new();

    public IEnumerable<ISeries> EventsChartSeries { get; private set; } = new List<ISeries>();
    public Axis[] EventsChartXAxes { get; private set; } = new Axis[0];
    public Axis[] EventsChartYAxes { get; private set; } = new Axis[0];

    public IEnumerable<ISeries> DeviceStatusSeries { get; private set; } = new List<ISeries>();

    public ModernDashboardViewModel(
        ConnectionManager connectionManager,
        DeviceRepository deviceRepo,
        AiEventRepository eventRepo)
    {
        _connectionManager = connectionManager;
        _deviceRepo = deviceRepo;
        _eventRepo = eventRepo;

        _connectionManager.DeviceStatusChanged += OnDeviceStatusChanged;
        LoadData();
        InitializeCharts();
    }

    private void LoadData()
    {
        TotalDevices = _deviceRepo.Count();
        ConnectedDevices = _connectionManager.GetConnectedCount();
        EventsToday = _eventRepo.CountToday();

        var recent = _eventRepo.GetRecent(10);
        foreach (var ev in recent)
            RecentEvents.Add(ev);

        Log.Information("Dashboard carregado: {Devices} dispositivos, {Events} eventos hoje",
            TotalDevices, EventsToday);
    }

    private void InitializeCharts()
    {
        // Gráfico: Eventos por dia (últimos 7 dias)
        var eventsByDay = GenerateEventsData();
        var lineColor = new SKColor(0, 75, 148); // Azul SIM Next

        EventsChartSeries = new List<ISeries>
        {
            new LineSeries<int>
            {
                Values = eventsByDay.Values,
                Fill = new SolidColorPaint(lineColor.WithAlpha(50)),
                Stroke = new SolidColorPaint(lineColor) { StrokeThickness = 3 },
                GeometrySize = 8,
                GeometryFill = new SolidColorPaint(lineColor),
                Name = "Eventos"
            }
        };

        EventsChartXAxes = new[]
        {
            new Axis
            {
                Labels = eventsByDay.Keys.ToList(),
                NamePaint = new SolidColorPaint(SKColors.Black),
                TextSize = 12
            }
        };

        EventsChartYAxes = new[]
        {
            new Axis
            {
                MinLimit = 0,
                NamePaint = new SolidColorPaint(SKColors.Black),
                TextSize = 12
            }
        };

        // Gráfico: Status dos dispositivos
        var statusData = new[] { ConnectedDevices, TotalDevices - ConnectedDevices };
        var colors = new[] { SKColors.Green, SKColors.Red };

        DeviceStatusSeries = new List<ISeries>
        {
            new PieSeries<int>
            {
                Values = new[] { ConnectedDevices },
                Fill = new SolidColorPaint(new SKColor(46, 125, 50)),
                Name = "Conectados"
            },
            new PieSeries<int>
            {
                Values = new[] { TotalDevices - ConnectedDevices },
                Fill = new SolidColorPaint(new SKColor(198, 40, 40)),
                Name = "Desconectados"
            }
        };

        OnPropertyChanged(nameof(EventsChartSeries));
        OnPropertyChanged(nameof(EventsChartXAxes));
        OnPropertyChanged(nameof(EventsChartYAxes));
        OnPropertyChanged(nameof(DeviceStatusSeries));
    }

    private Dictionary<string, int> GenerateEventsData()
    {
        var data = new Dictionary<string, int>();
        for (int i = 6; i >= 0; i--)
        {
            var date = DateTime.Now.AddDays(-i);
            var count = _eventRepo.CountByDate(date);
            data[date.ToString("ddd")] = count;
        }
        return data;
    }

    private void OnDeviceStatusChanged(long deviceId, DeviceStatus status)
    {
        ConnectedDevices = _connectionManager.GetConnectedCount();
        OnPropertyChanged(nameof(DeviceStatusSeries));
        Log.Information("Dashboard atualizado: {Count} dispositivos conectados", ConnectedDevices);
    }
}
