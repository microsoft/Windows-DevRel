using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.ObjectModel;
using Windows.Media.Core;
using Windows.Media.Playback;

namespace AudioEditor
{
    public sealed partial class MainWindow : Window
    {
        public ObservableCollection<AudioFile> AudioFiles { get; set; } = new ObservableCollection<AudioFile>();

        public MainWindow()
        {
            this.InitializeComponent();


            // Add audio files to the list - have to replace with files from your computer 
            AudioFiles.Add(new AudioFile("Tame Impala - The Less I Know the Better", "C:/Users/jaylynbarbee/Desktop/test_audio/impala.mp3"));
            AudioFiles.Add(new AudioFile("Carolesdaughter - Violent", "C:/Users/jaylynbarbee/Desktop/test_audio/violent.mp3"));
            AudioFiles.Add(new AudioFile("Michael Jackson - PYT", "C:/Users/jaylynbarbee/Desktop/test_audio/pyt.mp3"));
            AudioFiles.Add(new AudioFile("Bobby Caldwell - What You Won't Do for Love", "C:/Users/jaylynbarbee/Desktop/test_audio/love.mp3"));

            AudioListBox.ItemsSource = AudioFiles;
            AudioListBox.DisplayMemberPath = "FileName";
        }

        private void AudioListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (AudioListBox.SelectedItem != null)
            {
                var selectedAudio = (AudioFile)AudioListBox.SelectedItem;

                // Set the audio file as the source of MediaPlayerElement
                myMediaPlayer.Source = MediaSource.CreateFromUri(new Uri(selectedAudio.FilePath));
            }
        }
    }
}

