using CommunityToolkit.Mvvm.ComponentModel;

namespace ContosoHome.ViewModels;

public sealed class DevicePageViewModel : ObservableObject
{
    private int _progressValue;

    public int ProgressValue
    {
        get => _progressValue;
        set => SetProperty(ref _progressValue, value);
    }

    private string? _label;

    public string? Label
    {
        get => _label;
        set => SetProperty(ref _label, value);
    }
}
