# AI Audio Editor Sample - Code Walkthrough

This document is intended as an in-depth walkthrough of all the code required to enable the AI functionality of the Audio Editor sample. If you just want to see a high-level explanation or run the sample itself, see the [README.](./README.md)

## Structure

This project uses three different models to execute its smart trimming functionality, and all the relevant logic can be found broken down as such:

* **Voice Activity Detection and Chunking** logic with Silero VAD can be found in `/Libs/VoiceActivity/`
* **Audio Transcription** logic with Whisper can be found in `/Libs/Transcription/`
* **Text embedding and semantic similarity logic** can be found in `/Libs/SemanticSearch/`


These libraries are used by the `SmartTrimming` class found at the root of the project, which is utilized in this function in `MainWindow.xaml.cs`:

```csharp
private async Task RunTranscribeSearchAndTrimTask(AudioFile audioFile, string outputPath)
{
    await Task.Run(async () =>
    {
        List<TranscribedChunk> transcribedChunks = await SmartTrimming.ChunkAndTranscribe(audioFile.FilePath);
        List<TranscribedChunk> similaritySortedChunks = SmartTrimming.ApplySemanticSearch(transcribedChunks, audioFile.Keyword, audioFile.TrimmedDuration);
        if (similaritySortedChunks.Count > 0)
        {
            TranscribedChunk chunk = similaritySortedChunks[0];
            Utils.TrimMp3(audioFile.FilePath, outputPath, TimeSpan.FromSeconds(chunk.Start), TimeSpan.FromSeconds(chunk.End));
        }
    });
}
```

This function will be in the entry point for all of the code covered in this document.

## `ChunkAndTranscribe`

The first call we make in `RunTranscribeSearchAndTrimTask` is to `SmartTrimming.ChunkAndTranscribe`. This function accepts one argument that is a path to the target audio file. Let's step through that function:

```csharp
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
```

The first thing we do is check if this audio has already been transcribed and saved to the file system, if we so, we return that transcription:

```csharp
string outputFilename = Path.GetFileNameWithoutExtension(audioPath);

List<TranscribedChunk> transcribedChunks = Utils.RetrieveTranscriptionFromFile(outputFilename) ?? new List<TranscribedChunk>();

if (transcribedChunks.Count != 0)
{
    return transcribedChunks;
}
```

Otherwise, we load in the audio as a byte array, and pass that as the only argument to our `SmartChunking` function:

```csharp
byte[] audioBytes = Utils.LoadAudioBytes(audioPath);
List<AudioChunk> dynamicChunks = AudioChunking.SmartChunking(audioBytes);
```
***If you want to see the implementation of `SmartChunking` with Silero VAD, head over to [Chunking.cs](./Libs/VoiceActivity/Chunking.cs)***

Our chunking call returns a list of `AudioChunks`, which we can then iterate over, making a call to our transcription library (`whisperModel.TranscribeAsync`) on each iteration:

```csharp
foreach (var chunk in dynamicChunks.Select((value, i) => (value, i)))
{
    byte[] audioSegment = Utils.ExtractAudioSegment(audioPath, chunk.value.start, chunk.value.end - chunk.value.start);
    Whisper whisperModel = new Whisper();
    List<TranscribedChunk> transcription = await whisperModel.TranscribeAsync(audioSegment, "en", TaskType.Transcribe, chunk.value.start);
    transcribedChunks.AddRange(transcription);
}
```

***If you want to see the implementation of `TranscribeAsync` with Whisper, head over to [Whisper.cs](./Libs/Transcription/Whisper.cs)***

Finally, we merge our transcribed chunks, save them to a file, and return the final transcription:

```csharp
transcribedChunks = Utils.MergeTranscribedChunks(transcribedChunks);
Utils.SaveTranscriptionToFile(transcribedChunks, outputFilename);
return transcribedChunks;
```

## `ApplySemanticSearch`

Once we've transcribed, we can pass our transcription to our `ApplySemanticSearch` function. Let's step through that function: 

```csharp
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

    // Generate embeddings for the search query, assuming one query
    float[] searchQueryEmbeddings = miniLM.GenerateEmbeddings(searchQueryArray);

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
```

First, we initialize a `MiniLML6v2` instance and pass in our configuration. This the model that will handle text-embedding generation for us:

```csharp
MiniLML6v2 miniLM = new MiniLML6v2(new MiniLML6v2Config());
```

Next, we make a call to `Utils.CreateDurationSizedChunkWindows` to convert our transcription into rolling windows of the same duration as our target clip duration. Once this function returns, we convert the result and the search query into string arrays:

```csharp
listOfChunks = Utils.CreateDurationSizedChunkWindows(listOfChunks, durationSeconds);
string[] corpusArray = listOfChunks.Select(x => x.Transcript).ToArray();
string[] searchQueryArray = { searchQuery };
```

Once we have our string arrays containing both our transcript corpus and the searchQuery, we can make a call to our embeddings model to get text embeddings for both:

```csharp
// Generate embeddings for the corpus
float[][] corpusEmbeddings = corpusArray
    .Select(text => miniLM.GenerateEmbeddings(new string[] { text }))
    .ToArray();

// Generate embeddings for the search query, assuming one query
float[] searchQueryEmbeddings = miniLM.GenerateEmbeddings(searchQueryArray);
```

***If you want to see the implementation of `GenerateEmbeddings` with MiniLML6v2, head over to [GenerateEmbeddings.cs](./Libs/SemanticSearch/MiniLM/MiniLML6v2.cs)***

Next, we calculate Cosine similarity scores between the search query embedding and each of the duration window embeddings, and then we sort these similarity scores in descending order:

```csharp
// Calculate similarities
float[] similarityScores = corpusEmbeddings
    .Select(embedding => Similarity.CosineSimilarity(searchQueryEmbeddings, embedding))
    .ToArray();

// Order by similarity in desc order and select indexes
List<TranscribedChunk> sortedIndexBySimilarity = similarityScores
    .Select((score, index) => new { Index = index, Score = score, Text = corpusArray[index] })
    .OrderByDescending(x => x.Score)
    .Select(x => listOfChunks[x.Index]).ToList();
```

Lastly, we call a utility that fixes the timestamps on our transcribed chunks, and return the list:

```csharp
List<TranscribedChunk> relevantSnips = Utils.GenerateRelevantSnipsWithDuration(sortedIndexBySimilarity, durationSeconds);

return relevantSnips;
```

And that's it. The first transcribed chunk entry in the return list will be the most relevant segment. We can trim our audio file based on the start and end timestamps found in that chunk.

