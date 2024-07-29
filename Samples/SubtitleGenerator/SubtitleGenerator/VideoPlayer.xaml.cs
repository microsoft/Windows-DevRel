using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Text;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SubtitleGenerator
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class VideoPlayer : Window
    {
        public VideoPlayer(string videoPath, string captionsPath)
        {
            InitializeComponent();
            ExtendsContentIntoTitleBar = true;

            SetupMediaPlayer(videoPath, captionsPath);

            Closed += VideoPlayer_Closed;
        }

        private void VideoPlayer_Closed(object sender, WindowEventArgs e)
        {
            mediaPlayer.MediaPlayer.Pause();
            mediaPlayer.Source = null;
        }

        private async void SetupMediaPlayer(string videoPath, string captionsPath)
        {
            // Create a MediaSource from the video file path
            StorageFile videoFile = await StorageFile.GetFileFromPathAsync(videoPath);
            MediaSource mediaSource = MediaSource.CreateFromStorageFile(videoFile);

            // Create a TimedMetadataTrack for the captions
            var timedMetadataTrack = new TimedMetadataTrack("Subtitles", "en", TimedMetadataKind.Caption);

            // Parse SRT file and add cues
            StorageFile captionsFile = await StorageFile.GetFileFromPathAsync(captionsPath);
            List<TimedTextCue> cues = await SrtParser.ParseSrtFileAsync(captionsFile);

            foreach (var cue in cues)
            {
                timedMetadataTrack.AddCue(cue);
            }

            // Add the TimedMetadataTrack to the MediaPlaybackItem
            var mediaPlaybackItem = new MediaPlaybackItem(mediaSource);
            // Add the TimedMetadataTrack to the MediaSource
            mediaSource.ExternalTimedMetadataTracks.Add(timedMetadataTrack);

            // Set the TimedMetadataTrack to be displayed
            mediaPlaybackItem.TimedMetadataTracks.SetPresentationMode(0, TimedMetadataTrackPresentationMode.PlatformPresented);

            // Set the MediaPlayerElement source
            mediaPlayer.Source = mediaPlaybackItem;

            // Start playback
            mediaPlayer.MediaPlayer.Play();
        }

        public static class SrtParser
        {
            public static async Task<List<TimedTextCue>> ParseSrtFileAsync(StorageFile srtFile)
            {
                List<TimedTextCue> cues = new List<TimedTextCue>();

                string srtContent = await FileIO.ReadTextAsync(srtFile);
                string[] srtBlocks = Regex.Split(srtContent, @"\r\n\r\n|\n\n|\r\r");

                foreach (string srtBlock in srtBlocks)
                {
                    string[] lines = srtBlock.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);
                    if (lines.Length < 3) continue;

                    try
                    {
                        string timeLine = lines[1];
                        string[] times = timeLine.Split(new[] { " --> " }, StringSplitOptions.None);
                        if (times.Length != 2) continue;

                        TimeSpan start = TimeSpan.Parse(times[0].Trim().Replace(",", "."));
                        TimeSpan end = TimeSpan.Parse(times[1].Trim().Replace(",", "."));

                        string text = string.Join(Environment.NewLine, lines, 2, lines.Length - 2);

                        TimedTextLine line = new TimedTextLine
                        {
                            Text = text
                        };

                        // Styling seems to be broken in the current version of WinUI
                        // https://github.com/microsoft/microsoft-ui-xaml/issues/9126

                        //TimedTextRegion region = new TimedTextRegion
                        //{
                        //    ScrollMode = TimedTextScrollMode.Rollup,
                        //    TextWrapping = TimedTextWrapping.Wrap,
                        //    DisplayAlignment = TimedTextDisplayAlignment.After,
                        //    Background = Windows.UI.Color.FromArgb(0, 0, 0, 255),
                        //};

                        TimedTextStyle style = new TimedTextStyle
                        {
                            // Customize style properties here
                            Background = Windows.UI.Color.FromArgb(0, 0, 0, 150),
                            Foreground = Windows.UI.Color.FromArgb(255, 255, 255, 255),
                            FontFamily = "Default",
                            FontSize = new TimedTextDouble { Value = 50 },
                            FontWeight = TimedTextWeight.Bold
                            //OutlineRadius = new TimedTextDouble { Value = 1 },
                            //OutlineColor = Windows.UI.Color.FromArgb(0, 0, 0, 255),
                            //OutlineThickness = new TimedTextDouble { Value = 25 },
                        
                        };

                        TimedTextCue cue = new TimedTextCue
                        {
                            StartTime = start,
                            Duration = end - start,
                            //CueRegion = region,
                            CueStyle = style
                        };

                        cue.Lines.Add(line);
                        cues.Add(cue);

                    }
                    catch
                    {
                        // Handle parse exceptions if necessary
                    }
                }

                return cues;
            }
        }
    }
}
