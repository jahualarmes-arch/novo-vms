using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace VMS_AlarmesJahu.App.ViewModels;

public abstract partial class ViewModelBase : ObservableObject
{
    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string? _busyMessage;

    protected void SetBusy(bool busy, string? message = null)
    {
        IsBusy = busy;
        BusyMessage = message;
    }
}
