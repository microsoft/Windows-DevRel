using CommunityToolkit.Mvvm.ComponentModel;

namespace SubtitleGenerator
{
    public class MainViewModel : ObservableObject
    {
        private bool _controlsEnabled;
        public bool ControlsEnabled
        {
            get => _controlsEnabled;
            set => SetProperty(ref _controlsEnabled, value);
        }
    }
}