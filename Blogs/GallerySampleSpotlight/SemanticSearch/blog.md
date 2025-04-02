# Semantic Search

This blog post is the second in a series spotlighting the local AI samples contained in the [AI Dev Gallery.]() The Gallery is a preview project that aims to showcase local AI scenarios on Windows and to give developers the guidance they need to enable those scenarios themselves. The Gallery is open-source and contains a wide selection of different models and samples, including text, image, audio, and video use cases. The first blog post in the series, on Pose Estimation, can be found [here.]()

This second post will cover how to implement a semantic search with a locally running text embedding model. The Gallery currently includes the `all-MiniLM-L6-v2` and `all-MiniLM-L12-v2` models for text embedding tasks, and these models are run in the app via ONNX Runtime.

## Semantic Search At A High Level
At its core, a semantic search is a search that matches *meaning* rather than directly matching text or text similarity. For example, if a user did a search with the query term "sports," and the source text only contained the word "athletics," a traditional search would find no matches, while a semantic search would surface the relevant text results. Semantic matching enables a robust version of searching that more closely parallels a human view of information, taking into account meaning, context, and nuance.

### How do text embedding models work?
Text embedding models are the magic behind semantic search. These models convert source text to a numerical representation of the meaning of that original text. Instead of a string, you will have a vector of numerical values that captures as much of the original meaning of the text as possible. 

For different pieces of text, these vectors will have the same dimensionality (or length) which makes comparing the distance between the two vectors relatively easy. The larger the distance between two vector representations, the farther apart in embedded meaning the two texts are. And the inverse is true as well: smaller distance between embedding vectors means closer meaning.

Text embedding models aren't perfect, and have limitations. For example, they can usually only handle text of up to a specified length. For longer pieces of text, the input must be chunked into smaller pieces that are manageable by the model.

They also are often limited to capturing meaning for a single token in a sequence (like a sentence or paragraph), rather than the entire sequence. Because of this, sometime some extra processing is required to get embeddings that represent larger groupings of words.

### Enabling Semantic Search with Text Embedding
If we can capture semantic meaning with a text embedding model as described above, our step-by-step for semantic search becomes relatively straightforward. Let's assume we already have a `sourceText` string and a `searchQuery` string:

1. Assuming `sourceText` is on the longer side, it must be broken it up into small enough pieces that the embedding model can handle.
1. Once the input has been chunked, both `sourceText` and `searchQuery` must first be tokenized before they can be passed to the embeddings model. In this case, the BERT tokenizer is used.
1. The tokenized input is passed to an ONNX inference session that runs the embeddings model with the desired inputs.
1. The text embeddings are output by the inference session.
1. These are token-by-token embeddings, so the embeddings for each sequence must be averaged to get an embedding for the entire string.
1. Now that `sourceText` and `searchQuery` have been embedded, a search can be performed to find the chunks of text from `sourceText` that most closely match the meaning of the `searchQuery`. This is done by calculating the distance between two embeddings.
1. The closest matches are output as semantic matches for the `searchQuery`.

These steps will be broken down in more detail in the code walkthrough below. The full code can also be viewed alongside the sample [within the AI Dev Gallery](), or in the [GitHub repository]().

## Code Walkthrough
This code walkthrough will cover the key logic for generating text embeddings and using them to execute a semantic search. Let's start with setting up the inference session and tokenizer.

### Instantiating the inference session and tokenizer
The ONNX `InferenceSession` and the `BertTokenizer` instantiated in the constructor for the `EmbeddingGenerator` class:

```c#
// Create default session options
_sessionOptions = new SessionOptions();

// Create a new ONNX inference session
_inferenceSession = new InferenceSession(Path.Join(modelPath, "onnx", "model.onnx"), _sessionOptions);

// Create a new BERT tokenizer
_tokenizer = BertTokenizer.Create(Path.Join(modelPath, "vocab.txt"));
```

