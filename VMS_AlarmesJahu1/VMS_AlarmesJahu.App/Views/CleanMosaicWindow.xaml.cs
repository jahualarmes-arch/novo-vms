using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using VMS_AlarmesJahu.App.Data;
using VMS_AlarmesJahu.App.Models;
using VMS_AlarmesJahu.App.Services;
using VMS_AlarmesJahu.App.Views.Controls;

namespace VMS_AlarmesJahu.App.Views;

/// <summary>
/// Janela fullscreen limpa estilo SmartPSS Lite
/// Mostra apenas as câmeras em mosaico sem nenhuma interface adicional
/// </summary>
public partial class CleanMosaicWindow : Window
{
    private readonly ConnectionManager? _connectionManager;
    private readonly DeviceRepository? _deviceRepo;
    private readonly List<VideoCell> _cells = new();
    private int _currentLayout = 4; // 4x4 padrão
    private DispatcherTimer? _hintTimer;

    /// <summary>
    /// Construtor sem parâmetros (para uso com AddCamera)
    /// </summary>
    public CleanMosaicWindow()
    {
        InitializeComponent();

        // Criar grid inicial
        SetLayout(_currentLayout);

        // Esconder dica após 3 segundos
        _hintTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(3) };
        _hintTimer.Tick += (s, e) =>
        {
            HintBorder.Visibility = Visibility.Collapsed;
            _hintTimer?.Stop();
        };
        _hintTimer.Start();

        Loaded += OnLoaded;
        Closing += OnClosing;
    }

    public CleanMosaicWindow(ConnectionManager connectionManager, DeviceRepository deviceRepo) : this()
    {
        _connectionManager = connectionManager;
        _deviceRepo = deviceRepo;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        // Iniciar todas as câmeras automaticamente
        StartAllCameras();
    }

    private void OnClosing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        // Parar todos os vídeos ao fechar
        StopAllCameras();
    }

    /// <summary>
    /// Define o layout do mosaico (1x1 até 8x8)
    /// </summary>
    public void SetLayout(int gridSize)
    {
        _currentLayout = Math.Clamp(gridSize, 1, 8);
        
        // Parar vídeos existentes
        foreach (var cell in _cells)
        {
            cell.StopPlay();
        }
        _cells.Clear();
        VideoGrid.Children.Clear();

        // Criar novo grid
        VideoGrid.Rows = _currentLayout;
        VideoGrid.Columns = _currentLayout;

        int totalCells = _currentLayout * _currentLayout;
        for (int i = 0; i < totalCells; i++)
        {
            var cell = new VideoCell
            {
                CellIndex = i,
                CleanMode = true, // IMPORTANTE: Modo limpo sem labels
                SnapshotFps = 12
            };
            
            // Só inicializa se tiver ConnectionManager
            if (_connectionManager != null)
            {
                cell.Initialize(_connectionManager);
            }
            
            cell.CellDoubleClicked += OnCellDoubleClicked;
            
            _cells.Add(cell);
            VideoGrid.Children.Add(cell);
        }
    }

    /// <summary>
    /// ✅ NOVO: Adiciona uma câmera específica na próxima célula vazia
    /// </summary>
    public void AddCamera(long deviceId, int channel, string deviceName, ConnectionManager connectionManager)
    {
        if (_cells.Count == 0) return;
        
        var emptyCell = _cells.FirstOrDefault(c => !c.IsPlaying);
        if (emptyCell == null)
        {
            // Se não tem célula vazia, para a primeira e usa ela
            emptyCell = _cells[0];
            emptyCell.StopPlay();
        }

        // Inicializa a célula se necessário
        if (emptyCell.ConnectionManager == null)
        {
            emptyCell.Initialize(connectionManager);
        }

        emptyCell.StartPlay(deviceId, channel, deviceName);
    }

    /// <summary>
    /// Inicia todas as câmeras de todos os DVRs
    /// </summary>
    public void StartAllCameras()
    {
        if (_deviceRepo == null || _connectionManager == null) return;
        
        var devices = _deviceRepo.GetAll();
        int cellIndex = 0;

        foreach (var device in devices)
        {
            if (!device.Enabled) continue;

            // Conectar ao dispositivo
            var login = _connectionManager.Connect(device.Id);
            if (login == IntPtr.Zero) continue;

            // Adicionar cada canal do dispositivo
            int channelCount = _connectionManager.GetChannelCount(device.Id);
            for (int ch = 0; ch < channelCount && cellIndex < _cells.Count; ch++)
            {
                _cells[cellIndex].StartPlay(device.Id, ch, device.Name);
                cellIndex++;
            }
        }
    }

    /// <summary>
    /// Inicia câmeras de um dispositivo específico
    /// </summary>
    public void StartDeviceCameras(long deviceId)
    {
        if (_deviceRepo == null || _connectionManager == null) return;
        
        var device = _deviceRepo.GetById(deviceId);
        if (device == null) return;

        var login = _connectionManager.Connect(device.Id);
        if (login == IntPtr.Zero) return;

        int channelCount = _connectionManager.GetChannelCount(device.Id);
        int cellIndex = 0;

        for (int ch = 0; ch < channelCount && cellIndex < _cells.Count; ch++)
        {
            _cells[cellIndex].StartPlay(device.Id, ch, device.Name);
            cellIndex++;
        }
    }

    /// <summary>
    /// Para todas as câmeras
    /// </summary>
    public void StopAllCameras()
    {
        foreach (var cell in _cells)
        {
            cell.StopPlay();
        }
    }

    private void OnCellDoubleClicked(VideoCell cell)
    {
        // Duplo clique: abrir câmera em tela cheia (1x1)
        if (cell.IsPlaying)
        {
            if (_currentLayout == 1)
            {
                // Voltar para 4x4 (mosaico) -> stream EXTRA
                SetLayout(4);
                StartAllCameras();
            }
            else
            {
                // Mostrar apenas esta câmera
                var deviceId = cell.DeviceId;
                var channel = cell.Channel;
                var deviceName = cell.DeviceName;

                SetLayout(1);
                // 1x1 em fullscreen -> stream PRINCIPAL
                _cells[0].StartPlay(deviceId, channel, deviceName, StreamProfile.Principal);
            }
        }
    }

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Escape:
                Close();
                break;

            case Key.F11:
                if (WindowState == WindowState.Maximized)
                {
                    WindowState = WindowState.Normal;
                    WindowStyle = WindowStyle.SingleBorderWindow;
                }
                else
                {
                    WindowStyle = WindowStyle.None;
                    WindowState = WindowState.Maximized;
                }
                break;

            // Números 1-8 para mudar layout
            case Key.D1:
            case Key.NumPad1:
                SetLayout(1);
                StartAllCameras();
                break;

            case Key.D2:
            case Key.NumPad2:
                SetLayout(2);
                StartAllCameras();
                break;

            case Key.D3:
            case Key.NumPad3:
                SetLayout(3);
                StartAllCameras();
                break;

            case Key.D4:
            case Key.NumPad4:
                SetLayout(4);
                StartAllCameras();
                break;

            case Key.D5:
            case Key.NumPad5:
                SetLayout(5);
                StartAllCameras();
                break;

            case Key.D6:
            case Key.NumPad6:
                SetLayout(6);
                StartAllCameras();
                break;

            case Key.D7:
            case Key.NumPad7:
                SetLayout(7);
                StartAllCameras();
                break;

            case Key.D8:
            case Key.NumPad8:
                SetLayout(8);
                StartAllCameras();
                break;
        }
    }
}
