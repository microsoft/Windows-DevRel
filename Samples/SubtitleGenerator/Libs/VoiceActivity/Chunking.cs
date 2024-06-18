
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace SubtitleGenerator.Libs.VoiceActivity;

public struct DetectionResult
{
    public string Type { get; set; }
    public double Seconds { get; set; }
}

public class AudioChunk
{
    public double start { get; set; }
    public double end { get; set; }

    public AudioChunk(double start, double end)
    {
        this.start = start;
        this.end = end;
    }

    public double Length => end - start;

}
public static class AudioChunking
{
    private static int SAMPLE_RATE = 16000;
    private static float START_THRESHOLD = 0.25f;
    private static float END_THRESHOLD = 0.25f;
    private static int MIN_SILENCE_DURATION_MS = 1000;
    private static int SPEECH_PAD_MS = 400;
    private static int WINDOW_SIZE_SAMPLES = 3200;
    private static double MAX_CHUNK_S = 29;
    private static double MIN_CHUNK_S = 5;

    public static List<AudioChunk> SmartChunking(byte[] audioBytes)
    {
        SileroVadDetector vadDetector;
        vadDetector = new SileroVadDetector(START_THRESHOLD, END_THRESHOLD, SAMPLE_RATE, MIN_SILENCE_DURATION_MS, SPEECH_PAD_MS);

        int bytesPerSample = 1;
        int bytesPerWindow = WINDOW_SIZE_SAMPLES * bytesPerSample;

        float totalSeconds = audioBytes.Length / (SAMPLE_RATE * 2);
        var result = new List<DetectionResult>();

        for (int offset = 0; offset + bytesPerWindow <= audioBytes.Length; offset += bytesPerWindow)
        {
            byte[] data = new byte[bytesPerWindow];
            Array.Copy(audioBytes, offset, data, 0, bytesPerWindow);

            // Simulating the process as if data was being read in chunks
            try
            {
                var detectResult = vadDetector.Apply(data, true);
                // iterate over detectResult and apply the data to result:
                foreach (var (key, value) in detectResult)
                {
                    result.Add(new DetectionResult { Type = key, Seconds = value });
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine($"Error applying VAD detector: {e.Message}");
                // Depending on the need, you might want to break out of the loop or just report the error
            }
        }
        var stamps = GetTimeStamps(result, totalSeconds, MAX_CHUNK_S, MIN_CHUNK_S);
        return stamps;
    }
    private static List<AudioChunk> GetTimeStamps(List<DetectionResult> voiceAreas, double totalSeconds, double maxChunkLength, double minChunkLength)
    {

        if (totalSeconds <= maxChunkLength)
        {
            return new List<AudioChunk> { new AudioChunk(0, totalSeconds) };
        }

        voiceAreas = voiceAreas.OrderBy(va => va.Seconds).ToList();

        List<AudioChunk> chunks = new List<AudioChunk>();

        double nextChunkStart = 0.0;
        while (nextChunkStart < totalSeconds)
        {
            double idealChunkEnd = nextChunkStart + maxChunkLength;
            double chunkEnd = idealChunkEnd > totalSeconds ? totalSeconds : idealChunkEnd;

            var validVoiceAreas = voiceAreas.Where(va => va.Seconds > nextChunkStart && va.Seconds <= chunkEnd).ToList();

            if (validVoiceAreas.Any())
            {
                chunkEnd = validVoiceAreas.Last().Seconds;
            }

            chunks.Add(new AudioChunk(nextChunkStart, chunkEnd));
            nextChunkStart = chunkEnd + 0.1;
        }

        return MergeSmallChunks(chunks, maxChunkLength, minChunkLength);
    }

    private static List<AudioChunk> MergeSmallChunks(List<AudioChunk> chunks, double maxChunkLength, double minChunkLength)
    {
        for (int i = 1; i < chunks.Count; i++)
        {
            // Check if current chunk is small and can be merged with previous
            if (chunks[i].Length < minChunkLength)
            {
                double prevChunkLength = chunks[i - 1].Length;
                double combinedLength = prevChunkLength + chunks[i].Length;

                if (combinedLength <= maxChunkLength)
                {
                    chunks[i - 1].end = chunks[i].end; // Merge with previous chunk
                    chunks.RemoveAt(i); // Remove current chunk
                    i--; // Adjust index to recheck current position now pointing to next chunk
                }
            }
        }

        return chunks;
    }
}