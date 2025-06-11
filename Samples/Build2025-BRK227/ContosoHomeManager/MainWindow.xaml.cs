using ContosoHomeManager.Models;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace ContosoHomeManager
{
    public partial class MainWindow : Window
    {
        ObservableCollection<Room> RoomsCollection;

        public MainWindow()
        {
            InitializeComponent();

            RoomsCollection =
            [
                new Room() { Name = "Living Room", Image ="pack://application:,,,/Assets/LivingRoom.png", Description = "1 event detected" },
                new Room() { Name = "Hallway", Image = "pack://application:,,,/Assets/Hall.png", Description = "" },
                new Room() { Name = "Garden", Image = "pack://application:,,,/Assets/Garden.png", Description = "" },
                new Room() { Name = "Kitchen", Image = "pack://application:,,,/Assets/Kitchen.png", Description = "" },
                new Room() { Name = "Master bedroom", Image = "pack://application:,,,/Assets/MasterBedroom.png", Description = "" },
                new Room() { Name = "Office", Image = "pack://application:,,,/Assets/HomeOffice.png", Description = "2 events detected" },
            ];
            RoomsListView.ItemsSource = RoomsCollection;

        }

        private void listView_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is ListView view && view.SelectedItem is Room room)
            {
                RoomWindow roomWindow = new RoomWindow(room);
                roomWindow.Show();
            }            
        }
    }
}