using Microsoft.ML.OnnxRuntime.Tensors;
using Microsoft.ML.OnnxRuntime;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using System.IO;
using Libs.VoiceActivity;
using AudioEditor;

namespace Libs.VoiceRecognition
{
    public class Whisper
    {
        private InferenceSession _inferenceSession;
        private void InitModel()
        {
            if (_inferenceSession != null)
            {
                return;
            }
            var assemblyLocation = Assembly.GetExecutingAssembly().Location;
            var assemblyPath = Path.GetDirectoryName(assemblyLocation);
            string modelPath = Path.GetFullPath(Path.Combine(assemblyPath, "Resources\\Models\\whisper_tiny.onnx"));
            SessionOptions options = new SessionOptions();
            options.RegisterOrtExtensions();
            options.AppendExecutionProvider_CPU();
            options.LogSeverityLevel = OrtLoggingLevel.ORT_LOGGING_LEVEL_INFO; 
            _inferenceSession = new InferenceSession(modelPath, options);
        }

        public static Dictionary<string, string> languageCodes = new()
            {
                {"English", "en"},
                {"Serbian", "sr"},
                {"Hindi", "hi"},
                {"Spanish", "es"},
                {"Russian", "ru"},
                {"Korean", "ko"},
                {"French", "fr"},
                {"Japanese", "ja"},
                {"Portuguese", "pt"},
                {"Turkish", "tr"},
                {"Polish", "pl"},
                {"Catalan", "ca"},
                {"Dutch", "nl"},
                {"Arabic", "ar"},
                {"Swedish", "sv"},
                {"Italian", "it"},
                {"Indonesian", "id"},
                {"Macedonian", "mk" },
                {"Mandarin", "zh" }
        };
        public enum TaskType
        {
            Translate = 50358,
            Transcribe = 50359
        }
        public static int GetLangId(string languageString)
        {
            int langId = 50259;
            Dictionary<string, int> langToId = new Dictionary<string, int>
        {
            {"af", 50327},
            {"am", 50334},
            {"ar", 50272},
            {"as", 50350},
            {"az", 50304},
            {"ba", 50355},
            {"be", 50330},
            {"bg", 50292},
            {"bn", 50302},
            {"bo", 50347},
            {"br", 50309},
            {"bs", 50315},
            {"ca", 50270},
            {"cs", 50283},
            {"cy", 50297},
            {"da", 50285},
            {"de", 50261},
            {"el", 50281},
            {"en", 50259},
            {"es", 50262},
            {"et", 50307},
            {"eu", 50310},
            {"fa", 50300},
            {"fi", 50277},
            {"fo", 50338},
            {"fr", 50265},
            {"gl", 50319},
            {"gu", 50333},
            {"haw", 50352},
            {"ha", 50354},
            {"he", 50279},
            {"hi", 50276},
            {"hr", 50291},
            {"ht", 50339},
            {"hu", 50286},
            {"hy", 50312},
            {"id", 50275},
            {"is", 50311},
            {"it", 50274},
            {"ja", 50266},
            {"jw", 50356},
            {"ka", 50329},
            {"kk", 50316},
            {"km", 50323},
            {"kn", 50306},
            {"ko", 50264},
            {"la", 50294},
            {"lb", 50345},
            {"ln", 50353},
            {"lo", 50336},
            {"lt", 50293},
            {"lv", 50301},
            {"mg", 50349},
            {"mi", 50295},
            {"mk", 50308},
            {"ml", 50296},
            {"mn", 50314},
            {"mr", 50320},
            {"ms", 50282},
            {"mt", 50343},
            {"my", 50346},
            {"ne", 50313},
            {"nl", 50271},
            {"nn", 50342},
            {"no", 50288},
            {"oc", 50328},
            {"pa", 50321},
            {"pl", 50269},
            {"ps", 50340},
            {"pt", 50267},
            {"ro", 50284},
            {"ru", 50263},
            {"sa", 50344},
            {"sd", 50332},
            {"si", 50322},
            {"sk", 50298},
            {"sl", 50305},
            {"sn", 50324},
            {"so", 50326},
            {"sq", 50317},
            {"sr", 50303},
            {"su", 50357},
            {"sv", 50273},
            {"sw", 50318},
            {"ta", 50287},
            {"te", 50299},
            {"tg", 50331},
            {"th", 50289},
            {"tk", 50341},
            {"tl", 50325},
            {"tr", 50268},
            {"tt", 50335},
            {"ug", 50348},
            {"uk", 50260},
            {"ur", 50337},
            {"uz", 50351},
            {"vi", 50278},
            {"xh", 50322},
            {"yi", 50305},
            {"yo", 50324},
            {"zh", 50258},
            {"zu", 50321}
        };

            if (languageCodes.TryGetValue(languageString, out string langCode))
            {
                langId = langToId[langCode];
            }

            return langId;
        }

