using System;
using System.Collections.Generic;
using System.Media;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using VMS_AlarmesJahu.App.Models;
using VMS_AlarmesJahu.App.Sdk;
using VMS_AlarmesJahu.App.Views.Controls;

namespace VMS_AlarmesJahu.App.Views;

public partial class FullscreenWindow : Window
{
    private readonly int _channel;
    private readonly IntPtr _login;
    private IntPtr _playHandle;
    private HiddenNativeWindow? _hiddenWindow;
    private DispatcherTimer? _countdownTimer;
    private DispatcherTimer? _flashTimer;
    private DispatcherTimer? _snapshotTimer;
    private int _remainingSeconds;
    private bool _isRed = true;
    private List<AiRule> _rules = new();

    public FullscreenWindow(long deviceId, int channel, IntPtr login, string title, string info, int durationSec = 30)
    {
        InitializeComponent();
        _channel = channel;
        _login = login;
        _remainingSeconds = durationSec;

        TxtTitle.Text = title;
        TxtInfo.Text = info;
        TxtTimer.Text = _remainingSeconds.ToString();

        Loaded += (s, e) => Start();
        Closed += (s, e) => Stop();
    }

    /// <summary>
    /// Define as regras a serem exibidas no overlay
    /// </summary>
    public void SetRules(IEnumerable<AiRule> rules)
    {
        _rules.Clear();
        _rules.AddRange(rules);
        RulesOverlay.SetRules(_rules);
    }

    private void Start()
    {
        // Tocar bipes
        _ = PlayBeepsAsync();

        // Criar janela oculta para o stream
        _hiddenWindow = new HiddenNativeWindow();

        // Iniciar vídeo na janela oculta
        Dispatcher.InvokeAsync(() =>
        {
            if (_hiddenWindow.Handle != IntPtr.Zero)
            {
                _playHandle = IntelbrasSdk.RealPlay(_login, _channel, _hiddenWindow.Handle, 0);
                
                if (_playHandle != IntPtr.Zero)
                {
                    StartSnapshotTimer();
                }
            }
        }, DispatcherPriority.Loaded);

        // Timer de contagem regressiva
        _countdownTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _countdownTimer.Tick += (s, e) =>
        {
            _remainingSeconds--;
            TxtTimer.Text = _remainingSeconds.ToString();
            if (_remainingSeconds <= 0)
            {
                _countdownTimer.Stop();
                Close();
            }
        };
        _countdownTimer.Start();

        // Timer de flash da borda
        _flashTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
        _flashTimer.Tick += (s, e) =>
        {
            _isRed = !_isRed;
            AlertBorder.BorderBrush = _isRed ? Brushes.Red : Brushes.DarkRed;
        };
        _flashTimer.Start();

        // Aplica regras se já foram definidas
        if (_rules.Count > 0)
        {
            RulesOverlay.SetRules(_rules);
        }
    }

    private void StartSnapshotTimer()
    {
        // 10 FPS para fullscreen (mais fluido)
        _snapshotTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
        _snapshotTimer.Tick += OnSnapshotTick;
        _snapshotTimer.Start();
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
                    VideoImage.Source = bitmap;
                }
            }
        }
        catch
        {
            // Ignora erros de snapshot
        }
    }

    private async Task PlayBeepsAsync()
    {
        await Task.Run(() =>
        {
            for (int i = 0; i < 3; i++)
            {
                try { Console.Beep(1200, 250); }
                catch { SystemSounds.Exclamation.Play(); }
                Thread.Sleep(120);
            }
        });
    }

    private void Stop()
    {
        _countdownTimer?.Stop();
        _flashTimer?.Stop();
        
        if (_snapshotTimer != null)
        {
            _snapshotTimer.Stop();
            _snapshotTimer.Tick -= OnSnapshotTick;
            _snapshotTimer = null;
        }

        if (_playHandle != IntPtr.Zero)
        {
            IntelbrasSdk.StopRealPlay(_playHandle);
            _playHandle = IntPtr.Zero;
        }

        _hiddenWindow?.Dispose();
        _hiddenWindow = null;
    }

    private void BtnClose_Click(object sender, RoutedEventArgs e) => Close();
    private void Window_KeyDown(object sender, KeyEventArgs e) { if (e.Key == Key.Escape) Close(); }
}