In this code:
1. A new `SessionOptions` object is created that will be passed to the inference session.
1. A new `InferenceSession` is instantiated that is passed the model path and the session options.
1. A new `BertTokenizer` is instantiated that is passed the vocabulary file that shipped with the embedding model.

The `InferenceSession` and `BertTokenizer` will be referenced in the following code to handle embedding generation.

### Generating Embeddings

Embedding generation/inference is handled in the `GetVectorsAsync` function, which takes in an `IEnumerable<string>` values and a `RunOptions` run options:

```c#
private async Task<float[][]> GetVectorsAsync(IEnumerable<string> values, RunOptions runOptions)
{
    // Clean input strings of "unprintable" characters
    values = values.Select(s => MyRegex().Replace(s, string.Empty));

    // Encode string enumerable into a EmbeddingModelInputs with the BERT tokenizer
    IEnumerable<EmbeddingModelInput> encoded = _tokenizer.EncodeBatch(values);

    // Store total values count
    int count = values.Count();
```

At the start of the `GetVectorsAsync` function:
1. The input is cleaned of any unprintable control characters that could causes issues for the tokenizer. `MyRegex` matches these characters.
1. The input is encoded with the BERT tokenizer into an enumerable of `EmbeddingModelInput` that can be used by the embedding model.

Next, the list of EmbeddingModelInputs is flattened to a single input:

```c#
    EmbeddingModelInput input = new EmbeddingModelInput
    {
        InputIds = encoded.SelectMany(t => t.InputIds).ToArray(),
        AttentionMask = encoded.SelectMany(t => t.AttentionMask).ToArray(),
        TokenTypeIds = encoded.SelectMany(t => t.TokenTypeIds).ToArray()
    };
```

A quick explanation of each of these values:
* **InputIds:** The input IDs represents the tokenized version of the string that was passed to the tokenizer. Each token is assigned a ID that correlates to a particular token in our array. You can think of this as the actual data that is being processed.
* **AttentionMask:** The attention mask is an array of values of either 1 or 0 that tells the embedding model which tokens to pay attention to. Tokens that are assigned 1 are processed while tokens that are assigned 0 are ignored. This is necessary because the tokenizer encodes all strings in a batch to be of equal length, and will pad shorter strings to match the lengths. The attention mask tells the embedding model to ignore the padding tokens.
* **TokenTypes:** The token types array assigns "types" to each token in the input. For certain use cases, this can allow separator tokens to be denoted as different than normal input tokens. For now, this can mostly be ignored as it doesn't come up much when just dealing with plain strings.

Now that the input is flattened, it needs to converted to `OrtValues` so that it matches ONNX's expected input format:

```c#
    // Get the length of each of the input sequences by dividing the total length by the count
    int sequenceLength = input.InputIds.Length / count;

    // Create input tensors over the input data using count and sequence length to define the shape
    using var inputIdsOrtValue = OrtValue.CreateTensorValueFromMemory(
        input.InputIds,
        [count, sequenceLength]);

    using var attMaskOrtValue = OrtValue.CreateTensorValueFromMemory(
        input.AttentionMask,
        [count, sequenceLength]);

    using var typeIdsOrtValue = OrtValue.CreateTensorValueFromMemory(
        input.TokenTypeIds,
        [count, sequenceLength]);
```

Each of the fields of the flattened `EmbeddingModelInput` is passed to `CreateTensorValueFromMemory` along with a shape define as `[count, sequenceLength]`. The input IDs, attention mask, and token types will each get their own input tensor with `count` number of entries, all of the same length `sequenceLength`.

Next, these values are added to a list and a corresponding list of input names is created:

```c#
    // Input names that correspond with input OrtValues, ONNX expects this
    var inputNames = new List<string>
    {
        "input_ids",
        "attention_mask",
        "token_type_ids"
    };

    // Move OrtValues that were created in a list
    var inputs = new List<OrtValue>
    {
        { inputIdsOrtValue },
        { attMaskOrtValue },
        { typeIdsOrtValue }
    };
```

