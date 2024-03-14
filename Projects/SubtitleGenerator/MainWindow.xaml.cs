using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Pickers;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SubtitleGenerator
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {

        public List<string> Languages = new List<string>()
        {
            "English",
            "Mandarin",
            "Yoruba",
            "German"
        };

        private const string FFMPEG_PATH = "ffmpeg.exe";

        public MainWindow()
        {
            this.InitializeComponent();
        }

        private void Combo2_Loaded(object sender, RoutedEventArgs e)
        {
            Combo2.SelectedIndex = 2;
        }

        private async void GetAudioFromVideoButtonClick(object sender, RoutedEventArgs e)
        {
            // Clear previous returned file name, if it exists, between iterations of this scenario
            PickAFileOutputTextBlock.Text = "";

            // Create a file picker
            var openPicker = new Windows.Storage.Pickers.FileOpenPicker();

            // See the sample code below for how to make the window accessible from the App class.
            var window = this;

            // Retrieve the window handle (HWND) of the current WinUI 3 window.
            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window);

            // Initialize the file picker with the window handle (HWND).
            WinRT.Interop.InitializeWithWindow.Initialize(openPicker, hWnd);

            // Set options for your file picker
            openPicker.ViewMode = PickerViewMode.Thumbnail;
            openPicker.FileTypeFilter.Add("*");

            // Open the picker for the user to pick a file
            var file = await openPicker.PickSingleFileAsync();
            if (file != null)
            {
                PickAFileOutputTextBlock.Text = "Picked file: " + file.Name;
            }
            else
            {
                PickAFileOutputTextBlock.Text = "Operation cancelled.";
            }

            ExtractAudioFromVideo(file.Path, file.Path + "Audio" + ".mp3");
        }

        private async void ExtractAudioFromVideo(string inPath, string outPath)
        {
            ProcessStartInfo ffmpegProcessInfo = new ProcessStartInfo
            {
                FileName = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", FFMPEG_PATH),
                Arguments = $"-i \"{inPath}\" -vn -acodec libmp3lame \"{outPath}\"",
                UseShellExecute = false,
                RedirectStandardError = true
            };

            using (Process ffmpegProcess = Process.Start(ffmpegProcessInfo))
            {
                string ffmpegOutput = ffmpegProcess.StandardError.ReadToEnd();
                Console.WriteLine(ffmpegOutput);

                ffmpegProcess.WaitForExit();
                PickAFileOutputTextBlock.Text = "Generated Audio File at: " + outPath;

            }

            Console.WriteLine("Audio extraction completed.");
        }

        private async void GenerateSubtitles(string audioFilePath, string outputSubtitlePath, string language)
        {
            // @Amrutha @Gleb Blank function for adding subtitle model
        }

    }
}
