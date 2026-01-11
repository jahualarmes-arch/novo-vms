using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using VMS_AlarmesJahu.App.Models;
using VMS_AlarmesJahu.App.Services;
using VMS_AlarmesJahu.App.Sdk;

namespace VMS_AlarmesJahu.App.Views.Controls;

/// <summary>
/// CÃ©lula de vÃ­deo com suporte a overlay de regras
/// Usa snapshot JPEG em vez de HwndHost para permitir overlays WPF
/// Modo CleanMode: mostra apenas a imagem sem labels (igual SmartPSS Lite)
/// </summary>
public class VideoCell : Border
{
    // UI Components
    private readonly Image _videoImage;
    private readonly RuleOverlay _ruleOverlay;
    private readonly TextBlock _label;
    private readonly TextBlock _deviceLabel;
    private readonly Border _statusIndicator;
    private readonly Border _alarmIndicator;
    private readonly Grid _mainGrid;
    private readonly Grid _infoOverlay;
    private readonly Border _topBar;
    
    // Ãcone de cÃ¢mera (quando sem vÃ­deo) - Estilo SmartPSS
    private readonly Border _cameraIconContainer;

    // Video Control
    private HiddenNativeWindow? _hiddenWindow;
    private IntPtr _playHandle;
    private DispatcherTimer? _snapshotTimer;
    private ConnectionManager? _connectionManager;
    
    // State
    private long _deviceId;
    private int _channel;
    private string? _deviceName;
    private StreamProfile _streamProfile = StreamProfile.Extra;
    private bool _isAlarm;
    private bool _cleanMode;
    private List<AiRule> _rules = new();

    // Drag & Drop
    private bool _isDragging;
    private Point _dragStartPoint;

    // Events
    public event Action<VideoCell>? CellClicked;
    public event Action<VideoCell>? CellDoubleClicked;
    public event Action<VideoCell>? CellDragStarted;
    public event EventHandler<CameraDropEventArgs>? OnCameraDrop;

    // Properties
    public long DeviceId => _deviceId;
    public int Channel => _channel;
    public string? DeviceName => _deviceName;
    public bool IsPlaying => _playHandle != IntPtr.Zero;
    public IntPtr PlayHandle => _playHandle;
    public StreamProfile StreamProfile => _streamProfile;
    public int CellIndex { get; set; }

    // ✅ expõe o ConnectionManager para outras telas (ex: CleanMosaicWindow)
    public ConnectionManager? ConnectionManager => _connectionManager;
/// <summary>
    /// FPS para captura de snapshots (padrÃ£o 10)
    /// </summary>
    public int SnapshotFps { get; set; } = 10;

    /// <summary>
    /// Modo limpo: mostra apenas a imagem, sem labels ou overlays (igual SmartPSS Lite fullscreen)
    /// </summary>
    public bool CleanMode
    {
        get => _cleanMode;
        set
        {
            _cleanMode = value;
            _infoOverlay.Visibility = value ? Visibility.Collapsed : Visibility.Visible;
            _ruleOverlay.Visibility = value ? Visibility.Collapsed : Visibility.Visible;
        }
    }

    public VideoCell()
    {
        // Estilo SmartPSS Lite: fundo escuro
        Background = new SolidColorBrush(Color.FromRgb(15, 20, 27)); // #0F141B
        BorderBrush = new SolidColorBrush(Color.FromRgb(40, 50, 65));
        BorderThickness = new Thickness(1);
        Margin = new Thickness(1);
        AllowDrop = true;

        _mainGrid = new Grid();

        // Ãcone de cÃ¢mera centralizado (quando sem vÃ­deo) - Estilo SmartPSS
        _cameraIconContainer = new Border
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        
        // Criar Ã­cone de cÃ¢mera estilizado (similar ao SmartPSS)
        var cameraIcon = CreateCameraIcon();
        _cameraIconContainer.Child = cameraIcon;
        _mainGrid.Children.Add(_cameraIconContainer);

        // Imagem do vÃ­deo (snapshot) - UniformToFill para preencher toda cÃ©lula
        _videoImage = new Image
        {
            Stretch = Stretch.UniformToFill,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        _mainGrid.Children.Add(_videoImage);

        // Overlay de regras (linhas/polÃ­gonos)
        _ruleOverlay = new RuleOverlay();
        _mainGrid.Children.Add(_ruleOverlay);

        // Overlay de informaÃ§Ãµes (pode ser ocultado no modo limpo)
        _infoOverlay = new Grid { IsHitTestVisible = false };

        // Top bar com nome da cÃ¢mera
        _topBar = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(160, 0, 0, 0)),
            VerticalAlignment = VerticalAlignment.Top,
            Padding = new Thickness(8, 4, 8, 4)
        };

