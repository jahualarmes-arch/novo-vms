using System;
using System.Collections.Generic;
using System.Linq;
using Serilog;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms; // Para Screen
using VMS_AlarmesJahu.App.Data;
using VMS_AlarmesJahu.App.IA;
using VMS_AlarmesJahu.App.Models;
using VMS_AlarmesJahu.App.Services;
using VMS_AlarmesJahu.App.Views.Controls;

namespace VMS_AlarmesJahu.App.Views;

public partial class MosaicView : System.Windows.Controls.UserControl
{
    private ConnectionManager? _connectionManager;
    private DeviceRepository? _deviceRepo;
    private AiRuleRepository? _ruleRepo;
    private AiEngine? _aiEngine;
    private readonly List<VideoCell> _cells = new();
    private int _gridSize = 2;
    private int _snapshotFps = 8;

    // Lista de janelas de mosaico abertas em outros monitores
    private readonly List<MosaicWindow> _mosaicWindows = new();

    public MosaicView()
    {
        InitializeComponent();
        
        CbLayout.SelectionChanged += (s, e) => UpdateMosaicLayout();
        CbDevice.SelectionChanged += (s, e) => OnDeviceChanged();
        
        BtnStartAll.Click += (s, e) => StartAllFromDevice();
        BtnStopAll.Click += (s, e) => StopAll();
        BtnClearAll.Click += (s, e) => ClearAll();
        BtnAddCamera.Click += (s, e) => AddSingleCamera();
        BtnAddAllChannels.Click += (s, e) => AddAllChannelsFromDevice();
        BtnNewWindow.Click += (s, e) => OpenNewMosaicWindow();
        BtnTestAlarm.Click += (s, e) => TestAlarm();
        BtnFullscreen.Click += (s, e) => OpenFullscreen(); // âœ… NOVO: BotÃ£o fullscreen
        
        ChkShowRules.Checked += (s, e) => RefreshRulesOverlay();
        ChkShowRules.Unchecked += (s, e) => RefreshRulesOverlay();
        
        Loaded += (s, e) => Initialize();
    }

    public void Initialize(ConnectionManager? connMgr = null, DeviceRepository? deviceRepo = null, 
                          AiRuleRepository? ruleRepo = null, AiEngine? aiEngine = null)
    {
        if (connMgr != null) _connectionManager = connMgr;
        if (deviceRepo != null) _deviceRepo = deviceRepo;
        if (ruleRepo != null) _ruleRepo = ruleRepo;
        if (aiEngine != null)
        {
            _aiEngine = aiEngine;
            _aiEngine.OnAlarm += OnAlarm;
        }
        
        LoadMonitors();
        LoadDevices();
        UpdateMosaicLayout();
    }

    /// <summary>
    /// Adiciona uma cÃ¢mera especÃ­fica (DeviceId/Channel) na prÃ³xima cÃ©lula vazia.
    /// Usado pela Ã¡rvore lateral (estilo SmartPSS).
    /// </summary>
    public void AddCamera(long deviceId, int channel, string deviceName)
    {
        if (_connectionManager == null) return;
        if (channel < 0) return;

        var emptyCell = _cells.FirstOrDefault(c => !c.IsPlaying);
        if (emptyCell == null)
        {
            System.Windows.MessageBox.Show("NÃ£o hÃ¡ cÃ©lulas vazias. Aumente o layout ou pare uma cÃ¢mera.");
            return;
        }

        emptyCell.StartPlay(deviceId, channel, deviceName);
        LoadRulesForCell(emptyCell);
    }

    /// <summary>
    /// Carrega lista de monitores disponÃ­veis
    /// </summary>
    private void LoadMonitors()
    {
        CbMonitor.Items.Clear();
        var screens = Screen.AllScreens;
        for (int i = 0; i < screens.Length; i++)
        {
            var s = screens[i];
            var name = s.Primary ? $"Monitor {i + 1} (Principal)" : $"Monitor {i + 1}";
            name += $" - {s.Bounds.Width}x{s.Bounds.Height}";
            CbMonitor.Items.Add(new ComboBoxItem { Content = name, Tag = i });
        }
        if (CbMonitor.Items.Count > 0)
            CbMonitor.SelectedIndex = 0;
    }

    /// <summary>
    /// Carrega lista de dispositivos (DVRs)
    /// </summary>
    private void LoadDevices()
    {
        if (_deviceRepo == null) return;
        var devices = _deviceRepo.GetAll();
        CbDevice.ItemsSource = devices;
        if (devices.Count > 0)
            CbDevice.SelectedIndex = 0;
    }

