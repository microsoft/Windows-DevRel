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

using NReco.VideoConverter;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using Windows.Storage.Streams;
using Windows.AI.MachineLearning;
using Path = System.IO.Path;
using System.Reflection;
using Windows.Storage;
using System.Threading.Tasks;


// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SubtitleGenerator
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {

        public List<string> Languages = new List<string>(Utils.languageCodes.Keys);
        private string VideoFilePath { get; set; }
        public enum TaskType
        {
            Translate = 50358,
            Transcribe = 50359
        }

        public MainWindow()
        {
            InitializeComponent();
            Title = "Subtitles Generator";
            AppWindow.MoveAndResize(new Windows.Graphics.RectInt32(100, 100, 1600, 1100));
        }

        private void Combo2_Loaded(object sender, RoutedEventArgs e)
        {
            Combo2.SelectedIndex = 2;
        }

        private async void GenerateSubtitles_ButtonClick(object sender, RoutedEventArgs e)
        {
            var audioData = ExtractAudioFromVideo(VideoFilePath);
            var srtBatches = new List<string>();
            //foreach (var batch in audioData)
            foreach (var batch in audioData.Select((value, i) => (value, i)))
            {
                srtBatches.Add(await TranscribeAsync(batch.value, Combo2.SelectedValue.ToString(), Switch1.IsOn ? TaskType.Translate : TaskType.Transcribe, VideoFilePath, batch.i));
            }
            var srtFilePath = Utils.SaveSrtContentToTempFile(srtBatches, Path.GetFileNameWithoutExtension(VideoFilePath));
            OpenVideo(addSubtitles(VideoFilePath, srtFilePath));
        }

        private async void GetAudioFromVideoButtonClick(object sender, RoutedEventArgs e)
        {
            // Clear previous returned file name, if it exists, between iterations of this scenario

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
                PickAFileOutputTextBlock.Text = "File selected: " + file.Name;
            }
            else
            {
                PickAFileOutputTextBlock.Text = "Operation cancelled.";
            }

            this.VideoFilePath = file.Path;

        }

        private List<float[]> ExtractAudioFromVideo(string inPath)
        {
            try
            {
                var extension = System.IO.Path.GetExtension(inPath).Substring(1);
                var output = new MemoryStream();

                var convertSettings = new ConvertSettings
                {
                    AudioCodec = "pcm_s16le",
                    AudioSampleRate = 16000,
                    CustomOutputArgs = "-vn -ac 1",
                    // TODO: add batching
                    //MaxDuration = 30
                };

                var ffMpegConverter = new FFMpegConverter();
                ffMpegConverter.ConvertMedia(
                    inputFile: inPath,
                    inputFormat: extension,
                    outputStream: output,
                    outputFormat: "s16le",

                    convertSettings);

                var buffer = output.ToArray();
                // Calculate number of samples in 30 seconds; Sample rate * 30 (assuming 16K sample rate)
                int samplesPer30Seconds = 16000 * 10;
                // Calculate bytes per sample, assuming 16-bit depth (2 bytes per sample)
                int bytesPerSample = 2;

                // Calculate total samples in the buffer
                int totalSamples = buffer.Length / bytesPerSample;

                List<float[]> batches = new List<float[]>();
                for (int startSample = 0; startSample < totalSamples; startSample += samplesPer30Seconds)
                {
                    int endSample = Math.Min(startSample + samplesPer30Seconds, totalSamples);
                    int numSamples = endSample - startSample;
                    float[] batch = new float[numSamples];

                    for (int i = 0; i < numSamples; i++)
                    {
                        int bufferIndex = (startSample + i) * bytesPerSample;
                        short sample = (short)(buffer[bufferIndex + 1] << 8 | buffer[bufferIndex]);
                        batch[i] = sample / 32768.0f;
                    }

                    batches.Add(batch);
                }

                return batches;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error during the audio extraction: " + ex.Message);
                return new List<float[]>(0);
            }
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

        private async Task<string> TranscribeAsync(float[] pcmAudioData, string inputLanguage, TaskType taskType, string videoFileName, int batch)
        {
            var assemblyLocation = Assembly.GetExecutingAssembly().Location;
            var assemblyPath = Path.GetDirectoryName(assemblyLocation);
            string modelPath = Path.GetFullPath(Path.Combine(assemblyPath, "..\\..\\..\\..\\..\\Assets\\model.onnx"));

            var audioTensor = new DenseTensor<float>(pcmAudioData, [1, pcmAudioData.Length]);
            var timestampsEnableTensor = new DenseTensor<int>(new[] { 1 }, [1]);

            int task = (int)taskType;
            int langCode = Utils.GetLangId(inputLanguage);
            var decoderInputIds = new int[] { 50258, langCode, task };
            var langAndModeTensor = new DenseTensor<int>(decoderInputIds, [1, 3]);

            SessionOptions options = new SessionOptions();
            options.RegisterOrtExtensions();
            //options.GraphOptimizationLevel = GraphOptimizationLevel.ORT_ENABLE_ALL;
            //options.EnableMemoryPattern = false;
            //options.AppendExecutionProvider_DML(1);
            //options.LogSeverityLevel = 0;
            options.AppendExecutionProvider_CPU();

            using var session = new InferenceSession(modelPath, options);

            var inputs = new List<NamedOnnxValue> {
                NamedOnnxValue.CreateFromTensor("audio_pcm", audioTensor),
                NamedOnnxValue.CreateFromTensor("min_length", new DenseTensor<int>(new int[] { 0 }, [1])),
                NamedOnnxValue.CreateFromTensor("max_length", new DenseTensor<int>(new int[] { 448 }, [1])),
                NamedOnnxValue.CreateFromTensor("num_beams", new DenseTensor<int>(new int[] {1}, [1])),
                NamedOnnxValue.CreateFromTensor("num_return_sequences", new DenseTensor<int>(new int[] { 1 }, [1])),
                NamedOnnxValue.CreateFromTensor("length_penalty", new DenseTensor<float>(new float[] { 1.0f }, [1])),
                NamedOnnxValue.CreateFromTensor("repetition_penalty", new DenseTensor<float>(new float[] { 1.0f }, [1])),
                //NamedOnnxValue.CreateFromTensor("attention_mask", config.attention_mask)
                NamedOnnxValue.CreateFromTensor("logits_processor", timestampsEnableTensor),
                NamedOnnxValue.CreateFromTensor("decoder_input_ids", langAndModeTensor)
            };

            using var results = session.Run(inputs);
            var output = ProcessResults(results);
            //var srtPath = Utils.ConvertToSrt(output, Path.GetFileNameWithoutExtension(videoFileName), batch);
            var srtText = Utils.ConvertToSrt(output, Path.GetFileNameWithoutExtension(videoFileName), batch);

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

        private void OpenVideo(string videoFilePath)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = videoFilePath,
                UseShellExecute = true
            };

            Process.Start(startInfo);
        }

    }
}
