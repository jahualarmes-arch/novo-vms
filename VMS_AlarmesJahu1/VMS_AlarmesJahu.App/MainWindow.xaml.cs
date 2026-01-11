using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using VMS_AlarmesJahu.App.Data;
using VMS_AlarmesJahu.App.IA;
using VMS_AlarmesJahu.App.Models;
using VMS_AlarmesJahu.App.Services;
using VMS_AlarmesJahu.App.Views;

namespace VMS_AlarmesJahu.App;

public partial class MainWindow : Window
{
    private readonly ConnectionManager _connectionManager;
    private readonly AiEngine _aiEngine;
    private readonly SettingsService _settings;
    private readonly DeviceRepository _deviceRepo;
    private readonly AiRuleRepository _ruleRepo;
    private readonly AiEventRepository _eventRepo;
    private readonly StorageService _storage;
    private readonly NotificationService _notifications;

    private HomeView? _home;
    private DashboardView? _dashboard;
    private MosaicView? _mosaic;
    private DevicesView? _devices;
    private AiConfigView? _aiConfig;
    private EventsView? _events;
    private SettingsView? _settingsView;

    private string _treeFilter = "";

    public MainWindow(
        ConnectionManager connectionManager,
        AiEngine aiEngine,
        SettingsService settings,
        DeviceRepository deviceRepo,
        AiRuleRepository ruleRepo,
        AiEventRepository eventRepo,
        StorageService storage,
        NotificationService notifications)
    {
        InitializeComponent();

        _connectionManager = connectionManager;
        _aiEngine = aiEngine;
        _settings = settings;
        _deviceRepo = deviceRepo;
        _ruleRepo = ruleRepo;
        _eventRepo = eventRepo;
        _storage = storage;
        _notifications = notifications;

        _aiEngine.OnStatusChanged += OnAiStatusChanged;
        _aiEngine.OnAlarm += OnAlarm;

        // Tabs
        BtnTabHome.Checked += (s, e) => ShowHome();
        BtnTabLive.Checked += (s, e) => ShowLive();
        BtnTabManage.Checked += (s, e) => ShowManage();
        BtnTabHome.Click += (s, e) => SelectTab(BtnTabHome);
        BtnTabLive.Click += (s, e) => SelectTab(BtnTabLive);
        BtnTabManage.Click += (s, e) => SelectTab(BtnTabManage);

        // Quick actions
        BtnToggleIa.Click += async (s, e) => await ToggleIaAsync();
        BtnQuickSettings.Click += (s, e) => { SelectTab(BtnTabManage); ShowSettings(); };

        // Tree
        TxtTreeSearch.GotKeyboardFocus += (s, e) =>
        {
            if (TxtTreeSearch.Text == "Pesquisar...")
            {
                TxtTreeSearch.Text = "";
                TxtTreeSearch.Opacity = 1.0;
            }
        };
        TxtTreeSearch.LostKeyboardFocus += (s, e) =>
        {
            if (string.IsNullOrWhiteSpace(TxtTreeSearch.Text))
            {
                TxtTreeSearch.Text = "Pesquisar...";
                TxtTreeSearch.Opacity = 0.75;
            }
        };
        TxtTreeSearch.TextChanged += (s, e) =>
        {
            if (TxtTreeSearch.Text == "Pesquisar...") return;
            _treeFilter = TxtTreeSearch.Text.Trim();
            ReloadTree();
        };

        TvOrg.MouseDoubleClick += TvOrg_MouseDoubleClick;

        BtnTreeAdd.Click += (s, e) => { SelectTab(BtnTabManage); ShowDevices(); };
        BtnTreeDel.Click += (s, e) => DeleteSelectedDeviceFromTree();

        // Default
        ReloadTree();
        ShowHome();
        OnAiStatusChanged(_aiEngine.IsRunning);
    }

