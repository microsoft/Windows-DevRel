using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using NReco.VideoConverter;
using Libs.VoiceActivity;
using NAudio.Wave;
using System.Text.Json;
using Windows.Storage;
using System.Diagnostics;

namespace AudioEditor
{
    public static class Utils
    {
        public static List<TranscribedChunk> ProcessTranscription(string transcription, double offsetSeconds)
        {
            Regex pattern = new Regex(@"<\|([\d.]+)\|>([^<]+)<\|([\d.]+)\|>");
            MatchCollection matches = pattern.Matches(transcription);
            List<TranscribedChunk> list = new();
            for (int i = 0; i < matches.Count; i++)
            {
                // Parse the original start and end times
                double start = double.Parse(matches[i].Groups[1].Value);
                double end = double.Parse(matches[i].Groups[3].Value);
                string subtitle = string.IsNullOrEmpty(matches[i].Groups[2].Value) ? "" : matches[i].Groups[2].Value.Trim();
                TranscribedChunk chunk = new(subtitle, start + offsetSeconds, end + offsetSeconds);
                list.Add(chunk);
            }
            return list;
        }

        public static List<TranscribedChunk> MergeTranscribedChunks(List<TranscribedChunk> chunks)
        {
            List<TranscribedChunk> list = new();
            TranscribedChunk transcribedChunk = chunks[0];
            
            for(int i=1;i< chunks.Count; i++)
            {
                char lastCharOfPrev = transcribedChunk.Transcript[transcribedChunk.Transcript.Length - 1];
                char firstCharOfNext = chunks[i].Transcript[0];
                //Approach 1: Get full sentences together //Approach 2: Sliding window of desired duration
                if (char.IsLower(firstCharOfNext) || (lastCharOfPrev != '.' && lastCharOfPrev != '?' && lastCharOfPrev != '!'))
                {
                    transcribedChunk.End = chunks[i].End;
                    transcribedChunk.Transcript += " " + chunks[i].Transcript;
                }
                else
                {
                    list.Add(transcribedChunk);
                    transcribedChunk = chunks[i];
                }
            }
            list.Add(transcribedChunk);

            return list;
        }

        public static List<TranscribedChunk> CreateDurationSizedChunkWindows(List<TranscribedChunk> chunks, double durationSeconds) {
            List<TranscribedChunk> list = new();

            TranscribedChunk currentChunk = new TranscribedChunk(chunks[0]);
            int left = 0;
            int right = 1;
            while(right < chunks.Count)
            {

                //If chunk is bigger than the duration, skip it and start the window from after
                while (right < chunks.Count && currentChunk.Length  + chunks[right].Length <= durationSeconds)
                {
                    currentChunk.End = chunks[right].End;
                    currentChunk.Transcript += chunks[right++].Transcript;
                }

                if (right >= chunks.Count) break;

                if (chunks[right].Length > durationSeconds) //Todo: I thought we should include a longer chunk just for completeness in case it is the most relevant.
                {
                    currentChunk = new TranscribedChunk(chunks[right]);            
                    left = right++;
                }
              
                list.Add(new TranscribedChunk(currentChunk));

                while (right < chunks.Count && left < right && currentChunk.Length + chunks[right].Length > durationSeconds)
                {
                    currentChunk.Transcript = currentChunk.Length > chunks[left].Length ? currentChunk.Transcript.Substring(chunks[left].Transcript.Length) : "";
                    currentChunk.Start = chunks[++left].Start;
                    
                }
                
            }

            list.Add(new TranscribedChunk(currentChunk.Transcript, currentChunk.Start, currentChunk.End));

            return list;
        }

        public static void SaveTranscriptionToFile(List<TranscribedChunk> transcribedChunks, string fileName)
        {
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            JsonSerializerOptions options = new()
            {
                WriteIndented = true
            };

            JsonSerializerOptions optionsCopy = new(options);
            var json = JsonSerializer.Serialize(transcribedChunks);
            File.WriteAllText(Path.Combine(localFolder.Path,fileName + "transcription.json"), json);
        }

        public static List<TranscribedChunk> RetrieveTranscriptionFromFile(string fileName)
        {
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            if (File.Exists(Path.Combine(localFolder.Path, fileName + "transcription.json"))) {
                var loadedChunks = JsonSerializer.Deserialize<List<TranscribedChunk>>(File.ReadAllText(Path.Combine(localFolder.Path, fileName + "transcription.json")));
                return loadedChunks;
            }
            else
            {
                return null;
            }
        }
        