Both of this lists will be passed when inference is called to tell ONNX how to process the input. The last thing left before embeddings are generated is to define an output tensor:

```c#
    // ALlocate an output tensor of the expected dimensionality (Sequence Count x Sequence Length x 384)
    using var output = OrtValue.CreateAllocatedTensorValue(OrtAllocator.DefaultInstance, TensorElementType.Float, [count, sequenceLength, 384]);
```

This tensor has a similar shape to the input, but the third dimension is specified as 384, which is the number of float values that define each individual token embedding. For every sequence we inferred on, there will be 384 output values for every token in that sequence: `Number of Sequences x Number of Tokens in each sequence x 384 embedding values`.

The last thing left to do is call for inference:

```c#
    // Run inference using the input and output fields that were defined in the above steps
    await _inferenceSession.RunAsync(runOptions, inputNames, inputs, _inferenceSession.OutputNames, [output]);
```

And then process the output:

```C#
    // Get the type and shape of the output to help with post-processing operations
    var typeAndShape = output.GetTensorTypeAndShape();

    // Mean pool token-level embeddings to get sequence-level embeddings
    ReadOnlyTensorSpan<float> sentence_embeddings = MeanPooling(output.GetTensorDataAsSpan<float>(), input.AttentionMask, typeAndShape.Shape);

    // Normalize final embedding values to smooth out outliers
    float[] resultArray = NormalizeSentenceEmbeddings(sentence_embeddings, typeAndShape.Shape);
```


The output processing for sentence embedding involves quite a bit of tensor math, so we can break down what these functions do at a high-level and link out to the source code if you want to dive into them deeper.

1. **Mean Pooling:** This step is necessary to get from token-level embeddings to sequence-level embeddings. Essentially, the model creates an embedding for every individual token in a sequence. If we want embeddings for each full sequence instead, we need to average out the token embedding values for every single token in each sequence. This is what mean pooling does: it reduces the dimensionality of the output from `Sequence Count x Sequence Length x 384` to `Sequence Count x 384` through averaging. After mean pooling the embeddings, there is a single embedding for each sequence, rather than an embedding for every token in that sequence. View [the code for this function]() for implementation details.
1. **Normalization:** This step smooths the sequence-level embeddings to prevent outliers and data anomalies from affecting embedding comparisons later on in our code. Every embedding is divided by the square root of the sum of all its squared values. This normalizes each embedding relative to its own scale, preventing particularly large or small embeddings from shifting comparisons. View [the code for this function]() for implementation details.

Once the output has been pooled and processed, it just needs to be chunked and then returned:

```c#
    return Enumerable.Chunk(resultArray, resultArray.Length / count).ToArray();
```

The call to `Enumerable.Chunk` ensures that each float array that we return is the proper size for a single embedding.

### Creating Embeddings

Let's take a look at how we build properly formatted embeddings from the float array output of `GetVectorsAsync`:

```c#
// Make the call to GetVectorsAsync to get the embedding data
float[][] vectors = await GetVectorsAsync(values, runOptions).ConfigureAwait(false);

// Instantiate a new GeneratedEmbeddings object to store each float Embedding
var generatedEmbeddings = new GeneratedEmbeddings<Embedding<float>>();

// Add each vector as a new Embedding<float> to the GeneratedEmbeddings
generatedEmbeddings.AddRange(vectors.Select(x => new Embedding<float>(x)));
```

`GetVectorsAsync` outputs a two-dimensional float array. Each sub-array represents a single embedding and is added to the `GeneratedEmbeddings` as an `Embedding<float>`. Its important to note that the order of these are preserved, so the text that corresponds with the embedding can be found via indexing the original source content.

Next, let's look at how we search for semantic matches.

### Putting It All Together: Semantic Search

We can use the `Microsoft.Extensions.VectorData` library to do a lot of the heavy lifting for implementing a vector search functionality.

