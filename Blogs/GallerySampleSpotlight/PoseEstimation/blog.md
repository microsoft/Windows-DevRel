# Pose Estimation with the AI Dev Gallery

## What's Going On Here?
This blog post is the first in an upcoming series that will spotlight the local AI samples contained in the new [AI Dev Gallery](). The Gallery is a recently released project that aims to showcase local AI scenarios on Windows and to give developers the guidance they need to enable those scenarios themselves. The Gallery is open-source and contains a wide selection of different models and samples, including text, image, audio, and video use cases. In addition to being able to see a given model in action, each sample contains a source code view and a button to export the sample directly to a new Visual Studio project.

The Gallery is available on the [Microsoft Store]() and is entirely open-sourced on [GitHub.]()

For this first sample spotlight, we will be taking a look at one of my favorite scenarios: Human Pose Estimation with HRNet. This samples is enabled ONNX Runtime, and deepending on the processor in your Windows device, this sample supports running on the CPU, GPU, and NPU. I'll cover how to check which hardware is supported and how to switch between them later in the post.

## Pose Estimation Demo

This sample takes in an uploaded photo and renders pose estimations onto the main human figure in the photo. It will render connections between the torso and limbs, along with five points corresponding to key facial features (eyes, nose, and ears). Before diving into the code for this sample, here's a quick video example:

[VIDEO HERE]

Let's get right to the code to see how this implemented.

## Code Walkthrough
This walkthrough will focus on essential code, and may gloss over some UI logic and helper functions. The full code for this sample can be browsed in depth in the [AI Dev Gallery.]()

When this sample is first opened, it will make an initial call to `LoadModelAsync` which looks like this:

```c#
protected override async Task LoadModelAsync(SampleNavigationParameters sampleParams)
{
    // Tell our inference session where our model lives and which hardware to run it on
    await InitModel(sampleParams.ModelPath, sampleParams.HardwareAccelerator); 
    sampleParams.NotifyCompletion();

    // Make first call to inference once model is loaded
    await DetectPose(Path.Join(Windows.ApplicationModel.Package.Current.InstalledLocation.Path, "Assets", "pose_default.png"));
}
```

In this function, a `ModelPath` and `HardwareAccelerator` are passed into our `InitModel` function, which handles instantiating an ONNX Runtime `InferenceSession` with our model location and the hardware that inference will be performed on. You can jump to [Switching to NPU or GPU Execution]() later in this post for more in depth information on how the `InferenceSession` is instantiated.

Once the model has finished initializing, this function calls an initial round of inference via `DetectPose`, which is passed a default image.

### Preprocessing, Calling For Inference, and Postprocessing Output

The inference logic, along with the required preprocessing and postprocessing, takes place in the `DetectPose` function. This is a pretty long function, so let's go through it piece by piece. First, this function checks that it was passed a valid file path and performs some updates to our XAML:

```c#
private async Task DetectPose(string filePath)
{
    // Check if the passed in file path exists, and return if not
    if (!Path.Exists(filePath))
    {
        return;
    }

    // Update XAML to put the view into the "Loading" state
    Loader.IsActive = true;
    Loader.Visibility = Visibility.Visible;
    UploadButton.Visibility = Visibility.Collapsed;
    DefaultImage.Source = new BitmapImage(new Uri(filePath));

```

Next, the input image is loaded into a `Bitmap` and then resized to the expected input size of the HRNet model (256x192) with the helper function [`ResizeBitmap`]():

```c#
// Load bitmap from image filepath
using Bitmap originalImage = new(filePath);

// Store expected input dimensions in variables, as these will be used later
int modelInputWidth = 256;
int modelInputHeight = 192;

// Resize Bitmap to expected dimensions with ResizeBitmap helper
using Bitmap resizedImage = BitmapFunctions.ResizeBitmap(originalImage, modelInputWidth, modelInputHeight);
```

