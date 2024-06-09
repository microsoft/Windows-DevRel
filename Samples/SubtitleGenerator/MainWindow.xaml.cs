using Microsoft.ML.OnnxRuntime.Tensors;
using Microsoft.ML.OnnxRuntime;
using Microsoft.UI.Xaml;
using NReco.VideoConverter;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.Storage;
using SubtitleGenerator.Libs.VoiceActivity;
using WinUIEx;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SubtitleGenerator
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : WindowEx
    {
        public List<string> Languages = new List<string>(Utils.languageCodes.Keys);
        public List<string> ModelSize = new List<string> { "tiny", "small", "medium"};

        public MainViewModel ViewModel { get; } = new MainViewModel();

        private string VideoFilePath { get; set; }
        public enum TaskType
        {
            Translate = 50358,
            Transcribe = 50359
        }
        public MainWindow()
        {
            ExtendsContentIntoTitleBar = true;
            ViewModel.ControlsEnabled = true;
            InitializeComponent();
        }
        private void RootContainer_Loaded(object sender, RoutedEventArgs e)
        {
            this.SetWindowSize(RootContainer.DesiredSize.Width + 16, RootContainer.DesiredSize.Height + 10);
            this.IsResizable = false;
            this.CenterOnScreen();
            BringToFront();
        }

        private void LangComboBox_Loaded(object sender, RoutedEventArgs e)
        {
            LangComboBox.SelectedIndex = 2;
        }
        private void ModelComboBox_Loaded(object sender, RoutedEventArgs e)
        {
            ModelComboBox.SelectedIndex = 1;
        }

        private async void GenerateSubtitles_ButtonClick(object sender, RoutedEventArgs e)
        {
            ViewModel.ControlsEnabled = false;
            LoadingBar.Visibility = Visibility.Visible;
            GenerateSubtitlesButton.Visibility = Visibility.Collapsed;
            LoadingBar.Value = 0;

            var audioBytes = Utils.LoadAudioBytes(VideoFilePath);
            var srtBatches = new List<string>();
            var Language = LangComboBox.SelectedValue.ToString();
            var TranscribeType = TranslateSwitch.IsOn ? TaskType.Translate : TaskType.Transcribe;
            var modelType = ModelComboBox.SelectedValue.ToString();

            var dynamicChunks = await Task.Run(() => AudioChunking.SmartChunking(audioBytes));

            
            int totalChunks = dynamicChunks.Count;
            int processedChunks = 1;

            foreach (var chunk in dynamicChunks.Select((value, i) => (value, i)))
            {
                var audioSegment = Utils.ExtractAudioSegment(VideoFilePath, chunk.value.start, chunk.value.end - chunk.value.start);

                var transcription = await Task.Run(() => TranscribeAsync(audioSegment, Language, TranscribeType, modelType, (int)chunk.value.start));

                srtBatches.Add(transcription);


                LoadingBar.Value = (double)processedChunks++ / totalChunks * 100;
                LoadingBar.IsIndeterminate = false;
            }

            var srtFilePath = Utils.SaveSrtContentToTempFile(srtBatches, Path.GetFileNameWithoutExtension(VideoFilePath));

            LoadingBar.Value = 100;

            OpenVideo(VideoFilePath, srtFilePath);

            LoadingBar.Visibility = Visibility.Collapsed;
            GenerateSubtitlesButton.Visibility = Visibility.Visible;
            ViewModel.ControlsEnabled = true;
        }



        private async void PickAFileButtonClick(object sender, RoutedEventArgs e)
        {
            // Clear previous returned file name, if it exists, between iterations of this scenario

            // Create a file picker
            var openPicker = new FileOpenPicker();

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

                PickAFileButton.Content = "Selected: " + (file.Name.Length > 28 ? file.Name.Substring(0, 28) + "..." : file.Name);
                GenerateSubtitlesButton.IsEnabled = true;
            }
            else
            {
                PickAFileButton.Content = "Select File";
                GenerateSubtitlesButton.IsEnabled = false;
                return;
            }

            VideoFilePath = file.Path;
        }

        public async Task<byte[]> ConvertStorageFileToByteArray(StorageFile storageFile)
        {
            if (storageFile == null)
                throw new ArgumentNullException(nameof(storageFile));

            // Open the file for reading
            using (IRandomAccessStreamWithContentType stream = await storageFile.OpenReadAsync())
            {
                // Create a buffer large enough to hold the file's contents
                var buffer = new Windows.Storage.Streams.Buffer((uint)stream.Size);

                // Read the file into the buffer
                await stream.ReadAsync(buffer, buffer.Capacity, InputStreamOptions.None);

                // Convert the buffer to a byte array and return it
                return buffer.ToArray();
            }
        }


        private string TranscribeAsync(float[] pcmAudioData, string inputLanguage, TaskType taskType, string modelType, int batchSeconds)
        {
            string exePath = AppDomain.CurrentDomain.BaseDirectory;
            string modelPath = Path.GetFullPath(Path.Combine(exePath, $"Assets\\Models\\Whisper\\whisper_{modelType}_int8_cpu_ort_1.18.0.onnx"));

            var audioTensor = new DenseTensor<float>(pcmAudioData, [1, pcmAudioData.Length]);
            var timestampsEnableTensor = new DenseTensor<int>(new[] { 1 }, [1]);

            int task = (int)taskType;
            int langCode = Utils.GetLangId(inputLanguage);
            var decoderInputIds = new int[] { 50258, langCode, task };
            var langAndModeTensor = new DenseTensor<int>(decoderInputIds, [1, 3]);

            SessionOptions options = new SessionOptions();
            options.RegisterOrtExtensions();
            options.GraphOptimizationLevel = GraphOptimizationLevel.ORT_ENABLE_ALL;
            options.EnableMemoryPattern = false;

            // Need to wait for DML 1.15 @sheil.kumar and integrating the changes back to ORT, with 
            // updating the beam search logic to correctly hand non-cpu/non-cuda eps
            //options.AppendExecutionProvider_DML(1);
            
            options.AppendExecutionProvider_CPU();
            //options.LogSeverityLevel = 0;

            using var session = new InferenceSession(modelPath, options);

            var inputs = new List<NamedOnnxValue> {
                NamedOnnxValue.CreateFromTensor("audio_pcm", audioTensor),
                NamedOnnxValue.CreateFromTensor("min_length", new DenseTensor<int>(new int[] { 0 }, [1])),
                NamedOnnxValue.CreateFromTensor("max_length", new DenseTensor<int>(new int[] { 448 }, [1])),
                NamedOnnxValue.CreateFromTensor("num_beams", new DenseTensor<int>(new int[] {2}, [1])),
                NamedOnnxValue.CreateFromTensor("num_return_sequences", new DenseTensor<int>(new int[] { 1 }, [1])),
                NamedOnnxValue.CreateFromTensor("length_penalty", new DenseTensor<float>(new float[] { 1.0f }, [1])),
                NamedOnnxValue.CreateFromTensor("repetition_penalty", new DenseTensor<float>(new float[] { 1.0f }, [1])),
                //NamedOnnxValue.CreateFromTensor("attention_mask", config.attention_mask)
                NamedOnnxValue.CreateFromTensor("logits_processor", timestampsEnableTensor),
                NamedOnnxValue.CreateFromTensor("decoder_input_ids", langAndModeTensor)
            };

            // for multithread need to try AsyncRun
            using var results = session.Run(inputs);
            var output = ProcessResults(results);
            //var srtPath = Utils.ConvertToSrt(output, Path.GetFileNameWithoutExtension(videoFileName), batch);
            var srtText = Utils.ConvertToSrt(output, batchSeconds);

            //PickAFileOutputTextBlock.Text = "Generated SRT File at: " + srtPath;

            return srtText;
        }

        private static string ProcessResults(IDisposableReadOnlyCollection<DisposableNamedOnnxValue> results)
        {
            foreach (var result in results)
            {
                if (result.Name == "str") // Replace "output_name" with the actual output name of your model
                {
                    var tensor = result.AsTensor<string>();
                    return tensor.GetValue(0); // Simplified; actual extraction may differ
                }
            }

            return "Unable to extract transcription.";
        }
        private string FixPath(string path)
        {
            return path.Replace("\\", "\\\\\\\\").Insert(1, "\\\\");
        }

        private string addSubtitles(string videoPath, string srtPath)
        {
            string documentsFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string outputFilePath = Path.Combine(documentsFolderPath, Path.GetFileNameWithoutExtension(videoPath) + "Subtitled" + Path.GetExtension(videoPath));

            if (File.Exists(outputFilePath))
            {
                File.Delete(outputFilePath);
            }

            var ffMpegConverter = new FFMpegConverter();
            string newSrtPath = FixPath(srtPath);
            ffMpegConverter.Invoke($"-i \"{videoPath}\" -vf subtitles=\"{newSrtPath}\"  \"{outputFilePath}\"");

            return outputFilePath;
        }

        private void OpenVideo(string videoFilePath, string srtFilePath)
        {
            //ProcessStartInfo startInfo = new ProcessStartInfo
            //{
            //    FileName = videoFilePath,
            //    UseShellExecute = true,
            //    Arguments = srtFilePath
            //};

            var videoPlayerWindow = new VideoPlayer(videoFilePath, srtFilePath);
            videoPlayerWindow.Activate();

            //string vlcPath = @"C:\Program Files\VideoLAN\VLC\vlc.exe";
            //ProcessStartInfo startInfo = new ProcessStartInfo
            //{
            //    FileName = vlcPath,
            //    UseShellExecute = false,
            //    Arguments = $"\"{videoFilePath}\" --sub-file=\"{srtFilePath}\" --no-osd"
            //};

            //Process.Start(startInfo);
        }

    }
}