To start, an `InMemoryVectorStore` is instantiated, which is used to fetch/create a `IVectorStoreRecordCollection`

```c#
// Create an InMemoryVectorStore
IVectorStore? vectorStore = new InMemoryVectorStore();

// Query the vector store for the VectoreStoreRecordCollection associated with the embeddings
IVectorStoreRecordCollection<int, StringData> embeddingsCollection = vectorStore.GetCollection<int, StringData>("embeddings");

// Double check that that collection exists and create it if not
await embeddingsCollection.CreateCollectionIfNotExistsAsync(ct).ConfigureAwait(false);
```

The above code sets up a `IVectorStoreRecordCollection<int, StringData>` that will be used to store the embeddings. This interface includes defines the `VectorizedSearch` function which can be used to perform a semantic search with embeddings.

Next, the input text is chunked to meet length requirements and passed to the embeddings generator:

```C#
// Chunk the input source text under a maximum length requirement, in this case, 512
List<string> sourceContent = ChunkSourceText(sourceText, 512);

// Generate embeddings for both the search text and the source content
// GenerateAsync will eventually make a call to GetVectorsAsync, where inference and processing was implemented
GeneratedEmbeddings<Embedding<float>> searchVectors = await _embeddings.GenerateAsync([searchText], null, ct).ConfigureAwait(false);
GeneratedEmbeddings<Embedding<float>> sourceVectors = await _embeddings.GenerateAsync(sourceContent, null, ct).ConfigureAwait(false);
```

After this code executes, `searchVectors` and `sourceVectors` contain the embedding vector data for both the search text and source content, respectively.

Now, the sourceVectors are upserted into the `IVectorStoreRecordCollection`:

```C#
// Call to UpsertBatchAsync
await foreach (var key in embeddingsCollection.UpsertBatchAsync(
    // Map each source vector to a StringData object to add to the collection
    sourceVectors.Select((x, i) => new StringData
    {
        Key = i,
        Text = sourceContent[i], 
        Vector = x.Vector
    }),
    ct).ConfigureAwait(false))
{
    // No action here, just iterating over entire enumerable.
}
```

Every embedding that was generated from the source content will have a corresponding entry in the record collection.

We can now finally execute our semantic search using the `VectorizedSearch` function exposed through the `IVectorStoreRecordCollection` interface:

```C#
// Call to the vectorized search via the embeddings collection
VectorSearchResults<StringData> vectorSearchResults = await embeddingsCollection.VectorizedSearchAsync(
    searchVectors[0].Vector, // Pass in the search vector
    new VectorSearchOptions<StringData> // Pass in search options
    {
        Top = 5, // Number of results to return
        VectorProperty = (str) => str.Vector // The mapping to the vector property to compare on
    },
    ct).ConfigureAwait(false);
```

This search will return the 5 closest (or most similar in meaning) embedding vectors to the search vector stored in `searchVectors[0].Vector`. The closest vectors are found using a regular distance formula, but with 384 dimensions for each embedding value isntead of 2 or 3 as in real space.

The output is returned as a `VectorSearchResults<StringData>` and the final output strings can be accessed like so:

```C#
var resultMessage = string.Join("\n\n", vectorSearchResults.Results.ToBlockingEnumerable().Select(r => r.Record.Text));
```

Woohoo! That's all it takes to implement an effecive semantic search with a local text embedding model. 

This walkthrough left out a lot of the extra code in the sample for updating the UI / error handling, but you can see the full sample code and try it out yourself in the [AI Dev Gallery]().

## Next Steps
If you enjoyed this post and want to see more of what's possible with local AI on Windows, check out the [AI Dev Gallery]().

If you have any feedback, want to open an issue, or contribute to the Gallery, please visit the [Github Repository]().

Lastly, if you enjoyed this article, check out the others in the series for walkthroughs of other Gallery samples:
* [Pose Estimation with the AI Dev Gallery]()