Once the image is stored in a bitmap of the proper size, we create a `Tensor` of dimensionality `1x3x192x256` that will represent the image. Each dimension, in order, corresponds to these values:

* *Batch Size:* our first value of `1` is just the number of inputs that are being processed. This implementation processes a single image at a time, so the batch size is just one.
* *Color Channels:* The next dimension has a value of `3`, and coressponds to each of the typical color channels: red, green, and blue. This will define the color of each pixel in the image.
* *Width:* The next value of `256` (passed as `modelInputWidth`) is the pixel width of our image.
* *Height:* The last value of `192` (passed as `modelInputHeight`) is the pixel width of our image.

Taken as a whole, this tensor represents a single image where each pixel in that image is defined by an X (width) and Y (height) pixel value and three color values (red, green, blue). 

Also, it is good to note that processing and inference section of this function is being ran in a Task to prevent the UI from becoming blocked:

```c#
// Run our processing and inference logic as a Task to prevent the UI from being blocked
var predictions = await Task.Run(() =>
{
    // Define a tensor that represents every pixel of a single image
    Tensor<float> input = new DenseTensor<float>([1, 3, modelInputWidth, modelInputHeight]);
```

To improve the quality of the input, instead of just passing in the original pixel values to the tensor, the pixels values are normalized with the [`PreprocessBitmapWithStdDev`]() helper function. This function uses the mean of each RGB value and the standard deviation (how far a value typically varies away from its mean) to "level out" outlier color values. You can think of it as a way of preventing images with really dramatic color differences from confusing the model. This step does not affect the dimensionality of the input, and just adjusts the values that will be stored in the tensor:

```c#
// Normalize our input and store it in the "input" tensor. Dimension is still 1x3x256x192
input = BitmapFunctions.PreprocessBitmapWithStdDev(resizedImage, input);
```

There is one last small step of set up before the input is passed to the `InferenceSession`, as ONNX expects a certain input format for inference. A List of type `NamedOnnxValue` is created with only one entry representating the input tensor that was just processed. Each `NamedOnnxValue` expects a metadata name (which is grabbed from the model itself using the `InferenceSession`) and a value (the tensor that was just processed):

```c#
// Snag the input metadata name from the inference session
var inputMetadataName = _inferenceSession!.InputNames[0];

// Create a list of NamedOnnxValues, with one entry
var onnxInputs = new List<NamedOnnxValue>
{
    // Call NamedOnnxValue.CreateFromTensor and pass in input metadata name and input tensor
    NamedOnnxValue.CreateFromTensor(inputMetadataName, input)
};
```

The `onnxInputs` list that was just created is passed to the `Run` function of the `InferenceSession`. It returns a collection of `DisposableNamedOnnxValues` to be processed:

```c#
// Call Run to perform inference
using IDisposableReadOnlyCollection<DisposableNamedOnnxValue> results = _inferenceSession!.Run(onnxInputs);
```

The output of the HRNet model is a bit more verbose than a list of coordinates that correspond with human pose key points (like left knee, or right shoulder). Instead of exact predictions, it returns a heatmap for every pose key point that scores each location on the image with a probability that a certain joint exists there. So, there's a bit more work to do to get points that can be placed on an image. 

First, the function sets up the necessary values for post processing:

```c#
// Fetch the heatmaps list from the inference results
var heatmaps = results[0].AsTensor<float>();

// Get the output name from the inference session
var outputName = _inferenceSession!.OutputNames[0];

// Use the output name to get the dimensions of the output from the inference session
var outputDimensions = _inferenceSession!.OutputMetadata[outputName].Dimensions;

// Finally, get the output width and height from those dimensions
float outputWidth = outputDimensions[2];
float outputHeight = outputDimensions[3];
```

The output width and height are passed, along with the heatmaps list and the original image dimensions, to the [`PostProcessResults`]() helper function. Ths function does two actions with each heatmap:

1. It iterates over every value in the heatmap to find at which coordinates the probability is highest for each pose key point.
1. It scales that value back to the size of the original image, since it was changed when it was passed into inference. This is why the original image dimensions were passed.

