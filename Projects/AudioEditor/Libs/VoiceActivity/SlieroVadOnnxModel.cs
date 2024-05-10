using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace AudioEditor.Libs.VoiceActivity
{
    public class SlieroVadOnnxModel : IDisposable
    {
        private readonly InferenceSession session;
        private OrtValue h;
        private OrtValue c;
        private int lastSr = 0;
        private int lastBatchSize = 0;
        private static readonly List<int> SampleRates = new List<int> { 8000, 16000 };

        public SlieroVadOnnxModel()
        {
            var assemblyLocation = Assembly.GetExecutingAssembly().Location;
            var assemblyPath = Path.GetDirectoryName(assemblyLocation);
            string modelPath = Path.GetFullPath(Path.Combine(assemblyPath, "Resources\\Models\\silero_vad.onnx"));

            var options = new SessionOptions();
            options.InterOpNumThreads = 1;
            options.IntraOpNumThreads = 1;
            options.GraphOptimizationLevel = GraphOptimizationLevel.ORT_ENABLE_EXTENDED;
            session = new InferenceSession(modelPath, options);
            ResetStates();
        }

        public void ResetStates()
        {
            var hTensor = new DenseTensor<float>(new[] { 2, 1, 64 });
            var cTensor = new DenseTensor<float>(new[] { 2, 1, 64 });
            h = OrtValue.CreateTensorValueFromMemory<float>(OrtMemoryInfo.DefaultInstance, hTensor.Buffer, [2, 1, 64]);
            c = OrtValue.CreateTensorValueFromMemory<float>(OrtMemoryInfo.DefaultInstance, cTensor.Buffer, [2, 1, 64]);
            lastSr = 0;
            lastBatchSize = 0;
        }

        public void Close()
        {
            session.Dispose();
        }

        public class ValidationResult
        {
            public readonly float[][] X;
            public readonly int Sr;

            public ValidationResult(float[][] x, int sr)
            {
                X = x;
                Sr = sr;
            }
        }

        private ValidationResult ValidateInput(float[][] x, int sr)
        {
            if (x.Length == 1)
            {
                x = new float[][] { x[0] };
            }
            if (x.Length > 2)
            {
                throw new ArgumentException($"Incorrect audio data dimension: {x.Length}");
            }

            if (sr != 16000 && sr % 16000 == 0)
            {
                int step = sr / 16000;
                float[][] reducedX = x.Select(row => row.Where((_, i) => i % step == 0).ToArray()).ToArray();
                x = reducedX;
                sr = 16000;
            }

            if (!SampleRates.Contains(sr))
            {
                throw new ArgumentException($"Only supports sample rates {String.Join(", ", SampleRates)} (or multiples of 16000)");
            }

            if ((float)sr / x[0].Length > 31.25)
            {
                throw new ArgumentException("Input audio is too short");
            }

            return new ValidationResult(x, sr);
        }

        public float[] Call(float[][] x, int sr)
        {
            var result = ValidateInput(x, sr);
            x = result.X;
            sr = result.Sr;

            int batchSize = x.Length;
            int sampleSize = x[0].Length; // Assuming all subarrays have identical length

            if (lastBatchSize == 0 || lastSr != sr || lastBatchSize != batchSize)
            {
                ResetStates();
            }

            // Flatten the jagged array and create the tensor with the correct shape
            var flatArray = x.SelectMany(inner => inner).ToArray();
            var inputTensor = new DenseTensor<float>(flatArray, new[] { batchSize, sampleSize });

            // Convert sr to a tensor, if the model expects a scalar as a single-element tensor, ensure matching the expected dimensions
            var srTensor = new DenseTensor<Int64>(new[] { sr });

            var input = new Dictionary<string, OrtValue>
            {
                {"input", OrtValue.CreateTensorValueFromMemory(flatArray, [batchSize, sampleSize]) },
                {"sr", OrtValue.CreateTensorValueFromMemory(new Int64[] { sr }, [1]) },
                {"h", h },
                {"c", c },

            };

            var runOptions = new RunOptions();

            try
            {
                using (var results = session.Run(runOptions, input, session.OutputNames))
                {
                    var output = results[0].GetTensorDataAsSpan<float>().ToArray();
                    h = OrtValue.CreateTensorValueFromMemory(results.ElementAt(1).GetTensorDataAsSpan<float>().ToArray(), [2, 1, 64]);
                    c = OrtValue.CreateTensorValueFromMemory(results.ElementAt(2).GetTensorDataAsSpan<float>().ToArray(), [2, 1, 64]);

                    lastSr = sr;
                    lastBatchSize = batchSize;

                    return output;
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("An error occurred while calling the model", ex);
            }
        }

        public static int count = 0;

        public void Dispose()
        {
            session?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}