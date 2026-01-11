using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using VMS_AlarmesJahu.App.Data;
using VMS_AlarmesJahu.App.Models;

namespace VMS_AlarmesJahu.App.Views;

public partial class EventsView : UserControl
{
    private AiEventRepository? _eventRepo;
    private DeviceRepository? _deviceRepo;

    public EventsView()
    {
        InitializeComponent();
        
        DpStart.SelectedDate = DateTime.Today.AddDays(-7);
        DpEnd.SelectedDate = DateTime.Today;
        
        BtnSearch.Click += (s, e) => Search();
        BtnExport.Click += (s, e) => Export();
        
        Loaded += (s, e) => Search();
    }

    public void Initialize(AiEventRepository eventRepo, DeviceRepository deviceRepo)
    {
        _eventRepo = eventRepo;
        _deviceRepo = deviceRepo;
        
        var devices = _deviceRepo.GetAll();
        devices.Insert(0, new Device { Id = 0, Name = "Todos" });
        CbDevice.ItemsSource = devices;
        CbDevice.SelectedIndex = 0;
        
        Search();
    }

    private void Search()
    {
        if (_eventRepo == null) return;

        var start = DpStart.SelectedDate ?? DateTime.Today.AddDays(-7);
        var end = (DpEnd.SelectedDate ?? DateTime.Today).AddDays(1);
        
        var device = CbDevice.SelectedItem as Device;
        long? deviceId = device?.Id > 0 ? device.Id : null;

        var events = _eventRepo.GetByDateRange(start, end, deviceId);
        DgEvents.ItemsSource = events;
        TxtCount.Text = $"{events.Count} eventos";
    }

    private void Export()
    {
        var events = DgEvents.ItemsSource as System.Collections.Generic.List<AiEvent>;
        if (events == null || events.Count == 0)
        {
            MessageBox.Show("Nenhum evento para exportar.");
            return;
        }

        var dialog = new SaveFileDialog
        {
            Filter = "CSV (*.csv)|*.csv",
            FileName = $"eventos_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
        };

        if (dialog.ShowDialog() == true)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Data/Hora,Dispositivo,Canal,Classe,Confiança,Tipo,Regra,Reconhecido");
            
            foreach (var ev in events)
            {
                sb.AppendLine($"{ev.Timestamp:yyyy-MM-dd HH:mm:ss},{ev.DeviceName},{ev.Channel},{ev.ClassName},{ev.Confidence:P0},{ev.EventType},{ev.RuleName},{ev.Acknowledged}");
            }

            File.WriteAllText(dialog.FileName, sb.ToString(), Encoding.UTF8);
            MessageBox.Show($"Exportado {events.Count} eventos para:\n{dialog.FileName}", "Exportação Concluída");
        }
    }

    private void BtnViewSnapshot_Click(object sender, RoutedEventArgs e)
    {
        var btn = sender as Button;
        var path = btn?.Tag?.ToString();
        
        if (!string.IsNullOrEmpty(path) && File.Exists(path))
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = path,
                UseShellExecute = true
            });
        }
        else
        {
            MessageBox.Show("Snapshot não encontrado.");
        }
    }
}