From this function, a list of tuples containing the X and Y location of each key point is returned, so that they can be properly rendered onto the image:

```c#
    // Post process heatmap results to get key point coordinates
    List<(float X, float Y)> keypointCoordinates = PoseHelper.PostProcessResults(heatmaps, originalImage.Width, originalImage.Height, outputWidth, outputHeight);

    // Return those coordinates from the task
    return keypointCoordinates;
});
```

Next up is rendering.

### Rendering Pose Predictions

Rendering is handled by the `RenderPredictions` helper function which takes in the original image, the predictions that were just generated, and a marker ratio to define how large to draw the predictions on the image. Note that this code is still being called from the `DetectPose` function:

```c#
using Bitmap output = PoseHelper.RenderPredictions(originalImage, predictions, .02f);
```

Rendering predictions is pretty key to the pose estimation flow, so let's dive into this function. This function will draw two things:
* Red ellipses at each pose key point (right knee, left eye, etc.)
* Blue lines connecting joint key points (right knee to right ankle, left shoulder to left elbow, etc.) Face key points (eyes, nose, ears) do not have any connections, and will just have dots ellipses rendered for them.

The first thing the function does is set up the `Graphics`, `Pen`, and `Brush` objects necessary for drawing:

```c#
public static Bitmap RenderPredictions(Bitmap image, List<(float X, float Y)> keypoints, float markerRatio, Bitmap? baseImage = null)
{
    // Create a graphics object from the image
    using (Graphics g = Graphics.FromImage(image))
    {
        // Average out width and height of image. 
        // Ignore baseImage portion, it is used by another sample.
        var averageOfWidthAndHeight = baseImage != null ? baseImage.Width + baseImage.Height : image.Width + image.Height;

        // Get the marker size from the average dimension value and the marker ratio
        int markerSize = (int)(averageOfWidthAndHeight * markerRatio / 2);
        
        // Create a Red brush for the keypoints and a Blue pen for the connections
        Brush brush = Brushes.Red;
        using Pen linePen = new(Color.Blue, markerSize / 2);      
```

Next, a list of `(int, int)` tuples is instantiated that represents each connection. Each tuple has a `StartIdx` (where the connection starts, like left shoulder) and an `EndIdx` (where the connection ends, like left elbow). These indexes are always the same based on the output of the pose model and move from top to bottom on the human figure. As a result, you'll notice that indexes 0-4 are skipped, as those indexes represent the face key points, which don't have any connections:

```c#
// Create a list of index tuples that represents each pose connection, face key points are excluded.
List<(int StartIdx, int EndIdx)> connections =
[
    (5, 6),   // Left shoulder to right shoulder
    (5, 7),   // Left shoulder to left elbow
    (7, 9),   // Left elbow to left wrist
    (6, 8),   // Right shoulder to right elbow
    (8, 10),  // Right elbow to right wrist
    (11, 12), // Left hip to right hip
    (5, 11),  // Left shoulder to left hip
    (6, 12),  // Right shoulder to right hip
    (11, 13), // Left hip to left knee
    (13, 15), // Left knee to left ankle
    (12, 14), // Right hip to right knee
    (14, 16) // Right knee to right ankle
];
```

Next, for each tuple in that list, a blue line represenating a connection is drawn on the image with `DrawLine`. It takes in the `Pen` that was created and start and end coordinates from the keypoints list that was passed into the function:

```c#
// Iterate over connections with a foreach loop
foreach (var (startIdx, endIdx) in connections)
{
    // Store keypoint start and end values in tuples
    var (startPointX, startPointY) = keypoints[startIdx];
    var (endPointX, endPointY) = keypoints[endIdx];

    // Pass those start and end coordinates, along with the Pen, to DrawLine
    g.DrawLine(linePen, startPointX, startPointY, endPointX, endPointY);
}
```

