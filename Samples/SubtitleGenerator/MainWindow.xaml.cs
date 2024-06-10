using Microsoft.ML.OnnxRuntime.Tensors;
using Microsoft.ML.OnnxRuntime;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage.Pickers;
using SubtitleGenerator.Libs.VoiceActivity;
using WinUIEx;
using System.Diagnostics;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SubtitleGenerator
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : WindowEx
    {
        public readonly List<string> Languages = new List<string>(Utils.languageCodes.Keys);
        public readonly List<string> ModelSize = new List<string> { "tiny", "small", "medium"};

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
           
            this.CenterOnScreen();
            BringToFront();
            IsResizable = false;

            GenerateSubtitlesButton.IsEnabled = false;
            LangComboBox.SelectedIndex = 2;
            ModelComboBox.SelectedIndex = 1;
        }

        private async void GenerateSubtitles_ButtonClick(object sender, RoutedEventArgs e)
        {
            ViewModel.ControlsEnabled = false;
            LoadingBar.IsIndeterminate = true;
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
            LoadingBar.Value = (double)1 / totalChunks * 100 / 2;
            LoadingBar.IsIndeterminate = false;

            foreach (var chunk in dynamicChunks.Select((value, i) => (value, i)))
            {
                var audioSegment = Utils.ExtractAudioSegment(VideoFilePath, chunk.value.start, chunk.value.end - chunk.value.start);

                var transcription = await Task.Run(() => TranscribeAsync(audioSegment, Language, TranscribeType, modelType, (int)chunk.value.start));

                srtBatches.Add(transcription);


                LoadingBar.Value = (double)processedChunks++ / totalChunks * 100;
            }

            var srtFilePath = Utils.SaveSrtContentToTempFile(srtBatches, VideoFilePath);

            LoadingBar.Value = 100;

            OpenVideoFile(VideoFilePath, srtFilePath);
            OpenSrtFile(srtFilePath);

            LoadingBar.Visibility = Visibility.Collapsed;
            GenerateSubtitlesButton.Visibility = Visibility.Visible;
            ViewModel.ControlsEnabled = true;
        }



        private async void PickAFileButtonClick(object sender, RoutedEventArgs e)
        {
            var openPicker = new FileOpenPicker();
            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            WinRT.Interop.InitializeWithWindow.Initialize(openPicker, hWnd);

            openPicker.ViewMode = PickerViewMode.Thumbnail;
            openPicker.FileTypeFilter.Add("*");

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

            using var results = session.Run(inputs);
            var output = ProcessResults(results);
            var srtText = Utils.ConvertToSrt(output, batchSeconds);

            return srtText;
        }

        private static string ProcessResults(IDisposableReadOnlyCollection<DisposableNamedOnnxValue> results)
        {
            foreach (var result in results)
            {
                if (result.Name == "str")
                {
                    var tensor = result.AsTensor<string>();
                    return tensor.GetValue(0);
                }
            }

            return "Unable to extract transcription.";
        }

        private void OpenVideoFile(string videoFilePath, string srtFilePath)
        {
            var videoPlayerWindow = new VideoPlayer(videoFilePath, srtFilePath);
            videoPlayerWindow.Activate();
        }

        public void OpenSrtFile(string filePath)
        {
            Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true });
        }

    }
}