    /// <summary>
    /// Atualiza lista de canais quando muda o DVR
    /// </summary>
    private void OnDeviceChanged()
    {
        var device = CbDevice.SelectedItem as Device;
        if (device == null) return;

        CbChannel.Items.Clear();
        for (int i = 0; i < device.ChannelCount; i++)
        {
            CbChannel.Items.Add($"Canal {i + 1}");
        }
        if (CbChannel.Items.Count > 0)
            CbChannel.SelectedIndex = 0;
    }

    /// <summary>
    /// Atualiza o layout do mosaico (1x1 atÃ© 8x8)
    /// </summary>
    private void UpdateMosaicLayout()
    {
        var item = CbLayout.SelectedItem as ComboBoxItem;
        if (item?.Tag != null && int.TryParse(item.Tag.ToString(), out int size))
        {
            _gridSize = size;
        }
        else
        {
            _gridSize = 2;
        }

        // Salvar cÃ¢meras atuais
        var currentCameras = _cells
            .Where(c => c.IsPlaying)
            .Select(c => new { c.DeviceId, c.Channel, c.DeviceName, Index = c.CellIndex })
            .ToList();

        // Parar todos os vÃ­deos existentes
        foreach (var cell in _cells)
        {
            cell.StopPlay();
            cell.OnCameraDrop -= OnCameraDrop;
        }
        _cells.Clear();

        VideoGrid.Children.Clear();
        VideoGrid.Rows = _gridSize;
        VideoGrid.Columns = _gridSize;

        // Criar cÃ©lulas
        var total = _gridSize * _gridSize;
        for (int i = 0; i < total; i++)
        {
            var cell = new VideoCell
            {
                CellIndex = i,
                SnapshotFps = _snapshotFps
            };
            
            if (_connectionManager != null)
                cell.Initialize(_connectionManager);
            
            cell.CellClicked += OnCellClick;
            cell.CellDoubleClicked += OnCellDoubleClick;
            cell.OnCameraDrop += OnCameraDrop;
            
            _cells.Add(cell);
            VideoGrid.Children.Add(cell);
        }

        // Restaurar cÃ¢meras que cabem no novo layout
        foreach (var cam in currentCameras.Where(c => c.Index < total))
        {
            _cells[cam.Index].StartPlay(cam.DeviceId, cam.Channel, cam.DeviceName);
            LoadRulesForCell(_cells[cam.Index]);
        }
    }

    /// <summary>
    /// Inicia todas as cÃ¢meras do DVR selecionado
    /// </summary>
    private void StartAllFromDevice()
    {
        var device = CbDevice.SelectedItem as Device;
        if (device == null || _connectionManager == null) return;

        var emptyCells = _cells.Where(c => !c.IsPlaying).ToList();
        var channelCount = Math.Min(device.ChannelCount, emptyCells.Count);

        for (int i = 0; i < channelCount; i++)
        {
            emptyCells[i].StartPlay(device.Id, i, device.Name);
            LoadRulesForCell(emptyCells[i]);
        }
    }

    /// <summary>
    /// Para todas as cÃ¢meras
    /// </summary>
    private void StopAll()
    {
        foreach (var cell in _cells)
            cell.StopPlay();
    }

    /// <summary>
    /// Limpa todas as cÃ©lulas
    /// </summary>
    private void ClearAll()
    {
        StopAll();
    }

    /// <summary>
    /// Adiciona uma Ãºnica cÃ¢mera na prÃ³xima cÃ©lula vazia
    /// </summary>
    private void AddSingleCamera()
    {
        var device = CbDevice.SelectedItem as Device;
        if (device == null || _connectionManager == null) return;

        var channel = CbChannel.SelectedIndex;
        if (channel < 0) return;

        // Encontra primeira cÃ©lula vazia
        var emptyCell = _cells.FirstOrDefault(c => !c.IsPlaying);
        if (emptyCell == null)
        {
            System.Windows.MessageBox.Show("NÃ£o hÃ¡ cÃ©lulas vazias. Aumente o layout ou pare uma cÃ¢mera.");
            return;
        }

        emptyCell.StartPlay(device.Id, channel, device.Name);
        LoadRulesForCell(emptyCell);
    }

    /// <summary>
    /// Adiciona todos os canais do DVR selecionado
    /// </summary>
    private void AddAllChannelsFromDevice()
    {
        StartAllFromDevice();
    }

