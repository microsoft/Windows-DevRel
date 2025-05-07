using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PoemGenerator.Model;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace PoemGenerator.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainViewModel ViewModel { get; }
        public AIModelService AIModelService { get; }

        public MainPage()
        {
            InitializeComponent();
            AIModelService = new AIModelService();
            ViewModel = new MainViewModel(AIModelService);
        }

        private void MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.SelectedPoemType = ((MenuFlyoutItem)sender).Text;
            PoemTypeDropdownText.Text = ((MenuFlyoutItem)sender).Text;
        }
    }
}
