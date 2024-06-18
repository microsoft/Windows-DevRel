using Libs.VoiceActivity;
using Libs.VoiceRecognition;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using System.Linq;
using Libs.SemanticSearch.MiniLM;
using Libs.SemanticSearch;
using static Libs.VoiceRecognition.Whisper;

namespace AudioEditor
{
    public static class SmartTrimming
    {
        public static async Task<List<TranscribedChunk>> ChunkAndTranscribe(string audioPath)
        {
            string outputFilename = Path.GetFileNameWithoutExtension(audioPath);

            List<TranscribedChunk> transcribedChunks = Utils.RetrieveTranscriptionFromFile(outputFilename) ?? new List<TranscribedChunk>();

            if (transcribedChunks.Count != 0)
            {
                return transcribedChunks;
            }

            byte[] audioBytes = Utils.LoadAudioBytes(audioPath);
            List<AudioChunk> dynamicChunks = AudioChunking.SmartChunking(audioBytes);

            foreach (var chunk in dynamicChunks.Select((value, i) => (value, i)))
            {
                byte[] audioSegment = Utils.ExtractAudioSegment(audioPath, chunk.value.start, chunk.value.end - chunk.value.start);
                Whisper whisperModel = new Whisper();
                List<TranscribedChunk> transcription = await whisperModel.TranscribeAsync(audioSegment, "en", TaskType.Transcribe, chunk.value.start);
                transcribedChunks.AddRange(transcription);
            }

            transcribedChunks = Utils.MergeTranscribedChunks(transcribedChunks);
            Utils.SaveTranscriptionToFile(transcribedChunks, outputFilename);
            return transcribedChunks;
        }

        public static List<TranscribedChunk> ApplySemanticSearch(List<TranscribedChunk> listOfChunks, string searchQuery, int durationSeconds = 30)
        {
            MiniLML6v2 miniLM = new MiniLML6v2(new MiniLML6v2Config());

            listOfChunks = Utils.CreateDurationSizedChunkWindows(listOfChunks, durationSeconds);

            string[] corpusArray = listOfChunks.Select(x => x.Transcript).ToArray();

            string[] searchQueryArray = { searchQuery };

            // Generate embeddings for the corpus
            float[][] corpusEmbeddings = corpusArray
                .Select(text => miniLM.GenerateEmbeddings(new string[] { text }))
                .ToArray();

            // Generate embeddings for the search query
            float[] searchQueryEmbeddings = miniLM.GenerateEmbeddings(searchQueryArray); // Assuming one query

            // Calculate similarities
            float[] similarityScores = corpusEmbeddings
                .Select(embedding => Similarity.CosineSimilarity(searchQueryEmbeddings, embedding))
                .ToArray();

            // Order by similarity in desc order and select indexes
            List<TranscribedChunk> sortedIndexBySimilarity = similarityScores
                .Select((score, index) => new { Index = index, Score = score, Text = corpusArray[index] })
                .OrderByDescending(x => x.Score)
                .Select(x => listOfChunks[x.Index]).ToList();

            List<TranscribedChunk> relevantSnips = Utils.GenerateRelevantSnipsWithDuration(sortedIndexBySimilarity, durationSeconds);

            return relevantSnips;
        }
    }
}
