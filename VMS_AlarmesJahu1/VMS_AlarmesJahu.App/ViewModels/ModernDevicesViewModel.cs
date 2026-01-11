using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Serilog;
using VMS_AlarmesJahu.App.Data;
using VMS_AlarmesJahu.App.Models;
using VMS_AlarmesJahu.App.Services;

namespace VMS_AlarmesJahu.App.ViewModels;

public partial class ModernDevicesViewModel : ViewModelBase
{
    private readonly ConnectionManager _connectionManager;
    private readonly DeviceRepository _deviceRepo;

    [ObservableProperty]
    private string searchText = "";

    [ObservableProperty]
    private string selectedConnectionType = "Todas";

    [ObservableProperty]
    private string selectedStatus = "Todos";

    public ObservableCollection<ModernDeviceItem> AllDevices { get; } = new();
    public ObservableCollection<ModernDeviceItem> FilteredDevices { get; } = new();

    [RelayCommand]
    public void NewDevice()
    {
        Log.Information("Abrir novo dispositivo");
    }

    [RelayCommand]
    public void Refresh()
    {
        LoadDevices();
        Log.Information("Dispositivos recarregados");
    }

    public ModernDevicesViewModel(
        ConnectionManager connectionManager,
        DeviceRepository deviceRepo)
    {
        _connectionManager = connectionManager;
        _deviceRepo = deviceRepo;

        _connectionManager.DeviceStatusChanged += OnDeviceStatusChanged;
        LoadDevices();
    }

    private void LoadDevices()
    {
        AllDevices.Clear();
        var devices = _deviceRepo.GetAll();

        foreach (var device in devices)
        {
            AllDevices.Add(new ModernDeviceItem
            {
                Id = device.Id,
                Name = device.Name,
                Host = device.Host,
                Port = device.Port,
                SerialNumber = device.SerialNumber,
                ConnectionType = device.ConnectionType,
                ChannelCount = device.ChannelCount,
                Status = device.Status,
                LastConnectedAt = device.LastConnectedAt,
                ConnectionTypeLabel = device.ConnectionType == ConnectionType.Direct ? "📡 IP Direto" : "☁️ P2P Cloud",
                ConnectionInfo = device.GetConnectionInfo(),
                LastConnectedText = device.LastConnectedAt.HasValue 
                    ? device.LastConnectedAt.Value.ToString("dd/MM HH:mm") 
                    : "Nunca conectado",
                IsConnected = device.Status == DeviceStatus.Connected,
                IsDisconnected = device.Status == DeviceStatus.Disconnected,
                IsError = device.Status == DeviceStatus.Error
            });
        }

        ApplyFilters();
        Log.Information("Carregados {Count} dispositivos", AllDevices.Count);
    }

    partial void OnSearchTextChanged(string value)
    {
        ApplyFilters();
    }

    partial void OnSelectedConnectionTypeChanged(string value)
    {
        ApplyFilters();
    }

    partial void OnSelectedStatusChanged(string value)
    {
        ApplyFilters();
    }

    private void ApplyFilters()
    {
        var filtered = AllDevices.AsEnumerable();

        // Filtro: Busca por nome ou serial
        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var search = SearchText.ToLower();
            filtered = filtered.Where(d => 
                d.Name.ToLower().Contains(search) ||
                d.SerialNumber.ToLower().Contains(search) ||
                d.Host.ToLower().Contains(search));
        }

        // Filtro: Tipo de conexão
        if (SelectedConnectionType != "Todas")
        {
            filtered = filtered.Where(d =>
                (SelectedConnectionType == "IP Direto" && d.ConnectionType == ConnectionType.Direct) ||
                (SelectedConnectionType == "P2P Cloud" && d.ConnectionType == ConnectionType.P2PCloud));
        }

        // Filtro: Status
        if (SelectedStatus != "Todos")
        {
            filtered = filtered.Where(d =>
                (SelectedStatus == "Conectado" && d.IsConnected) ||
                (SelectedStatus == "Desconectado" && d.IsDisconnected) ||
                (SelectedStatus == "Erro" && d.IsError));
        }

        FilteredDevices.Clear();
        foreach (var item in filtered.OrderBy(d => d.Name))
            FilteredDevices.Add(item);

        Log.Information("Filtros aplicados: {Count} dispositivos visíveis", FilteredDevices.Count);
    }

    private void OnDeviceStatusChanged(long deviceId, DeviceStatus status)
    {
        LoadDevices();
    }
}

public class ModernDeviceItem : ObservableObject
{
    public long Id { get; set; }
    public string Name { get; set; } = "";
    public string Host { get; set; } = "";
    public int Port { get; set; }
    public string SerialNumber { get; set; } = "";
    public ConnectionType ConnectionType { get; set; }
    public int ChannelCount { get; set; }
    public DeviceStatus Status { get; set; }
    public DateTime? LastConnectedAt { get; set; }
    public string ConnectionTypeLabel { get; set; } = "";
    public string ConnectionInfo { get; set; } = "";
    public string LastConnectedText { get; set; } = "";
    public bool IsConnected { get; set; }
    public bool IsDisconnected { get; set; }
    public bool IsError { get; set; }

    public ICommand ConnectCommand => new RelayCommand(() =>
    {
        Log.Information("Conectar: {Device}", Name);
    });

    public ICommand EditCommand => new RelayCommand(() =>
    {
        Log.Information("Editar: {Device}", Name);
    });

    public ICommand DeleteCommand => new RelayCommand(() =>
    {
        Log.Warning("Deletar: {Device}", Name);
    });
}
