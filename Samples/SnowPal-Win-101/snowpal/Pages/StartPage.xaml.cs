using System;
using System.Collections.Generic;
using System.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using SnowPal;


// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SnowPal
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class StartPage : Page
    {
        public StartPage()
        {
            this.InitializeComponent();
        }

        private void NewGame_Click(object sender, RoutedEventArgs e)
        {
            // Navigate to the MainPage
            Frame.Navigate(typeof(MainPage));
        }

        private void Leaderboard_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Navigate to the Leaderboard page
        }

        private void Options_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Navigate to the Options page
        }

        private void Credits_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Navigate to the Credits page
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Exit();
        }
    }
}