    private void SelectTab(ToggleButton tab)
    {
        BtnTabHome.IsChecked = tab == BtnTabHome;
        BtnTabLive.IsChecked = tab == BtnTabLive;
        BtnTabManage.IsChecked = tab == BtnTabManage;

        if (tab == BtnTabHome) ShowHome();
        else if (tab == BtnTabLive) ShowLive();
        else ShowManage();
    }

    private void ShowHome()
    {
        LeftPanel.Visibility = Visibility.Collapsed;
        ColLeft.Width = new GridLength(0);

        _home ??= new HomeView();
        _home.OpenDeviceManager += () => { SelectTab(BtnTabManage); ShowDevices(); };
        _home.OpenLogs += () => { SelectTab(BtnTabManage); ShowEvents(); };
        _home.OpenEventConfig += () => { SelectTab(BtnTabManage); ShowAiConfig(); };
        _home.OpenManual += OpenManual;
        _home.OpenLiveView += () => { SelectTab(BtnTabLive); ShowLive(); };
        _home.OpenFullscreen += OpenCleanFullscreen;
        _home.OpenPlayback += () => { /* Reprodução não implementada ainda */ };

        ContentHost.Content = _home;
    }

    private void ShowLive()
    {
        LeftPanel.Visibility = Visibility.Visible;
        ColLeft.Width = new GridLength(280);

        _mosaic ??= new MosaicView();
        _mosaic.Initialize(_connectionManager, _deviceRepo, _ruleRepo, _aiEngine);
        ContentHost.Content = _mosaic;
    }

    private void ShowManage()
    {
        LeftPanel.Visibility = Visibility.Collapsed;
        ColLeft.Width = new GridLength(0);

        // By default show device manager like SmartPSS "Gerenciamento"
        ShowDevices();
    }

    private void ShowDevices()
    {
        _devices ??= new DevicesView();
        _devices.Initialize(_deviceRepo, _connectionManager);
        ContentHost.Content = _devices;
        ReloadTree();
    }

    private void ShowAiConfig()
    {
        _aiConfig ??= new AiConfigView();
        _aiConfig.Initialize(_deviceRepo, _ruleRepo, _connectionManager);
        ContentHost.Content = _aiConfig;
    }

    private void ShowEvents()
    {
        _events ??= new EventsView();
        _events.Initialize(_eventRepo, _deviceRepo);
        ContentHost.Content = _events;
    }

    private void ShowSettings()
    {
        _settingsView ??= new SettingsView();
        _settingsView.Initialize(_settings, _storage);
        ContentHost.Content = _settingsView;
    }

    /// <summary>
    /// Abre a janela fullscreen limpa (estilo SmartPSS Lite)
    /// </summary>
    private void OpenCleanFullscreen()
    {
        var devices = _deviceRepo.GetAll().Where(d => d.Enabled).ToList();
        if (devices.Count == 0)
        {
            MessageBox.Show("Nenhum dispositivo cadastrado.\n\nAdicione um DVR/NVR primeiro em Gerenciamento > Dispositivos.", 
                "Sem Dispositivos", MessageBoxButton.OK, MessageBoxImage.Information);
            SelectTab(BtnTabManage);
            ShowDevices();
            return;
        }

        var fullscreenWindow = new CleanMosaicWindow(_connectionManager, _deviceRepo);
        fullscreenWindow.Show();
    }

    private void OpenManual()
    {
        try
        {
            // Opens the PDF if present in installer folder; otherwise just open project folder.
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var pdf = System.IO.Path.Combine(baseDir, "Documentação API INC Cloud v.1.0.0_2.pdf");
            if (System.IO.File.Exists(pdf))
            {
                Process.Start(new ProcessStartInfo(pdf) { UseShellExecute = true });
                return;
            }
            Process.Start(new ProcessStartInfo(baseDir) { UseShellExecute = true });
        }
        catch
        {
            MessageBox.Show("Não foi possível abrir o manual.");
        }
    }

