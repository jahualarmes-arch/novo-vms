using System;
using System.Windows;
using System.Windows.Controls;

namespace VMS_AlarmesJahu.App.Views;

public partial class HomeView : UserControl
{
    public event Action? OpenDeviceManager;
    public event Action? OpenLogs;
    public event Action? OpenEventConfig;
    public event Action? OpenManual;
    public event Action? OpenLiveView;
    public event Action? OpenFullscreen;
    public event Action? OpenPlayback;

    public HomeView()
    {
        InitializeComponent();

        BtnDeviceManager.Click += (s, e) => OpenDeviceManager?.Invoke();
        BtnLogs.Click += (s, e) => OpenLogs?.Invoke();
        BtnEventConfig.Click += (s, e) => OpenEventConfig?.Invoke();
        BtnManual.Click += (s, e) => OpenManual?.Invoke();
        BtnLiveView.Click += (s, e) => OpenLiveView?.Invoke();
        BtnFullscreen.Click += (s, e) => OpenFullscreen?.Invoke();
        BtnPlayback.Click += (s, e) => OpenPlayback?.Invoke();
    }
}
