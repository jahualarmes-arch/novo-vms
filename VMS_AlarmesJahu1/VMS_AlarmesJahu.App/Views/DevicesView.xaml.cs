using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using VMS_AlarmesJahu.App.Data;
using VMS_AlarmesJahu.App.Models;
using VMS_AlarmesJahu.App.Services;
using VMS_AlarmesJahu.App.ViewModels;

namespace VMS_AlarmesJahu.App.Views;

public partial class DevicesView : UserControl
{
    private DeviceRepository? _deviceRepo;
    private ConnectionManager? _connectionManager;
    private Device? _currentDevice;

    public DevicesView()
    {
        InitializeComponent();
        BtnAdd.Click += (s, e) => AddDevice();
        BtnRefresh.Click += (s, e) => LoadDevices();
        BtnSave.Click += (s, e) => SaveDevice();
        BtnDelete.Click += (s, e) => DeleteDevice();
        BtnTestConnection.Click += (s, e) => TestConnection();
        PanelDetails.Visibility = Visibility.Collapsed;
        CmbConnectionType.SelectedIndex = 0; // Padrão: Conexão Direta
    }

    public void Initialize(DeviceRepository deviceRepo, ConnectionManager connMgr)
    {
        _deviceRepo = deviceRepo;
        _connectionManager = connMgr;
        _connectionManager.DeviceStatusChanged += (id, status) => Dispatcher.Invoke(LoadDevices);
        LoadDevices();
    }

    private void LoadDevices()
    {
        if (_deviceRepo == null || _connectionManager == null) return;
        var items = _deviceRepo.GetAll().Select(d => new DeviceItem
        {
            Device = d,
            Status = _connectionManager.GetStatus(d.Id)
        }).ToList();
        LvDevices.ItemsSource = items;
    }

    private void LvDevices_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var item = LvDevices.SelectedItem as DeviceItem;
        if (item == null)
        {
            PanelDetails.Visibility = Visibility.Collapsed;
            return;
        }

        _currentDevice = item.Device;
        TxtName.Text = _currentDevice.Name;
        TxtUser.Text = _currentDevice.User;
        TxtPassword.Password = _currentDevice.GetPassword();
        ChkEnabled.IsChecked = _currentDevice.Enabled;

        // Configurar tipo de conexão
        if (_currentDevice.ConnectionType == ConnectionType.P2PCloud)
        {
            CmbConnectionType.SelectedIndex = 1;
            TxtSerialNumber.Text = _currentDevice.SerialNumber;
            TxtPortP2P.Text = _currentDevice.Port.ToString(); // CORRIGIDO: Usar porta do device
            TxtChannelsP2P.Text = _currentDevice.ChannelCount.ToString();
        }
        else
        {
            CmbConnectionType.SelectedIndex = 0;
            TxtHost.Text = _currentDevice.Host;
            TxtPort.Text = _currentDevice.Port.ToString();
            TxtChannels.Text = _currentDevice.ChannelCount.ToString();
        }