    /// <summary>
    /// Abre nova janela de mosaico em outro monitor
    /// </summary>
    private void OpenNewMosaicWindow()
    {
        var selectedItem = CbMonitor.SelectedItem as ComboBoxItem;
        if (selectedItem?.Tag == null) return;

        var monitorIndex = (int)selectedItem.Tag;
        var screens = Screen.AllScreens;
        if (monitorIndex >= screens.Length) return;

        var screen = screens[monitorIndex];
        
        var window = new MosaicWindow();
        window.Initialize(_connectionManager, _deviceRepo, _ruleRepo, _aiEngine);
        
        // IMPORTANTE: Definir posiÃ§Ã£o ANTES de mostrar a janela
        // e NÃƒO usar WindowState.Maximized inicialmente
        window.WindowStartupLocation = WindowStartupLocation.Manual;
        window.WindowState = WindowState.Normal;
        
        // Posiciona no monitor selecionado
        window.Left = screen.WorkingArea.Left;
        window.Top = screen.WorkingArea.Top;
        window.Width = screen.WorkingArea.Width;
        window.Height = screen.WorkingArea.Height;
        
        window.Closed += (s, e) => _mosaicWindows.Remove(window);
        _mosaicWindows.Add(window);
        
        // Mostra a janela primeiro
        window.Show();
        
        // Depois maximiza (jÃ¡ no monitor correto)
        window.WindowState = WindowState.Maximized;
    }

    /// <summary>
    /// Carrega regras de IA para uma cÃ©lula especÃ­fica
    /// </summary>
    private void LoadRulesForCell(VideoCell cell)
    {
        if (_ruleRepo == null || ChkShowRules.IsChecked != true) return;

        var rules = _ruleRepo.GetByDeviceAndChannel(cell.DeviceId, cell.Channel);
        cell.SetRules(rules);
    }

    /// <summary>
    /// Atualiza overlay de regras em todas as cÃ©lulas
    /// </summary>
    private void RefreshRulesOverlay()
    {
        foreach (var cell in _cells.Where(c => c.IsPlaying))
        {
            if (ChkShowRules.IsChecked == true)
            {
                LoadRulesForCell(cell);
            }
            else
            {
                cell.SetRules(new List<AiRule>());
            }
        }
    }

    /// <summary>
    /// Evento de clique em cÃ©lula
    /// </summary>
    private void OnCellClick(VideoCell cell)
    {
        // Seleciona o DVR e canal correspondente
        if (cell.IsPlaying && cell.DeviceId > 0)
        {
            var device = (CbDevice.ItemsSource as IEnumerable<Device>)?
                .FirstOrDefault(d => d.Id == cell.DeviceId);
            if (device != null)
            {
                CbDevice.SelectedItem = device;
                if (cell.Channel < CbChannel.Items.Count)
                    CbChannel.SelectedIndex = cell.Channel;
            }
        }
    }

    /// <summary>
    /// Evento de duplo-clique em cÃ©lula (abre fullscreen)
    /// </summary>
    private void OnCellDoubleClick(VideoCell cell)
    {
        if (!cell.IsPlaying) return;
        
        var login = _connectionManager?.GetLogin(cell.DeviceId) ?? IntPtr.Zero;
        if (login == IntPtr.Zero) return;

        var device = _deviceRepo?.GetById(cell.DeviceId);
        var title = $"{device?.Name ?? "DVR"} - CH{cell.Channel + 1}";
        
        // Para evitar 2 conexões simultâneas (sub + main) na mesma câmera,
        // paramos o tile do mosaico e abrimos o fullscreen em principal.
        var deviceId = cell.DeviceId;
        var channel = cell.Channel;
        var deviceName = cell.DeviceName;

        cell.StopPlay();

        var win = new FullscreenWindow(deviceId, channel, login, title, "Visualização manual");
        
        // Passa as regras para a fullscreen tambÃ©m
        if (_ruleRepo != null && ChkShowRules.IsChecked == true)
        {
            var rules = _ruleRepo.GetByDeviceAndChannel(cell.DeviceId, cell.Channel);
            win.SetRules(rules);
        }
        
        win.Closed += (s, e) =>
        {
            // Ao fechar o fullscreen, voltamos a câmera para o mosaico em Stream Extra.
            cell.StartPlay(deviceId, channel, deviceName, StreamProfile.Extra);
            LoadRulesForCell(cell);
        };

        win.Show();
    }

    /// <summary>
    /// Evento de drag & drop entre cÃ©lulas
    /// </summary>
    private void OnCameraDrop(object? sender, CameraDropEventArgs e)
    {
        if (e.SourceData == null) return;

        var sourceIndex = e.SourceData.SourceCellIndex;
        var targetIndex = e.TargetCellIndex;

        if (sourceIndex < 0 || sourceIndex >= _cells.Count) return;
        if (targetIndex < 0 || targetIndex >= _cells.Count) return;
        if (sourceIndex == targetIndex) return;

        var sourceCell = _cells[sourceIndex];
        var targetCell = _cells[targetIndex];

        // Guarda dados de ambas as cÃ©lulas
        var sourceData = sourceCell.GetDragData();
        var targetPlaying = targetCell.IsPlaying;
        CameraDragData? targetData = targetPlaying ? targetCell.GetDragData() : null;

        // Para ambas
        sourceCell.StopPlay();
        targetCell.StopPlay();

        // Troca posiÃ§Ãµes
        targetCell.StartPlay(sourceData.DeviceId, sourceData.Channel, sourceData.DeviceName);
        LoadRulesForCell(targetCell);

        if (targetData != null)
        {
            sourceCell.StartPlay(targetData.DeviceId, targetData.Channel, targetData.DeviceName);
            LoadRulesForCell(sourceCell);
        }
    }

