using System.Windows;
using Serilog;

namespace VMS_AlarmesJahu.App;

public partial class MainWindowModern : Window
{
    public MainWindowModern()
    {
        InitializeComponent();
        Log.Information("MainWindowModern carregada");
    }

    private void OnDashboardClick(object sender, RoutedEventArgs e)
    {
        ShowView(DashboardView);
        BtnDashboard.Opacity = 1.0;
        BtnDevices.Opacity = 0.7;
        BtnLive.Opacity = 0.7;
        Log.Information("Dashboard selecionado");
    }

    private void OnDevicesClick(object sender, RoutedEventArgs e)
    {
        ShowView(DevicesView);
        BtnDashboard.Opacity = 0.7;
        BtnDevices.Opacity = 1.0;
        BtnLive.Opacity = 0.7;
        Log.Information("Dispositivos selecionado");
    }

    private void OnLiveClick(object sender, RoutedEventArgs e)
    {
        ShowView(LiveViewContainer);
        BtnDashboard.Opacity = 0.7;
        BtnDevices.Opacity = 0.7;
        BtnLive.Opacity = 1.0;
        Log.Information("Visualização ao vivo selecionada");
    }

    private void ShowView(UIElement view)
    {
        DashboardView.Visibility = view == DashboardView ? Visibility.Visible : Visibility.Collapsed;
        DevicesView.Visibility = view == DevicesView ? Visibility.Visible : Visibility.Collapsed;
        LiveViewContainer.Visibility = view == LiveViewContainer ? Visibility.Visible : Visibility.Collapsed;
    }
}
