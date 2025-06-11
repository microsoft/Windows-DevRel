using ContosoHome.Helpers;
using ContosoHome.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace ContosoHome.Pages;
public sealed partial class DevicePage : Page
{
    ObservableCollection<Scene> ScenesCollection { get; set; } = new();

    public DevicePage()
    {
        this.InitializeComponent();
        LoadScenes();
    }

    private void syncButton_Click(object sender, RoutedEventArgs e)
    {
        LoadScenes();
    }

    public async void LoadScenes()
    {
        syncButton.IsEnabled = false;
        progressControl.Visibility = Visibility.Visible;
        ScenesCollection.Clear();

        progressControl.ProgressValue = 0;
        progressControl.Label = "Importing pictures...";

        await Task.Delay(2000);
        for (int i = 0; i < 5; i++)
        {
            foreach (var scene in ScenesLoader.Load(i))
            {
                ScenesCollection.Add(scene);
            }

            progressControl.ProgressValue += 1;
            await Task.Delay(1000);
        };

        syncButton.IsEnabled = true;
        progressControl.Visibility = Visibility.Collapsed;
    }

    private void progressControl_CancelButtonClicked(object sender, System.EventArgs e)
    {
        progressControl.Visibility = Visibility.Collapsed;
        // TO DO
    }
}
