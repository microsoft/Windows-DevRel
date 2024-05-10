using Libs.SemanticSearch.MiniLM;
using Libs.SemanticSearch;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;

using System.Runtime.Serialization.Json;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;

using System.Runtime.InteropServices.WindowsRuntime;
using Libs.VoiceActivity;
using Libs.VoiceRecognition;
using static Libs.VoiceRecognition.Whisper;
using System.Reflection;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Media.Imaging;
using System.Drawing;
using Microsoft.UI.Xaml.Media;
using System.Drawing.Imaging;
using Microsoft.UI;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace AudioEditor
{
    public sealed partial class MainWindow : Window, INotifyPropertyChanged
    {
        private string timestampDisplayString = "00:00:00 / 00:00:00";
        private Canvas waveformDisplayCanvas;
        private TextBlock timeDisplayTextBlock;
        private ProgressRing waveformProgressRing;
        private DispatcherQueue dispatcher;
        private WaveformRenderer renderer;
        private ImageBrush canvasImageBrush;

        public ObservableCollection<AudioFile> AudioFiles { get; set; }
        public ObservableCollection<string> LoadingCards { get; } = [];

        public int numCurrentlyLoading = 0;
        public AudioPlayer Player { get; set; }
        public string TimeDisplayString 
        {
            get { return this.timestampDisplayString; }
            set
            {
                if (value != this.timestampDisplayString)
                {
                    this.timestampDisplayString = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public ImageBrush CanvasImageBrush
        {
            get { return this.canvasImageBrush; }
            set
            {
                if (value != this.canvasImageBrush)
                {
                    this.canvasImageBrush = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public MainWindow()
        {
            this.InitializeComponent();
            dispatcher = DispatcherQueue.GetForCurrentThread();
            timeDisplayTextBlock = this.TimeDisplayTextBlock;
            waveformDisplayCanvas = this.WaveformCanvas;
            waveformProgressRing = this.WaveformProgressRing;
            AudioFiles = new ObservableCollection<AudioFile>();
            Player = new AudioPlayer();
            Player.TimestampUpdated += UpdateTimeDisplayStringHandler;

            InitializeAsync();
            Title = "AI Audio Trimmer";
            this.AppWindow.SetIcon("Assets/AIAudioTrimLogo.ico");
        }


        private async void InitializeAsync()
        {
            await LoadAudioFilesAsync();
        }


        private void UpdateTimeDisplayStringHandler(object sender, TimestampUpdatedEventArgs args)
        {  
            dispatcher.TryEnqueue(() =>
            {
                UpdateTimeDisplayString(args.Time);
                UpdatePlayline(args.Time);
            });   
        }

        private void UpdateTimeDisplayString(TimeSpan span)
        {
            TimeDisplayString = Utils.FormatTimeSpan(span) + " / " + Utils.FormatTimeSpan(this.Player.CurrentAudioFile.TotalDuration);
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
                Task.Run(async () =>
                {
                    if (!Utils.DoesTranscriptionExist(selectedFile.Name))
                    {
                        await ChunkAndTranscribe(selectedFile.Path);
                    }
                });
                await SaveAudioFilesAsync(); // Save the updated list
            }
        }

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
                        if(audioFile.TotalDuration.TotalNanoseconds !> 0)
                        {
                            audioFile.SetTotalDuration();
                        }
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
                ResetPlayer();
                Player.CurrentAudioFile = selectedAudio;
                UpdateTimeDisplayString(TimeSpan.FromSeconds(0));
                ClearLines();
                DisposeRenderer();
                UpdateWaveform(selectedAudio);
            }
        }

        public static async Task ClearLocalStorageFile(string fileName)
        {
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            StorageFile file = await localFolder.GetFileAsync(fileName);
            await file.DeleteAsync();
        }

        private void RemoveBtn_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                AudioFile selectedAudioFile = btn.DataContext as AudioFile;
                AudioFiles.Remove(selectedAudioFile);
            }
        }

        private async void GenerateButton_Click(object sender, RoutedEventArgs e)
        {

            if (sender is Button btn)
            {
                AudioFile selectedAudioFile = btn.DataContext as AudioFile;
         
                if (selectedAudioFile.Keyword != null)
                {
                    LoadingCards.Add("Generating snippet from " + selectedAudioFile.FileName);
                    string outputFilename = selectedAudioFile.TrimmedClipName == "" ? Path.GetFileName(selectedAudioFile.FilePath) : selectedAudioFile.TrimmedClipName + ".mp3";
                    string outputPath = Utils.CreateOutputPath(selectedAudioFile.FilePath, outputFilename);
                    Debug.WriteLine(outputPath);
                    await RunTranscribeSearchaAndTrimTask(selectedAudioFile, outputPath);
                    if (File.Exists(outputPath))
                    {
                        AudioFiles.Add(new AudioFile(Path.GetFileNameWithoutExtension(outputPath), outputPath));
                        await SaveAudioFilesAsync();
                    }
                    LoadingCards.Remove("Generating snippet from " + selectedAudioFile.FileName);
                }  
            }
        }

        private async Task RunTranscribeSearchaAndTrimTask(AudioFile audioFile, string outputPath)
        {
            await Task.Run(async () =>
            {
                List<TranscribedChunk> chunkList = ApplySemanticSearch(await ChunkAndTranscribe(audioFile.FilePath), audioFile.Keyword, audioFile.TrimmedDuration);
                if (chunkList.Count > 0)
                {
                    TranscribedChunk chunk = chunkList[0];
                    Utils.TrimMp3(audioFile.FilePath, outputPath, TimeSpan.FromSeconds(chunk.Start), TimeSpan.FromSeconds(chunk.End));
                }
            });
        }

        private async Task<List<TranscribedChunk>> ChunkAndTranscribe(string audioPath)
        {
            var assemblyLocation = Assembly.GetExecutingAssembly().Location;
            var assemblyPath = Path.GetDirectoryName(assemblyLocation);
            string inputAudioPath = audioPath;
            string fileName = Path.GetFileNameWithoutExtension(audioPath);


            var transcribedChunks = Utils.RetrieveTranscriptionFromFile(fileName) ?? new List<TranscribedChunk>();

            if (transcribedChunks.Count != 0)
            {
                return transcribedChunks;
            }
            var audioBytes = Utils.LoadAudioBytes(inputAudioPath);
            var dynamicChunks = AudioChunking.SmartChunking(audioBytes);

            foreach (var chunk in dynamicChunks.Select((value, i) => (value, i)))
            {
                var audioSegment = Utils.ExtractAudioSegment(inputAudioPath, chunk.value.start, chunk.value.end - chunk.value.start);
                var whisperModel = new Whisper();
                var transcription = await whisperModel.TranscribeAsync(audioSegment, "en", TaskType.Transcribe, chunk.value.start);
                transcribedChunks.AddRange(transcription);
            }

            transcribedChunks = Utils.MergeTranscribedChunks(transcribedChunks);
            Utils.SaveTranscriptionToFile(transcribedChunks, fileName);
            return transcribedChunks;
        }

        private List<TranscribedChunk> ApplySemanticSearch(List<TranscribedChunk> listOfChunks, string searchQuery, int durationSeconds = 30)
        {
            var miniLM = new MiniLML6v2(new MiniLML6v2Config());

            listOfChunks = Utils.CreateDurationSizedChunkWindows(listOfChunks, durationSeconds);

            string[] corpusArray = listOfChunks.Select(x => x.Transcript).ToArray();

            string[] searchQueryArray = { searchQuery };

            // Generate embeddings for the corpus
            var corpusEmbeddings = corpusArray
                .Select(text => miniLM.GenerateEmbeddings(new string[] { text }))
                .ToArray();

            // Generate embeddings for the search query
            var searchQueryEmbeddings = miniLM.GenerateEmbeddings(searchQueryArray); // Assuming one query

            // Calculate similarities
            var similarityScores = corpusEmbeddings
                .Select(embedding => Similarity.CosineSimilarity(searchQueryEmbeddings, embedding))
                .ToArray();

            // Order by similarity in desc order and select indexes
            var sortedIndexBySimilarity = similarityScores
                .Select((score, index) => new { Index = index, Score = score, Text = corpusArray[index] })
                .OrderByDescending(x => x.Score)
                .Select(x => listOfChunks[x.Index]).ToList();

            var relevantSnips = Utils.GenerateRelevantSnipsWithDuration(sortedIndexBySimilarity, durationSeconds);


            return relevantSnips;
        }

        private void PlayPause_ButtonClick(object sender, RoutedEventArgs e)
        {
            if(Player.CurrentAudioFile != null)
            {
                TogglePlayPause();
            }
        }

        private void TogglePlayPause()
        {
            Player.Paused = !Player.Paused;

            if (!Player.Paused)
            {
                Player.Play();
                playPauseButton.Content = new FontIcon { Glyph = "\uF8AE" };
            }
            else
            {
                Player.Stop();
                playPauseButton.Content = new FontIcon { Glyph = "\uF5B0" };
            }
        }

        private void ResetPlayer()
        {
            Player.Paused = true;
            playPauseButton.Content = new FontIcon { Glyph = "\uF5B0" };
            Player.Stop();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void Restart_ButtonClick(object sender, RoutedEventArgs e)
        {
            Player.Seek(0);
        }

        private void Rewind_ButtonClick(object sender, RoutedEventArgs e)
        {
            Player.Seek(-10);
        }

        private void FastForward_ButtonClick(object sender, RoutedEventArgs e)
        {
            Player.Seek(10);
        }

        private void CreateRenderer(AudioFile audioFile)
        {
            renderer = new WaveformRenderer(audioFile, (int)waveformDisplayCanvas.ActualHeight, (int)waveformDisplayCanvas.ActualWidth);
        }

        private void DisposeRenderer()
        {
            renderer = null;
        }

        private async void UpdateWaveform(AudioFile audioFile)
        {
            if(renderer != null)
            {
                waveformProgressRing.IsActive = true;
                StorageFile file;
                StorageFolder localFolder = ApplicationData.Current.LocalFolder;
                try
                {
                    file = await localFolder.CreateFileAsync(Path.ChangeExtension(audioFile.FileName + "-waveform", ".png"), CreationCollisionOption.FailIfExists);
                    using (var stream = await file.OpenStreamForWriteAsync())
                    {
                        System.Drawing.Image image = renderer.Render();
                        image.Save(stream, ImageFormat.Png);
                    }
                } 
                catch
                {
                    file = await localFolder.GetFileAsync(Path.ChangeExtension(audioFile.FileName + "-waveform", ".png"));
                }
                

                Uri uri = new Uri(file.Path);
                BitmapImage bi = new BitmapImage(uri);
                ImageBrush imageBrush = new ImageBrush();
                imageBrush.ImageSource = bi;
                CanvasImageBrush = imageBrush;
                waveformProgressRing.IsActive = false;

            } else
            {
                CreateRenderer(audioFile);
                UpdateWaveform(audioFile);
            }
        }


        private BitmapImage ConvertImageToBitmapImage(System.Drawing.Image image)
        {
            BitmapImage bitmapImage = new BitmapImage();

            using (MemoryStream memoryStream = new MemoryStream())
            {
                // Save the Bitmap to the MemoryStream as PNG (you can change the format as needed)
                image.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);

                bitmapImage.SetSource(memoryStream.AsRandomAccessStream());
            }

            return bitmapImage;

        }

        private void AddLine(double X)
        {
            Color color = Color.Red;
            Microsoft.UI.Xaml.Shapes.Line myLine = new Microsoft.UI.Xaml.Shapes.Line();
            myLine.Stroke = new SolidColorBrush(Windows.UI.Color.FromArgb(color.A, color.R, color.G, color.B));
            myLine.X1 = X; // Starting X-coordinate
            myLine.X2 = X; // Ending X-coordinate (same as starting for a vertical line)
            myLine.Y1 = 0; // Starting Y-coordinate
            myLine.Y2 = waveformDisplayCanvas.ActualHeight; // Ending Y-coordinate
            myLine.StrokeThickness = 1; // Set the thickness to 1 pixel

            waveformDisplayCanvas.Children.Add(myLine);
        }

        private void ClearLines()
        {
            waveformDisplayCanvas.Children.Clear();
        }

        private void UpdatePlayline(TimeSpan currentTime)
        {
            double timeFraction = currentTime.TotalSeconds / Player.CurrentAudioFile.TotalDuration.TotalSeconds;
            ClearLines();
            AddLine(timeFraction * waveformDisplayCanvas.ActualWidth);
        }
    }
}
