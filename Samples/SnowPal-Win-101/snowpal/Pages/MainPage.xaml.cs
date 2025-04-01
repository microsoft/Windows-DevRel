using Microsoft.UI.Xaml.Controls;



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
