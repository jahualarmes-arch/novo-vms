using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VMS_AlarmesJahu.App.Data;
using VMS_AlarmesJahu.App.Models;
using VMS_AlarmesJahu.App.Services;

namespace VMS_AlarmesJahu.App.ViewModels;

public partial class DevicesViewModel : ViewModelBase
{
    private readonly DeviceRepository _deviceRepo;
    private readonly ConnectionManager _connectionManager;

    [ObservableProperty]
    private Device? _selectedDevice;

    public ObservableCollection<DeviceItem> Devices { get; } = new();

    public DevicesViewModel(DeviceRepository deviceRepo, ConnectionManager connectionManager)
    {
        _deviceRepo = deviceRepo;
        _connectionManager = connectionManager;
        _connectionManager.DeviceStatusChanged += OnDeviceStatusChanged;
        LoadDevices();
    }

    private void LoadDevices()
    {
        Devices.Clear();
        foreach (var d in _deviceRepo.GetAll())
        {
            Devices.Add(new DeviceItem
            {
                Device = d,
                Status = _connectionManager.GetStatus(d.Id)
            });
        }
    }

    private void OnDeviceStatusChanged(long deviceId, DeviceStatus status)
    {
        Application.Current?.Dispatcher.Invoke(() =>
        {
            var item = Devices.FirstOrDefault(d => d.Device.Id == deviceId);
            if (item != null)
                item.Status = status;
        });
    }

    [RelayCommand]
    private async Task ConnectAsync(DeviceItem item)
    {
        if (item.Status == DeviceStatus.Connected)
        {
            _connectionManager.Disconnect(item.Device.Id);
        }
        else
        {
            SetBusy(true, $"Conectando a {item.Device.Name}...");
            await Task.Run(() => _connectionManager.Connect(item.Device.Id));
            SetBusy(false);
        }
    }

    [RelayCommand]
    private void Add()
    {
        var device = new Device
        {
            Name = "Novo DVR",
            Host = "192.168.1.108",
            Port = 37777,
            User = "admin",
            ChannelCount = 16
        };
        device.SetPassword("admin");
        device.Id = _deviceRepo.Insert(device);
        
        Devices.Add(new DeviceItem { Device = device, Status = DeviceStatus.Disconnected });
    }

    [RelayCommand]
    private void Delete(DeviceItem item)
    {
        if (MessageBox.Show($"Remover dispositivo '{item.Device.Name}'?", "Confirmar", 
            MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
            return;

        _connectionManager.Disconnect(item.Device.Id);
        _deviceRepo.Delete(item.Device.Id);
        Devices.Remove(item);
    }

    [RelayCommand]
    private void Save(DeviceItem item)
    {
        _deviceRepo.Update(item.Device);
        MessageBox.Show("Dispositivo salvo!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    [RelayCommand]
    private void Refresh()
    {
        LoadDevices();
    }
}

public partial class DeviceItem : ObservableObject
{
    [ObservableProperty]
    private Device _device = new();

    [ObservableProperty]
    private DeviceStatus _status;

    public string StatusText => Status switch
    {
        DeviceStatus.Connected => "🟢 Conectado",
        DeviceStatus.Connecting => "🟡 Conectando...",
        DeviceStatus.Error => "🔴 Erro",
        _ => "⚪ Desconectado"
    };
}
