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
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.UI.Windowing;
using Microsoft.Windows.ApplicationModel.DynamicDependency;

using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.System.Memory;
using Windows.Win32.Storage.Packaging.Appx;
using Windows.Win32.Security;
using Microsoft.Win32.SafeHandles;
using System.ComponentModel;



namespace SubtitleGenerator
{
    public sealed partial class MainWindow : Window
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
            SetWindowSizeAndPosition();

            GenerateSubtitlesButton.IsEnabled = false;
        }

        private void SetWindowSizeAndPosition()
        {
            var windowHandle = WinRT.Interop.WindowNative.GetWindowHandle(this);
            var dpi = GetDpiForWindow(windowHandle);
            var scalingFactor = dpi / 96.0;

            var desiredWidth = (RootContainer.DesiredSize.Width + 16) * scalingFactor;
            var desiredHeight = (RootContainer.DesiredSize.Height + 10) * scalingFactor;

            var desiredWidthInt = (int)Math.Round(desiredWidth);
            var desiredHeightInt = (int)Math.Round(desiredHeight);

            // Get the current window position and size
            var currentBounds = AppWindow.Position;
            var currentSize = AppWindow.Size;

            // Calculate the center position of the current window
            var centerX = currentBounds.X + (currentSize.Width / 2);
            var centerY = currentBounds.Y + (currentSize.Height / 2);

            // Calculate the new position to keep the window centered
            var newPositionX = centerX - (desiredWidthInt / 2);
            var newPositionY = centerY - (desiredHeightInt / 2);

            //AppWindow.MoveAndResize(new RectInt32(newPositionX, newPositionY, desiredWidthInt, desiredHeightInt));

            (AppWindow.Presenter as OverlappedPresenter).IsResizable = false;
        }

        [DllImport("user32.dll")]
        private static extern uint GetDpiForWindow(IntPtr hWnd);

        private async void LoadOptionalPackagesContent()
        {
            //const string packageFamilyName = "khmyznikov.25605DF47F6B5_ggh13nganeqyr";
            //var minVersion = new Windows.ApplicationModel.PackageVersion(1, 0, 0, 0);

            //Bootstrap.Initialize(0x00010006);

            //var depend =
            //    Microsoft.Windows.ApplicationModel.DynamicDependency.PackageDependency.Create(packageFamilyName, minVersion);
            //var context = depend.Add();

            //// WinAppSDK doesn't support adding dynamic dependencies when the process is packaged.
            //// So, use the Win 11 APIs instead. Win32 interop provided via CsWin32
            //PACKAGE_VERSION version = new PACKAGE_VERSION();
            //version.Anonymous.Anonymous.Major = minVersion.Major;
            //version.Anonymous.Anonymous.Minor = minVersion.Minor;
            //version.Anonymous.Anonymous.Revision = minVersion.Revision;
            //version.Anonymous.Anonymous.Build = minVersion.Build;

            //// -6 = pseudo handle from GetCurrentThreadEffectiveToken
            //HANDLE currentThreadToken = new(new IntPtr(-6));
            //unsafe
            //{
            //    uint size = 0;
            //    PInvoke.GetTokenInformation(currentThreadToken, TOKEN_INFORMATION_CLASS.TokenUser, null, 0, &size);
            //    if (size == 0)
            //    {
            //        throw new Win32Exception(Marshal.GetLastWin32Error());
            //    }

            //    byte[] bytes = new byte[size];
            //    string dependId;
            //    int hr;
            //    fixed (byte* pBytes = &bytes[0])
            //    {
            //        PInvoke.GetTokenInformation(currentThreadToken, TOKEN_INFORMATION_CLASS.TokenUser, pBytes, size, &size);
            //        TOKEN_USER* pToken = (TOKEN_USER*)pBytes;
            //        hr =
            //            PInvoke.TryCreatePackageDependency(
            //                new SafeFileHandle(IntPtr.Zero, true), //pToken->User.Sid,
            //                packageFamilyName,
            //                version,
            //                Windows.Win32.Storage.Packaging.Appx.PackageDependencyProcessorArchitectures.PackageDependencyProcessorArchitectures_X64,
            //                Windows.Win32.Storage.Packaging.Appx.PackageDependencyLifetimeKind.PackageDependencyLifetimeKind_Process,
            //                null,
            //                Windows.Win32.Storage.Packaging.Appx.CreatePackageDependencyOptions.CreatePackageDependencyOptions_None,
            //                out var rawDependId);
            //        using HeapFreeSafeHandle dependHandle = new(rawDependId.Value);

            //        if (hr != 0)
            //        {
            //            Marshal.ThrowExceptionForHR(hr);
            //        }

            //        dependId = rawDependId.ToString();
            //    }

            //    PWSTR fullName;
            //    hr = PInvoke.AddPackageDependency(
            //        dependId,
            //        0,
            //        Windows.Win32.Storage.Packaging.Appx.AddPackageDependencyOptions.AddPackageDependencyOptions_None,
            //        out var pkgContext,
            //        &fullName);
            //    using HeapFreeSafeHandle nameHandle = new(fullName.Value);
            //    if (hr != 0)
            //    {
            //        Marshal.ThrowExceptionForHR(hr);
            //    }
            //}

            //RuntimeInformation.ProcessArchitecture.ToString();
            ////PackageVersion minVersion = new PackageVersion(1, 0, 0, 0); // Example version, adjust accordingly
            
            const string packageFamilyName = "khmyznikov.25605DF47F6B5_ggh13nganeqyr";
            var pkgVersion = 0x0001000000000000;
            string lifetimeArtifact = null; // As lifetimeKind is Process, this is null
            int options = (int)CreatePackageDependencyOptions.None;

            // Step 1: Create a package dependency
            Console.WriteLine("Creating package dependency...");
            int hr_create = DynDep.TryCreate(
                packageFamilyName, pkgVersion, (int)PackageDependencyProcessorArchitectures.X64, (int)PackageDependencyLifetimeKind.Process,
                lifetimeArtifact, options, out string packageDependencyId);

            if (hr_create < 0)
            {
                Console.WriteLine($"Failed to create package dependency. HRESULT: {hr_create}");
                return;
            }
            //Console.WriteLine($"Created package dependency with ID: {packageDependencyId}");

            //// Step 2: Add package dependency at runtime
            //Console.WriteLine("Adding package dependency at runtime...");
            //hr_create = PackageDependency.Add(packageDependencyId, 0, 0, out IntPtr packageDependencyContext, out string packageFullName);

            //if (hr_create < 0)
            //{
            //    Console.WriteLine($"Failed to add package dependency at runtime. HRESULT: {hr_create}");
            //    return;
            //}
            //Console.WriteLine($"Added package dependency with full name: {packageFullName}");

            //// Simulate some operations using the MSIX package
            //Console.WriteLine("Package dependency operations can be used here...");

            //// Step 3: Remove package dependency runtime reference
            //Console.WriteLine("Removing package dependency runtime reference...");
            //int hr_remove = PackageDependency.Remove(packageDependencyContext);

            //if (hr_remove < 0)
            //{
            //    Console.WriteLine($"Failed to remove package dependency runtime reference. HRESULT: {hr_remove}");
            //}
            //else
            //{
            //    Console.WriteLine("Successfully removed package dependency runtime reference.");
            //}

            //// Step 4: Delete the install-time package dependency
            //Console.WriteLine("Deleting the package dependency...");
            //int hr_delete = PackageDependency.Delete(packageDependencyId);

            //if (hr_delete < 0)
            //{
            //    Console.WriteLine($"Failed to delete the package dependency. HRESULT: {hr_delete}");
            //}
            //else
            //{
            //    Console.WriteLine("Successfully deleted the package dependency.");
            //}

            //var manager = new DynamicDependencyManager();

            //var package = manager.CreateInstallTimeReference(packageFamilyName, new Windows.ApplicationModel.PackageVersion(1, 0, 0, 0));
            //var context = package.Add();

            //PackageManager packageManager = new PackageManager();
            //var _package = packageManager.FindPackageForUser(string.Empty, context.PackageFullName);
            //_package.InstalledPath.ToString();

            //package.Id.ToString();
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

            LoadOptionalPackagesContent();
            return;

            //var openPicker = new FileOpenPicker();
            //var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            //WinRT.Interop.InitializeWithWindow.Initialize(openPicker, hWnd);

            //openPicker.ViewMode = PickerViewMode.Thumbnail;
            //openPicker.FileTypeFilter.Add("*");

            //var file = await openPicker.PickSingleFileAsync();
            //if (file != null)
            //{
            //    PickAFileButton.Content = "Selected: " + (file.Name.Length > 28 ? file.Name.Substring(0, 28) + "..." : file.Name);
            //    GenerateSubtitlesButton.IsEnabled = true;
            //}
            //else
            //{
            //    PickAFileButton.Content = "Select File";
            //    GenerateSubtitlesButton.IsEnabled = false;
            //    return;
            //}

            //VideoFilePath = file.Path;
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
