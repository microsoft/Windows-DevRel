using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;
using Windows.Media.Core;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace AudioEditor
{
    public sealed partial class MainWindow : Window
    {
        public ObservableCollection<AudioFile> AudioFiles { get; set; } = new ObservableCollection<AudioFile>();

        public MainWindow()
        {
            this.InitializeComponent();

            InitializeAsync();
        }

        private async void InitializeAsync()
        {
            //await ClearLocalStorageFile("audioFiles.json"); // I was using this to reset the list

            await LoadAudioFilesAsync();

            AudioListBox.ItemsSource = AudioFiles;
            AudioListBox.DisplayMemberPath = "FileName";
        }

        private async void UploadButton_Click(object sender, RoutedEventArgs e)
        {
            var openPicker = new Windows.Storage.Pickers.FileOpenPicker();
            var window = this;
            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
            WinRT.Interop.InitializeWithWindow.Initialize(openPicker, hWnd);
            openPicker.FileTypeFilter.Add(".mp3");
            openPicker.ViewMode = PickerViewMode.Thumbnail;

            var selectedFile = await openPicker.PickSingleFileAsync();
            if (selectedFile != null)
            {
                AudioFiles.Add(new AudioFile(selectedFile.DisplayName, selectedFile.Path));
                await SaveAudioFilesAsync(); // Save the updated list
            }
        }

        // loads audio files from local storage
        private async Task LoadAudioFilesAsync()
        {
            try
            {
                StorageFolder localFolder = ApplicationData.Current.LocalFolder;
                StorageFile file = await localFolder.GetFileAsync("audioFiles.json");

                using (var stream = await file.OpenStreamForReadAsync())
                {
                    var serializer = new DataContractJsonSerializer(typeof(AudioFile[])); 
                    var audioFilesArray = (AudioFile[])serializer.ReadObject(stream);
                    AudioFiles.Clear(); 
                    foreach (var audioFile in audioFilesArray)
                    {
                        AudioFiles.Add(audioFile);
                    }
                }
            }
            catch (FileNotFoundException ex)
            {
                Debug.WriteLine(ex.Message);
                Debug.WriteLine(ex.StackTrace);
            }
        }

        // Saves audio file to local storage
        public async Task SaveAudioFilesAsync()
        {
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            StorageFile file = await localFolder.CreateFileAsync("audioFiles.json", CreationCollisionOption.ReplaceExisting);

            using (var stream = await file.OpenStreamForWriteAsync())
            {
                var serializer = new DataContractJsonSerializer(typeof(AudioFile[])); 
                serializer.WriteObject(stream, AudioFiles.ToArray());
            }
        }

        private void AudioListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (AudioListBox.SelectedItem != null)
            {
                var selectedAudio = (AudioFile)AudioListBox.SelectedItem;

                myMediaPlayer.Source = MediaSource.CreateFromUri(new Uri(selectedAudio.FilePath));
            }
        }

        // helper function to reset local storage
        public static async Task ClearLocalStorageFile(string fileName)
        {
            // Get the local app data folder
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;

            // Get the file to delete
            StorageFile file = await localFolder.GetFileAsync(fileName);

            // Delete the file
            await file.DeleteAsync();
        }

    }
}
