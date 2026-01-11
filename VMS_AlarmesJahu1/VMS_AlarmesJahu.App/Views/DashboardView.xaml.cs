using System.Windows.Controls;
using VMS_AlarmesJahu.App.ViewModels;

namespace VMS_AlarmesJahu.App.Views;

public partial class DashboardView : UserControl
{
    public DashboardView()
    {
        InitializeComponent();
    }

    public void SetViewModel(DashboardViewModel vm)
    {
        DataContext = vm;
    }
}