        //public async Task<List<TranscribedChunk>> TranscribeAsync(float[] pcmAudioData, string inputLanguage, TaskType taskType, double offsetSeconds)
        //{

        //    InitModel();
        //    var audioTensor = new DenseTensor<float>(pcmAudioData, [1, pcmAudioData.Length]);
        //    var timestampsEnableTensor = new DenseTensor<int>(new[] { 1 }, [1]);

        //    int task = (int)taskType;
        //    int langCode = GetLangId(inputLanguage);
        //    var decoderInputIds = new int[] { 50258, langCode, task };
        //    var langAndModeTensor = new DenseTensor<int>(decoderInputIds, [1, 3]);



        //    var inputs = new List<NamedOnnxValue> {
        //        NamedOnnxValue.CreateFromTensor("audio_pcm", audioTensor),
        //        NamedOnnxValue.CreateFromTensor("min_length", new DenseTensor<int>(new int[] { 0 }, [1])),
        //        NamedOnnxValue.CreateFromTensor("max_length", new DenseTensor<int>(new int[] { 448 }, [1])),
        //        NamedOnnxValue.CreateFromTensor("num_beams", new DenseTensor<int>(new int[] {2}, [1])),
        //        NamedOnnxValue.CreateFromTensor("num_return_sequences", new DenseTensor<int>(new int[] { 1 }, [1])),
        //        NamedOnnxValue.CreateFromTensor("length_penalty", new DenseTensor<float>(new float[] { 1.0f }, [1])),
        //        NamedOnnxValue.CreateFromTensor("repetition_penalty", new DenseTensor<float>(new float[] { 1.0f }, [1])),
        //        //NamedOnnxValue.CreateFromTensor("attention_mask", config.attention_mask)
        //        NamedOnnxValue.CreateFromTensor("logits_processor", timestampsEnableTensor),
        //        NamedOnnxValue.CreateFromTensor("decoder_input_ids", langAndModeTensor)
        //    };

        //    // for multithread need to try AsyncRun
        //    using var results = _inferenceSession.Run(inputs);
        //    var output = ProcessResults(results, offsetSeconds);

        //    return output;
        //}

        public async Task<List<TranscribedChunk>> TranscribeAsync(byte[] pcmAudioData, string inputLanguage, TaskType taskType, double offsetSeconds)
        {

            InitModel();
            var audioTensor = new DenseTensor<byte>(pcmAudioData, [1, pcmAudioData.Length]);
            var timestampsEnableTensor = new DenseTensor<int>(new[] { 1 }, [1]);

            int task = (int)taskType;
            int langCode = GetLangId(inputLanguage);
            var decoderInputIds = new int[] { 50258, langCode, task };
            var langAndModeTensor = new DenseTensor<int>(decoderInputIds, [1, 3]);



            var inputs = new List<NamedOnnxValue> {
                NamedOnnxValue.CreateFromTensor("audio_stream", audioTensor),
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
            using var results = _inferenceSession.Run(inputs);
            var output = ProcessResults(results, offsetSeconds);

            return output;
        }

        private static List<TranscribedChunk> ProcessResults(IDisposableReadOnlyCollection<DisposableNamedOnnxValue> results, double offsetSeconds)
        {
            List<TranscribedChunk> list = new();
            foreach (var result in results)
            {
                if (result.Name == "str") // Replace "output_name" with the actual output name of your model
                {
                    var tensor = result.AsTensor<string>();
                    var transcription = tensor.GetValue(0); // Simplified; actual extraction may differ

                    list.AddRange(Utils.ProcessTranscription(transcription, offsetSeconds));

                }
            }
            return list;
        }
        private static string FixPath(string path)
        {
            return path.Replace("\\", "\\\\\\\\").Insert(1, "\\\\");
        }
    }
}
