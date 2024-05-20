using System.Collections.Generic;
using System.Linq;
using System.IO;
using TorchSharp;
using Microsoft.ML.OnnxRuntime;
using System.Reflection;

namespace Libs.SemanticSearch.MiniLM
{
    internal class MiniLML6v2
    {
        private static readonly string[] OutputColumnNames =
        {
            "last_hidden_state"
        };

        private static readonly string[] InputColumnNames =
        {
            "input_ids", "attention_mask", "token_type_ids"
        };

        private readonly MiniLML6v2Config config;
        private readonly WordPieceTokenizer tokenizer;
        private InferenceSession _inferenceSession;

        public MiniLML6v2(MiniLML6v2Config config)
        {
            this.config = config;
            var assemblyLocation = Assembly.GetExecutingAssembly().Location;
            var assemblyPath = Path.GetDirectoryName(assemblyLocation);
            string vocabTxtPath = Path.GetFullPath(Path.Combine(assemblyPath, "Resources\\vocab.txt"));
            tokenizer = new WordPieceTokenizer(File.ReadAllLines(vocabTxtPath).ToList());
        }

        private void InitModel(string modelPath)
        {
            if (_inferenceSession == null)
            {
                var sessionOptions = new SessionOptions();
                sessionOptions.LogSeverityLevel = OrtLoggingLevel.ORT_LOGGING_LEVEL_INFO;
                sessionOptions.AppendExecutionProvider_CPU(); //hardcoded to my machine - how do I get the device id?
                _inferenceSession = new InferenceSession(modelPath, sessionOptions);
            } 
        }

        public float[] GenerateEmbeddings(IEnumerable<string> input, bool meanPooling = true, bool normalize = true)
        {
            var assemblyLocation = Assembly.GetExecutingAssembly().Location;
            var assemblyPath = Path.GetDirectoryName(assemblyLocation);
            string modelPath = Path.GetFullPath(Path.Combine(assemblyPath, "Resources\\Models\\all-MiniLM-L6-v2.onnx"));
            InitModel(modelPath);
        

            var inputTexts = input.ToList();
            var batchSize = inputTexts.Count;
            var encodedCorpus = PrepareInput(inputTexts);

            using var inputIdsOrtValue = OrtValue.CreateTensorValueFromMemory(encodedCorpus.InputIds,
                 [batchSize, encodedCorpus.InputIds.Length]);

            using var attMaskOrtValue = OrtValue.CreateTensorValueFromMemory(encodedCorpus.AttentionMask,
                  [batchSize, encodedCorpus.AttentionMask.Length]);

            using var typeIdsOrtValue = OrtValue.CreateTensorValueFromMemory(encodedCorpus.TokenTypeIds,
                  [batchSize, encodedCorpus.TokenTypeIds.Length]);

            var inputs = new Dictionary<string, OrtValue>
            {
                { "input_ids", inputIdsOrtValue },
                { "attention_mask", attMaskOrtValue },
                { "token_type_ids", typeIdsOrtValue }
            };

            var runOptions = new RunOptions();

            using var output = _inferenceSession.Run(runOptions, inputs, _inferenceSession.OutputNames);

            var data = output.ToList()[0].GetTensorDataAsSpan<float>().ToArray();


            var sentence_embeddings = Pooling.MeanPooling(data, encodedCorpus.AttentionMask, batchSize, encodedCorpus.AttentionMask.Length);

          

            if (!meanPooling && !normalize)
            {
                return data;
            }
            else if (meanPooling && !normalize)
            {
                return sentence_embeddings.data<float>().ToArray();
            }
            else if (meanPooling && normalize)
            {
                return Normalization.Normalize(sentence_embeddings);
            }
            else // meanPooling == false && normalize == true
            {
                torch.Tensor tokenEmbeddings = torch.tensor(
                    data,
                    [batchSize, encodedCorpus.AttentionMask.Length, 384]);
                return Normalization.Normalize(tokenEmbeddings);
            }
        }

        public MiniLML6v2Input PrepareInput(string text)
        {
            return Encode(tokenizer.Tokenize(new[] { text }), config.MaxSequenceLength);
        }

        public MiniLML6v2Input PrepareInput(IEnumerable<string> texts)
        {
            var inputTexts = texts.ToList();
            var batchSize = inputTexts.Count;

            // Encode the inputs with Bert Tokenizer
            var tokens = tokenizer.Tokenize(inputTexts);
            var miniLML6v2Inputs = inputTexts.Select(text => Encode(
                tokens,
                tokens.Count)).ToList();

            // Convert encoded inputs to tensors
            var inputIdsTensor =
                miniLML6v2Inputs.SelectMany(b => b.InputIds).ToArray();
            var attentionMaskTensor =
                miniLML6v2Inputs.SelectMany(b => b.AttentionMask).ToArray();

            var tokenTypeIdsTensor =
                miniLML6v2Inputs.SelectMany(b => b.TokenTypeIds).ToArray();

            return new MiniLML6v2Input
            {
                InputIds = inputIdsTensor.ToArray(),
                AttentionMask = attentionMaskTensor.ToArray(),
                TokenTypeIds = tokenTypeIdsTensor.ToArray()
            };
        }

        private MiniLML6v2Input Encode(
            List<(string Token, int Index)> tokens,
            int maxSequenceLength)
        {
            var padding = Enumerable
                .Repeat(0L, maxSequenceLength - tokens.Count)
                .ToList();

            var tokenIndexes = tokens
                .Select(token => (long)token.Index)
                .Concat(padding)
                .ToArray();

            var segmentIndexes = this.GetSegmentIndexes(tokens)
                .Concat(padding)
                .ToArray();

            var inputMask =
                tokens.Select(o => 1L)
                    .Concat(padding)
                    .ToArray();

            return new MiniLML6v2Input
            {
                InputIds = tokenIndexes,
                AttentionMask = inputMask,
                TokenTypeIds = segmentIndexes
            };
        }

        private IEnumerable<long> GetSegmentIndexes(
            List<(string Token, int Index)> tokens)
        {
            var segmentIndex = 0;
            var segmentIndexes = new List<long>();

            foreach (var (token, _) in tokens)
            {
                segmentIndexes.Add(segmentIndex);

                if (token == WordPieceTokenizer.DefaultTokens.Separation)
                {
                    segmentIndex++;
                }
            }

            return segmentIndexes;
        }
    }
    public class ModelInput
    {
        public long[] InputIds { get; set; }

        public long[] AttentionMask { get; set; }

        public long[] TokenTypeIds { get; set; }
    }

}