    private void ReloadTree()
    {
        TvOrg.Items.Clear();

        var devices = _deviceRepo.GetAll().Where(d => d.Enabled).ToList();
        if (!string.IsNullOrWhiteSpace(_treeFilter))
        {
            var f = _treeFilter.ToLowerInvariant();
            devices = devices
                .Where(d => d.Name.ToLowerInvariant().Contains(f) || d.Host.ToLowerInvariant().Contains(f) || (d.SerialNumber ?? "").ToLowerInvariant().Contains(f))
                .ToList();
        }

        var root = new TreeViewItem
        {
            Header = "Grupo padrão",
            IsExpanded = true
        };

        foreach (var dev in devices)
        {
            var devNode = new TreeViewItem
            {
                Header = $"{dev.Name}",
                Tag = dev,
                IsExpanded = false
            };

            for (int ch = 0; ch < Math.Max(1, dev.ChannelCount); ch++)
            {
                var chNode = new TreeViewItem
                {
                    Header = $"CH{ch + 1}",
                    Tag = new TreeChannel(dev, ch)
                };
                devNode.Items.Add(chNode);
            }

            root.Items.Add(devNode);
        }

        TvOrg.Items.Add(root);
    }

    private void TvOrg_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        var item = TvOrg.SelectedItem as TreeViewItem;
        if (item?.Tag is TreeChannel tc)
        {
            SelectTab(BtnTabLive);
            ShowLive();
            _mosaic?.AddCamera(tc.Device.Id, tc.Channel, tc.Device.Name);
        }
    }

    private void DeleteSelectedDeviceFromTree()
    {
        var item = TvOrg.SelectedItem as TreeViewItem;
        if (item?.Tag is Device dev)
        {
            if (MessageBox.Show($"Excluir o dispositivo '{dev.Name}'?", "Confirmar", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
                return;
            _deviceRepo.Delete(dev.Id);
            ReloadTree();
        }
    }

    private async System.Threading.Tasks.Task ToggleIaAsync()
    {
        if (_aiEngine.IsRunning)
        {
            _aiEngine.Stop();
            return;
        }

        BtnToggleIa.IsEnabled = false;
        BtnToggleIa.Content = "Conectando...";

        var ok = await _aiEngine.CheckHealthAsync();
        if (!ok)
        {
            MessageBox.Show($"Não foi possível conectar ao servidor de IA em:\n{_settings.Settings.IaServer.BaseUrl}\n\nVerifique se o servidor está rodando.",
                "IA Offline", MessageBoxButton.OK, MessageBoxImage.Warning);
            BtnToggleIa.IsEnabled = true;
            BtnToggleIa.Content = "Iniciar IA";
            return;
        }

        _aiEngine.Start();
        BtnToggleIa.IsEnabled = true;
    }

    private void OnAiStatusChanged(bool running)
    {
        Dispatcher.Invoke(() =>
        {
            IaIndicator.Fill = running ? Brushes.LimeGreen : Brushes.Gray;
            TxtIaStatus.Text = running ? "Rodando" : "Parada";
            BtnToggleIa.Content = running ? "Parar IA" : "Iniciar IA";
        });
    }

    private void OnAlarm(Models.AiEvent ev)
    {
        Dispatcher.Invoke(() =>
        {
            var login = _connectionManager.GetLogin(ev.DeviceId);
            if (login == IntPtr.Zero) return;

            var title = $"🚨 ALERTA • {ev.DeviceName} CH{ev.Channel + 1} • {ev.ClassName}";
            var info = $"{ev.RuleName} • {ev.EventType} • {ev.ConfidenceDisplay}";

            var fullscreen = new FullscreenWindow(ev.DeviceId, ev.Channel, login, title, info,
                _settings.Settings.Alarm.FullscreenDurationSec);

            var rules = _ruleRepo.GetByDeviceAndChannel(ev.DeviceId, ev.Channel);
            fullscreen.SetRules(rules);
            fullscreen.Show();
        });
    }

    protected override void OnClosed(EventArgs e)
    {
        _aiEngine.Stop();
        _connectionManager.Dispose();
        base.OnClosed(e);
    }

    private record TreeChannel(Device Device, int Channel);
}
