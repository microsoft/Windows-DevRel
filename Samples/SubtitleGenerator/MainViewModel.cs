using CommunityToolkit.Mvvm.ComponentModel;

namespace SubtitleGenerator;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private bool _controlsEnabled;
}