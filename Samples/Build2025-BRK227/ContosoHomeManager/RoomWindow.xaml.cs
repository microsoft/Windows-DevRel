using ContosoHomeManager.Models;
using System.Windows;
using System.Windows.Media.Imaging;

namespace ContosoHomeManager
{
    /// <summary>
    /// Interaction logic for RoomWindow.xaml
    /// </summary>
    public partial class RoomWindow : Window
    {
        private CancellationTokenSource? _cts;

        private Room selectedRoom;
        public RoomWindow(Room room)
        {
            InitializeComponent();

            selectedRoom = room;
            roomNameText.Text = selectedRoom.Name;
            roomImage.ImageSource = new BitmapImage(new Uri(selectedRoom.Image!, UriKind.RelativeOrAbsolute));

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            descriptionText.Text = "Someone playing a video game on the console, a bowl was refilled with popcorn, and a cat settled in for a nap atop the shelf.";
        }
    }
}