        var topStack = new StackPanel { Orientation = Orientation.Horizontal };
        
        _statusIndicator = new Border
        {
            Width = 8,
            Height = 8,
            CornerRadius = new CornerRadius(4),
            Background = Brushes.Gray,
            Margin = new Thickness(0, 0, 6, 0),
            VerticalAlignment = VerticalAlignment.Center
        };
        topStack.Children.Add(_statusIndicator);

        _label = new TextBlock
        {
            Text = "",
            Foreground = Brushes.White,
            FontSize = 11,
            FontWeight = FontWeights.SemiBold,
            VerticalAlignment = VerticalAlignment.Center
        };
        topStack.Children.Add(_label);

        _topBar.Child = topStack;
        _infoOverlay.Children.Add(_topBar);

        // Device label (canto inferior esquerdo)
        _deviceLabel = new TextBlock
        {
            Foreground = Brushes.White,
            FontSize = 10,
            Opacity = 0.7,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Bottom,
            Margin = new Thickness(8, 0, 0, 6)
        };
        _infoOverlay.Children.Add(_deviceLabel);

        // Indicador de alarme
        _alarmIndicator = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(180, 255, 0, 0)),
            VerticalAlignment = VerticalAlignment.Bottom,
            HorizontalAlignment = HorizontalAlignment.Right,
            CornerRadius = new CornerRadius(4),
            Padding = new Thickness(8, 4, 8, 4),
            Margin = new Thickness(0, 0, 8, 6),
            Visibility = Visibility.Collapsed,
            Child = new TextBlock
            {
                Text = "âš ï¸ ALERTA",
                Foreground = Brushes.White,
                FontSize = 10,
                FontWeight = FontWeights.Bold
            }
        };
        _infoOverlay.Children.Add(_alarmIndicator);

        _mainGrid.Children.Add(_infoOverlay);
        Child = _mainGrid;

        // Mouse events
        MouseLeftButtonDown += OnMouseDown;
        MouseLeftButtonUp += OnMouseUp;
        MouseMove += OnMouseMove;
        MouseEnter += (s, e) => { if (!_isAlarm && !_cleanMode) BorderBrush = new SolidColorBrush(Color.FromRgb(80, 100, 130)); };
        MouseLeave += (s, e) => { if (!_isAlarm) BorderBrush = new SolidColorBrush(Color.FromRgb(40, 50, 65)); };

        // Drag & Drop events
        DragEnter += OnDragEnter;
        DragLeave += OnDragLeave;
        Drop += OnDrop;
    }

    /// <summary>
    /// Cria Ã­cone de cÃ¢mera estilizado (similar ao SmartPSS Lite)
    /// </summary>
    private UIElement CreateCameraIcon()
    {
        var canvas = new Canvas
        {
            Width = 60,
            Height = 45,
            Opacity = 0.3
        };

        // Corpo da cÃ¢mera
        var body = new System.Windows.Shapes.Rectangle
        {
            Width = 40,
            Height = 30,
            Fill = new SolidColorBrush(Color.FromRgb(60, 70, 85)),
            RadiusX = 4,
            RadiusY = 4
        };
        Canvas.SetLeft(body, 0);
        Canvas.SetTop(body, 7);
        canvas.Children.Add(body);

        // Lente
        var lens = new System.Windows.Shapes.Ellipse
        {
            Width = 18,
            Height = 18,
            Fill = new SolidColorBrush(Color.FromRgb(45, 55, 70)),
            Stroke = new SolidColorBrush(Color.FromRgb(70, 80, 95)),
            StrokeThickness = 2
        };
        Canvas.SetLeft(lens, 11);
        Canvas.SetTop(lens, 13);
        canvas.Children.Add(lens);

        // Parte lateral (viewfinder)
        var viewfinder = new System.Windows.Shapes.Polygon
        {
            Fill = new SolidColorBrush(Color.FromRgb(60, 70, 85)),
            Points = new PointCollection
            {
                new Point(40, 12),
                new Point(60, 0),
                new Point(60, 25),
                new Point(40, 32)
            }
        };
        canvas.Children.Add(viewfinder);

        return canvas;
    }

    public void Initialize(ConnectionManager connectionManager)
    {
        _connectionManager = connectionManager;
    }

    /// <summary>
    /// Inicia o stream de vÃ­deo
    /// </summary>
    public void StartPlay(long deviceId, int channel, string? deviceName = null)
        => StartPlay(deviceId, channel, deviceName, StreamProfile.Extra);

    /// <summary>
    /// Inicia o stream de vídeo com perfil (Extra/Substream ou Principal/Main).
    /// </summary>
    public void StartPlay(long deviceId, int channel, string? deviceName, StreamProfile profile)
    {
        if (_connectionManager == null) return;

        StopPlay();

        _deviceId = deviceId;
        _channel = channel;
        _deviceName = deviceName;
        _label.Text = $"CH{channel + 1}";
        _deviceLabel.Text = deviceName ?? "";

        var login = _connectionManager.GetLogin(deviceId);
        if (login == IntPtr.Zero)
        {
            SetStatus(false);
            _label.Text = "Offline";
            return;
        }

        // Oculta Ã­cone de cÃ¢mera quando conectado
        _cameraIconContainer.Visibility = Visibility.Collapsed;

        // Cria janela oculta para o stream
        _hiddenWindow = new HiddenNativeWindow();

        _streamProfile = profile;

        // Inicia o play na janela oculta
        // 0 = principal (main), 1 = extra/substream
        _playHandle = IntelbrasSdk.RealPlay(login, channel, _hiddenWindow.Handle, (int)profile);

        if (_playHandle == IntPtr.Zero)
        {
            SetStatus(false);
            _label.Text = "Erro";
            _hiddenWindow?.Dispose();
            _hiddenWindow = null;
            _cameraIconContainer.Visibility = Visibility.Visible;
            return;
        }

        SetStatus(true);

        // Inicia timer para capturar snapshots
        StartSnapshotTimer();

        // Carrega regras da cÃ¢mera se disponÃ­vel
        LoadRulesForChannel();
    }

    /// <summary>
    /// Para o stream de vÃ­deo
    /// </summary>
    public void StopPlay()
    {
        StopSnapshotTimer();

        if (_playHandle != IntPtr.Zero)
        {
            IntelbrasSdk.StopRealPlay(_playHandle);
            _playHandle = IntPtr.Zero;
        }

        _hiddenWindow?.Dispose();
        _hiddenWindow = null;

        _videoImage.Source = null;
        _ruleOverlay.Clear();
        SetStatus(false);
        _label.Text = "";
        _deviceLabel.Text = "";
        _deviceId = 0;
        _channel = 0;
        
        // Mostra Ã­cone de cÃ¢mera novamente
        _cameraIconContainer.Visibility = Visibility.Visible;
    }

    /// <summary>
    /// Define as regras a serem exibidas no overlay
    /// </summary>
    public void SetRules(IEnumerable<AiRule> rules)
    {
        _rules.Clear();
        _rules.AddRange(rules);
        _ruleOverlay.SetRules(_rules);
    }

    /// <summary>
    /// Ativa/desativa indicador de alarme
    /// </summary>
    public void SetAlarm(bool alarm)
    {
        _isAlarm = alarm;
        
        if (alarm)
        {
            BorderBrush = Brushes.Red;
            BorderThickness = new Thickness(3);
            _alarmIndicator.Visibility = Visibility.Visible;

            // AnimaÃ§Ã£o de piscar
            var anim = new ColorAnimation
            {
                From = Colors.Red,
                To = Color.FromRgb(255, 100, 100),
                Duration = TimeSpan.FromMilliseconds(300),
                AutoReverse = true,
                RepeatBehavior = new RepeatBehavior(5)
            };
            BorderBrush.BeginAnimation(SolidColorBrush.ColorProperty, anim);
        }
        else
        {
            BorderBrush = new SolidColorBrush(Color.FromRgb(40, 50, 65));
            BorderThickness = new Thickness(1);
            _alarmIndicator.Visibility = Visibility.Collapsed;
        }
    }

    /// <summary>
    /// Retorna os dados da cÃ¢mera para drag & drop
    /// </summary>
    public CameraDragData GetDragData()
    {
        return new CameraDragData
        {
            DeviceId = _deviceId,
            Channel = _channel,
            DeviceName = _deviceName,
            SourceCellIndex = CellIndex
        };
    }

    /// <summary>
    /// Aplica dados de cÃ¢mera (usado no drop)
    /// </summary>
    public void ApplyDragData(CameraDragData data)
    {
        if (_connectionManager == null) return;
        StartPlay(data.DeviceId, data.Channel, data.DeviceName);
    }

    private void StartSnapshotTimer()
    {
        StopSnapshotTimer();

        var interval = 1000 / Math.Max(1, SnapshotFps);
        _snapshotTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(interval)
        };
        _snapshotTimer.Tick += OnSnapshotTick;
        _snapshotTimer.Start();
    }

    private void StopSnapshotTimer()
    {
        if (_snapshotTimer != null)
        {
            _snapshotTimer.Stop();
            _snapshotTimer.Tick -= OnSnapshotTick;
            _snapshotTimer = null;
        }
    }

    private void OnSnapshotTick(object? sender, EventArgs e)
    {
        if (_playHandle == IntPtr.Zero) return;

        try
        {
            var jpegData = IntelbrasSdk.SnapshotJpegBytesFromPlay(_playHandle, 70);
            if (jpegData != null)
            {
                var bitmap = ImageUtil.BytesToBitmapSource(jpegData);
                if (bitmap != null)
                {
                    _videoImage.Source = bitmap;
                }
            }
        }
        catch
        {
            // Ignora erros de snapshot
        }
    }

    private void LoadRulesForChannel()
    {
        // As regras serÃ£o carregadas externamente pelo MosaicView
        _ruleOverlay.Refresh();
    }

    private void SetStatus(bool connected)
    {
        _statusIndicator.Background = connected ? Brushes.LimeGreen : Brushes.Gray;
    }

    #region Mouse Events

    private void OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        _dragStartPoint = e.GetPosition(this);
        _isDragging = false;

        if (e.ClickCount == 2)
        {
            CellDoubleClicked?.Invoke(this);
            e.Handled = true;
        }
    }

    private void OnMouseUp(object sender, MouseButtonEventArgs e)
    {
        if (!_isDragging)
        {
            CellClicked?.Invoke(this);
        }
        _isDragging = false;
    }

    private void OnMouseMove(object sender, MouseEventArgs e)
    {
        if (e.LeftButton != MouseButtonState.Pressed || !IsPlaying) return;

        var pos = e.GetPosition(this);
        var diff = pos - _dragStartPoint;

        if (Math.Abs(diff.X) > 10 || Math.Abs(diff.Y) > 10)
        {
            if (!_isDragging)
            {
                _isDragging = true;
                CellDragStarted?.Invoke(this);
                
                var data = new DataObject("CameraDragData", GetDragData());
                DragDrop.DoDragDrop(this, data, DragDropEffects.Move);
            }
        }
    }

    #endregion

    #region Drag & Drop Events

    private void OnDragEnter(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent("CameraDragData"))
        {
            BorderBrush = new SolidColorBrush(Colors.DodgerBlue);
            BorderThickness = new Thickness(2);
            e.Effects = DragDropEffects.Move;
        }
        else
        {
            e.Effects = DragDropEffects.None;
        }
        e.Handled = true;
    }

    private void OnDragLeave(object sender, DragEventArgs e)
    {
        if (!_isAlarm)
        {
            BorderBrush = new SolidColorBrush(Color.FromRgb(40, 50, 65));
            BorderThickness = new Thickness(1);
        }
    }

    private void OnDrop(object sender, DragEventArgs e)
    {
        if (!_isAlarm)
        {
            BorderBrush = new SolidColorBrush(Color.FromRgb(40, 50, 65));
            BorderThickness = new Thickness(1);
        }

        if (e.Data.GetDataPresent("CameraDragData"))
        {
            var data = e.Data.GetData("CameraDragData") as CameraDragData;
            if (data != null && data.SourceCellIndex != CellIndex)
            {
                // O MosaicView vai tratar a troca
                var args = new CameraDropEventArgs
                {
                    SourceData = data,
                    TargetCellIndex = CellIndex
                };
                OnCameraDrop?.Invoke(this, args);
            }
        }
        e.Handled = true;
    }

    #endregion
}

/// <summary>
/// Dados para drag & drop de cÃ¢mera
/// </summary>
public class CameraDragData
{
    public long DeviceId { get; set; }
    public int Channel { get; set; }
    public string? DeviceName { get; set; }
    public int SourceCellIndex { get; set; }
}

/// <summary>
/// Args do evento de drop de cÃ¢mera
/// </summary>
public class CameraDropEventArgs : EventArgs
{
    public CameraDragData? SourceData { get; set; }
    public int TargetCellIndex { get; set; }
}