        PanelDetails.Visibility = Visibility.Visible;
    }

    private void CmbConnectionType_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (CmbConnectionType.SelectedIndex == 1) // P2P Cloud
        {
            PanelDirect.Visibility = Visibility.Collapsed;
            PanelP2P.Visibility = Visibility.Visible;
        }
        else // Conexão Direta
        {
            PanelDirect.Visibility = Visibility.Visible;
            PanelP2P.Visibility = Visibility.Collapsed;
        }
    }

    private void BtnConnect_Click(object sender, RoutedEventArgs e)
    {
        var btn = sender as Button;
        var item = btn?.Tag as DeviceItem;
        if (item == null || _connectionManager == null) return;

        if (item.Status == DeviceStatus.Connected)
            _connectionManager.Disconnect(item.Device.Id);
        else
            _connectionManager.Connect(item.Device.Id);
        
        LoadDevices();
    }

    private void AddDevice()
    {
        // Criar novo dispositivo vazio (não salva no banco ainda)
        _currentDevice = new Device
        {
            Name = "",
            Host = "",
            Port = 37777,
            SerialNumber = "",
            ConnectionType = ConnectionType.Direct,
            User = "admin",
            ChannelCount = 16,
            Enabled = true
        };
        
        // Limpar formulário
        TxtName.Text = "";
        TxtHost.Text = "";
        TxtPort.Text = "37777";
        TxtChannels.Text = "16";
        TxtSerialNumber.Text = "";
        TxtPortP2P.Text = "37777"; // CORRIGIDO: Porta P2P
        TxtChannelsP2P.Text = "16";
        TxtUser.Text = "admin";
        TxtPassword.Password = "";
        ChkEnabled.IsChecked = true;
        CmbConnectionType.SelectedIndex = 0; // Padrão: Conexão Direta
        
        // Desselecionar lista e mostrar painel
        LvDevices.SelectedItem = null;
        PanelDetails.Visibility = Visibility.Visible;
        TxtName.Focus();
    }

    private void SaveDevice()
    {
        if (_currentDevice == null) return;

        // Validar nome
        if (string.IsNullOrWhiteSpace(TxtName.Text))
        {
            MessageBox.Show("Preencha o Nome do dispositivo.", "Campos Obrigatórios", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        _currentDevice.Name = TxtName.Text;
        _currentDevice.User = TxtUser.Text;
        _currentDevice.Enabled = ChkEnabled.IsChecked == true;

        // Definir tipo de conexão baseado na seleção
        if (CmbConnectionType.SelectedIndex == 1) // P2P Cloud
        {
            if (string.IsNullOrWhiteSpace(TxtSerialNumber.Text))
            {
                MessageBox.Show("Preencha o Número de Série para conexão P2P.", "Campos Obrigatórios", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _currentDevice.ConnectionType = ConnectionType.P2PCloud;
            _currentDevice.SerialNumber = TxtSerialNumber.Text.Trim();
            _currentDevice.Port = int.TryParse(TxtPortP2P.Text, out var p2pPort) ? p2pPort : 37777; // CORRIGIDO: Salvar porta P2P
            _currentDevice.ChannelCount = int.TryParse(TxtChannelsP2P.Text, out var c2) ? c2 : 16;
            _currentDevice.Host = "";
        }
        else // Conexão Direta
        {
            if (string.IsNullOrWhiteSpace(TxtHost.Text))
            {
                MessageBox.Show("Preencha o Host/IP para conexão direta.", "Campos Obrigatórios", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _currentDevice.ConnectionType = ConnectionType.Direct;
            _currentDevice.Host = TxtHost.Text.Trim();
            _currentDevice.Port = int.TryParse(TxtPort.Text, out var p) ? p : 37777;
            _currentDevice.ChannelCount = int.TryParse(TxtChannels.Text, out var c) ? c : 16;
            _currentDevice.SerialNumber = "";
        }

        // Atualizar senha se foi digitada
        if (!string.IsNullOrEmpty(TxtPassword.Password))
            _currentDevice.SetPassword(TxtPassword.Password);

        // Se é novo (Id == 0), inserir; senão, atualizar
        if (_currentDevice.Id == 0)
        {
            _currentDevice.Id = _deviceRepo?.Insert(_currentDevice) ?? 0;
            MessageBox.Show("Dispositivo adicionado!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        else
        {
            _deviceRepo?.Update(_currentDevice);
            MessageBox.Show("Dispositivo salvo!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        
        LoadDevices();
    }

    private void DeleteDevice()
    {
        if (_currentDevice == null) return;
        if (MessageBox.Show($"Excluir '{_currentDevice.Name}'?", "Confirmar", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
            return;

        _connectionManager?.Disconnect(_currentDevice.Id);
        _deviceRepo?.Delete(_currentDevice.Id);
        _currentDevice = null;
        PanelDetails.Visibility = Visibility.Collapsed;
        LoadDevices();
    }

    private void TestConnection()
    {
        if (_connectionManager == null || _currentDevice == null) return;

        // Salvar primeiro
        SaveDevice();
        
        if (_currentDevice.Id == 0) return;

        // Tentar conectar
        var result = _connectionManager.Connect(_currentDevice.Id);
        
        if (result != IntPtr.Zero)
        {
            // Extra: se for IP direto, tenta também validar a API HTTP V3.59 (inspirado em PyIntelbras)
            // Isso melhora o diagnóstico (canais/status) quando o DVR está acessível por rede.
            string extra = "";
            if (_currentDevice.ConnectionType == ConnectionType.Direct && !string.IsNullOrWhiteSpace(_currentDevice.Host))
            {
                try
                {
                    var httpClient = new IntelbrasHttpV359Client();
                    var probe = httpClient.ProbeAsync(
                        _currentDevice.Host,
                        _currentDevice.User,
                        _currentDevice.GetPassword()
                    ).GetAwaiter().GetResult();

                    if (probe.Success)
                    {
                        extra = $"\n\nHTTP API OK: {probe.Host}:{probe.Port}\n{probe.Summary}";
                    }
                    else
                    {
                        extra = "\n\nHTTP API: não respondeu (ok se você usa só SDK/stream).";
                    }
                }
                catch
                {
                    // não derruba o teste
                }
            }
            else if (_currentDevice.ConnectionType == ConnectionType.P2PCloud)
            {
                extra = "\n\nObs: Em Cloud P2P, a API HTTP não é testada (depende de IP/porta HTTP acessível).";
            }

            MessageBox.Show("✅ Conexão realizada com sucesso!" + extra, "Teste de Conexão", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        else
        {
            var connInfo = _currentDevice.ConnectionType == ConnectionType.P2PCloud
                ? $"P2P: {_currentDevice.SerialNumber} (Porta: {_currentDevice.Port})"
                : $"{_currentDevice.Host}:{_currentDevice.Port}";
            
            MessageBox.Show($"❌ Falha na conexão!\n\nVerifique:\n• {connInfo}\n• Usuário e Senha\n• Se o DVR está online", 
                "Teste de Conexão", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        
        LoadDevices();
    }
}