    /// <summary>
    /// Evento de alarme da IA
    /// </summary>
    private void OnAlarm(AiEvent ev)
    {
        Dispatcher.Invoke(() =>
        {
            // Encontrar cÃ©lula correspondente e piscar
            var cell = _cells.FirstOrDefault(c => c.DeviceId == ev.DeviceId && c.Channel == ev.Channel);
            if (cell != null)
            {
                cell.SetAlarm(true);
                
                // Remover indicador apÃ³s 5 segundos
                var timer = new System.Windows.Threading.DispatcherTimer
                {
                    Interval = TimeSpan.FromSeconds(5)
                };
                timer.Tick += (s, e) =>
                {
                    cell.SetAlarm(false);
                    timer.Stop();
                };
                timer.Start();
            }

            // Abrir fullscreen
            OpenFullscreen(ev);
        });
    }

    /// <summary>
    /// Abre janela fullscreen para um evento
    /// </summary>
    private void OpenFullscreen(AiEvent ev)
    {
        var login = _connectionManager?.GetLogin(ev.DeviceId) ?? IntPtr.Zero;
        if (login == IntPtr.Zero) return;

        var title = $"ðŸš¨ ALERTA â€¢ {ev.DeviceName} CH{ev.Channel + 1} â€¢ {ev.ClassName}";
        var info = $"{ev.RuleName} â€¢ {ev.EventType} â€¢ {ev.Timestamp:HH:mm:ss}";

        var win = new FullscreenWindow(ev.DeviceId, ev.Channel, login, title, info);
        
        // Passa as regras
        if (_ruleRepo != null)
        {
            var rules = _ruleRepo.GetByDeviceAndChannel(ev.DeviceId, ev.Channel);
            win.SetRules(rules);
        }
        
        win.Show();
    }

    /// <summary>
    /// Testa alarme manualmente
    /// </summary>
    private void TestAlarm()
    {
        var device = CbDevice.SelectedItem as Device;
        if (device == null)
        {
            System.Windows.MessageBox.Show("Selecione um DVR primeiro.");
            return;
        }

        var ev = new AiEvent
        {
            DeviceId = device.Id,
            DeviceName = device.Name,
            Channel = CbChannel.SelectedIndex >= 0 ? CbChannel.SelectedIndex : 0,
            RuleName = "TESTE",
            ClassName = "person",
            EventType = "MANUAL",
            Confidence = 0.99,
            Timestamp = DateTime.Now
        };

        OnAlarm(ev);
    }

    /// <summary>
    /// Retorna lista de cÃ¢meras ativas (para IA)
    /// </summary>
    public List<(long DeviceId, int Channel, IntPtr PlayHandle)> GetActivePlays()
    {
        var plays = _cells
            .Where(c => c.IsPlaying)
            .Select(c => (c.DeviceId, c.Channel, c.PlayHandle))
            .ToList();

        // Inclui janelas de mosaico extras
        foreach (var win in _mosaicWindows)
        {
            plays.AddRange(win.GetActivePlays());
        }

        return plays;
    }

    /// <summary>
    /// âœ… NOVO: Abre janela fullscreen com apenas as cÃ¢meras
    /// </summary>
    private void OpenFullscreen()
    {
        try
        {
            // Cria nova janela fullscreen limpa
            var fullscreenWindow = new CleanMosaicWindow();
            
            // Passa o layout atual
            fullscreenWindow.SetLayout(_gridSize);
            
            // Copia as cÃ©lulas ativas para a janela fullscreen
            var activeCells = _cells.Where(c => c.IsPlaying).ToList();
            
            foreach (var cell in activeCells)
            {
                if (_connectionManager != null)
                {
                    fullscreenWindow.AddCamera(cell.DeviceId, cell.Channel, cell.DeviceName, _connectionManager);
                }
            }
            
            // Mostra a janela fullscreen
            fullscreenWindow.Show();
            
            Log.Information("Fullscreen aberto com {Count} cÃ¢meras ativas", activeCells.Count);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Erro ao abrir fullscreen");
            System.Windows.MessageBox.Show($"Erro ao abrir fullscreen: {ex.Message}", 
                "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}


