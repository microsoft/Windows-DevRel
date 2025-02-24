using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SnowPal
{
    public sealed partial class MainPage : Page
    {
        public SnowpalViewModel ViewModel { get; } = new();

        public MainPage()
        {
            this.InitializeComponent();
        }
    }
}