Next, the exact same thing is done for the red ellipses representing the keypoints. The entire keypoints list is iterated over because every key point gets an indicator regardless of whether or not it was included in a connection. The red ellipses are drawn second as they should be rendered on top of the blue lines representing connections:

```c#
// Iterate over keypoints with a foreach loop
foreach (var (x, y) in keypoints)
{
    // Draw an ellipse using the red brush, the x and y coordinates, and the marker size
    g.FillEllipse(brush, x - markerSize / 2, y - markerSize / 2, markerSize, markerSize);
}
```

Now just return the image:

```c#
return image;
```

Jumping back over to `DetectPose`, the last thing left to do is to update the UI with the rendered predictions on the image:

```c#
// Convert the output to a BitmapImage
BitmapImage outputImage = BitmapFunctions.ConvertBitmapToBitmapImage(output);

// Enqueue all our UI updates to ensure they don't happen off the UI thread.
DispatcherQueue.TryEnqueue(() =>
{
    DefaultImage.Source = outputImage;
    Loader.IsActive = false;
    Loader.Visibility = Visibility.Collapsed;
    UploadButton.Visibility = Visibility.Visible;
});
```
That's it! The final output looks like this:

[IMAGE HERE]

## Switching to NPU or GPU Execution
As mentioned before, this sample supports running on the NPU or GPU, in addition to the CPU, if you have meet the correct device requirements:

* **For GPU:** A DirectML enabled Windows device without a Qualcomm NPU
* **For NPU:** A Windows device with a Qualcomm NPU

The easiest way to check if your device is NPU or GPU capable is within the sample in the Gallery. Using the Select Model dropdown, you can see which execution providers are supported on your device:

[IMAGE HERE]

I'm on a device with a Qualcomm NPU, so the Gallery is only giving me to the option to run the sample on CPU or NPU.

### How The Pose Sample Handles Switching Between Execution Providers

When the pose is selected with specific hardware accelerator, that information is passed to the `InitModel` function that handles how the inference session is instantiated. The DML execution provider enables GPU execution while the Qualcomm QNN execution provider enables NPU execution. 

It looks like this: 

```c#
private Task InitModel(string modelPath, HardwareAccelerator hardwareAccelerator)
{
    return Task.Run(() =>
    {
        // Check if we already have an inference session
        if (_inferenceSession != null)
        {
            return;
        }

        // Set up ONNX Runtime (ORT) session options object
        SessionOptions sessionOptions = new();
        sessionOptions.RegisterOrtExtensions();

        // Check if DML was passed
        if (hardwareAccelerator == HardwareAccelerator.DML)
        {
            // Add the DML execution provider if so
            sessionOptions.AppendExecutionProvider_DML(DeviceUtils.GetBestDeviceId());
        }
        else if (hardwareAccelerator == HardwareAccelerator.QNN) // Check if QNN was passed
        {
            // Add the QNN execution provider if so
            Dictionary<string, string> options = new()
            {
                { "backend_path", "QnnHtp.dll" },
                { "htp_performance_mode", "high_performance" },
                { "htp_graph_finalization_optimization_mode", "3" }
            };
            sessionOptions.AppendExecutionProvider("QNN", options);
        }
        
        // Create a new inference session with these sessionOptions, if CPU is selected, they will be default
        _inferenceSession = new InferenceSession(modelPath, sessionOptions);
    });
}
```

With this function, an `InferenceSession` can be instantiated to fit whatever execution provider is passed in that particular situation and then that `InferenceSession` can be used throughout the sample.

## What's Next

More in-depth coverage of the other samples in the gallery will be released periodically, covering a range of what is possible with local AI on Windows. Stay tuned for more sample breakdowns coming soon. 

In the meantime, go check out the [AI Dev Gallery]() to explore more samples and models on Windows. If you run into any problems, feel free to [open an issue]() on the GitHub repository. This project is open-sourced and any feedback to help us improve the Gallery is highly appreicated. 