        public static bool DoesTranscriptionExist(string fileName)
        {
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            return File.Exists(Path.Combine(localFolder.Path, fileName + "transcription.json"));
        }

        public static List<TranscribedChunk> GenerateRelevantSnipsWithDuration(List<TranscribedChunk> transcribedChunks, int durationSeconds)
        {
            int durationSoFar = 0;
            List<TranscribedChunk> list = new();
            foreach (TranscribedChunk chunk in transcribedChunks)
            {
                durationSoFar += (int)chunk.Length;
                if (durationSoFar > durationSeconds)
                {
                    return list;
                }

                list.Add(chunk);
            }
            return list;
        }
        public static byte[] LoadAudioBytes(string file)
        {
            var ffmpeg = new FFMpegConverter();
            var output = new MemoryStream();

            var extension = Path.GetExtension(file).Substring(1);

            // Convert to PCM
            ffmpeg.ConvertMedia(inputFile: file,
                                inputFormat: extension,
                                outputStream: output,
                                //  DE s16le PCM signed 16-bit little-endian
                                outputFormat: "s16le",
                                new ConvertSettings()
                                {
                                    AudioCodec = "pcm_s16le",
                                    AudioSampleRate = 16000,
                                    // Convert to mono
                                    CustomOutputArgs = "-ac 1"
                                });

            return output.ToArray();
        }


        public static byte[] ExtractAudioSegment(string inPath, double startTimeInSeconds, double segmentDurationInSeconds)
        {
            try
            {
                var extension = System.IO.Path.GetExtension(inPath).Substring(1);
                var output = new MemoryStream();

                var convertSettings = new ConvertSettings
                {
                    Seek = (float?)startTimeInSeconds,
                    MaxDuration = (float?)segmentDurationInSeconds,
                    //AudioCodec = "pcm_s16le",
                    AudioSampleRate = 16000,
                    CustomOutputArgs = "-vn -ac 1",
                };

                var ffMpegConverter = new FFMpegConverter();
                ffMpegConverter.ConvertMedia(
                    inputFile: inPath,
                    inputFormat: null,
                    outputStream: output,
                    outputFormat: "wav",
                    convertSettings);

                return output.ToArray();

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error during the audio extraction: " + ex.Message);
                return new byte[0]; // Return an empty array in case of exception
            }
        }

        public static void TrimMp3(string inputPath, string outputPath, TimeSpan? begin, TimeSpan? end)
        {
            if (begin.HasValue && end.HasValue && begin > end)
                throw new ArgumentOutOfRangeException("end", "end should be greater than begin");

            using (var reader = new Mp3FileReader(inputPath))
            using (var writer = File.Create(outputPath))
            {
                Mp3Frame frame;
                while ((frame = reader.ReadNextFrame()) != null)
                    if (reader.CurrentTime >= begin || !begin.HasValue)
                    {
                        if (reader.CurrentTime <= end || !end.HasValue)
                            writer.Write(frame.RawData, 0, frame.RawData.Length);
                        else break;
                    }
            }
        }

        public static string FormatTimeSpan(TimeSpan span)
        {
            return FixTimeSegmentLength(span.Hours) + ":" + FixTimeSegmentLength(span.Minutes) + ":" + FixTimeSegmentLength(span.Seconds);
        }

        public static string FixTimeSegmentLength(int timeSegment)
        {
            string castedTimeSegment = timeSegment.ToString();
            return timeSegment < 10 ? "0" + castedTimeSegment : castedTimeSegment;
        }

        public static string CreateOutputPath(string inputPath, string outputFilename)
        {
            int characterCountToRemove = Path.GetFileName(inputPath).Length;
            string extension = Path.GetExtension(outputFilename);
            return IncrementFilepath(inputPath.Remove(inputPath.Length - characterCountToRemove) + outputFilename);
        }

        public static string IncrementFilepath(string filepath)
        {   
            string currFilepath = filepath;
            string extension = Path.GetExtension(filepath);
            int i = 1;
            while(File.Exists(currFilepath)) 
            {
                Debug.WriteLine(i.ToString().Length);
                int charactersToRemove = i == 1 ? extension.Length : i.ToString().Length + extension.Length - 1;
                currFilepath = currFilepath.Remove(filepath.Length - charactersToRemove) + i.ToString() + extension;
                i++;
            }
            return currFilepath;
        }
    }
}
