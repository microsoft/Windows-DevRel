using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Windows.AI.Generative;
using Microsoft.Windows.AI.ContentModeration;
using Microsoft.Windows.Management.Deployment;
using Microsoft.Graphics.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Diagnostics;
using System.Collections.ObjectModel;


namespace PoemGenerator.Model
{
    public partial class AIModelService : ObservableObject
    {
        private ImageDescriptionGenerator? _imageDescriptionGenerator;

        [ObservableProperty]
        public partial bool IsModelLoading { get; set; } = true;

        public async Task InitializeModelsAsync()
        {
            Debug.WriteLine("Initializing AI models...");
            if (!LanguageModel.IsAvailable())
            {
                var op = await LanguageModel.MakeAvailableAsync();
            }
            Debug.WriteLine("Language model is available.");
            //if (!LanguageModel.GetReadyState())
            //{
            //    var op = await LanguageModel.EnsureReadyAsync();
            //}

            if (!ImageDescriptionGenerator.IsAvailable())
            {
                var result = await ImageDescriptionGenerator.MakeAvailableAsync();
                if (result.Status != PackageDeploymentStatus.CompletedSuccess)
                {
                    throw result.ExtendedError;
                }
            }
            //if (!ImageDescriptionGenerator.GetReadyState())
            //{
            //    var result = await ImageDescriptionGenerator.EnsureReadyAsync();
            //    if (result.Status != PackageDeploymentStatus.CompletedSuccess)
            //    {
            //        throw result.ExtendedError;
            //    }
            //}
            Debug.WriteLine("Image model is available.");
            IsModelLoading = false;
            Debug.WriteLine("IsModelLoading is set to: " + IsModelLoading);
        }

        public async Task<string> GeneratePoem(ObservableCollection<PhotoItem> photos, string poemType)
        {
            // Process photos to generate descriptions
            await ProcessPhotosForDescriptions(photos);

            // Generate the prompt based on image descriptions
            var imageDescriptions = string.Join(", ", photos.Select(photo => photo.Description));

            var prompt = GeneratePrompt(imageDescriptions, poemType);

            // Generate the poem using the prompt
            return await GeneratePoemFromPrompt(prompt);
        }

        private string GeneratePrompt(string imageDescriptions, string poemType)
        {
            return $"Act as the most creative writer. You will be given the descriptions of images. Use that information to create a single {poemType} inspired by the image descriptions. Here are the image descriptions: {imageDescriptions}";
        }
        public async Task<string> GeneratePoemFromPrompt(string prompt)
        {
            Debug.WriteLine(prompt);
            using var languageModel = await LanguageModel.CreateAsync();
            var result = await languageModel.GenerateResponseAsync(prompt);
            return result.Response;
        }

        private async Task ProcessPhotosForDescriptions(ObservableCollection<PhotoItem> photos)
        {
            Debug.WriteLine("ProcessPhotosForDescriptions");
            var photosToDescribe = photos.Where(photo => photo.Bitmap != null && photo.Description == null);

            await Parallel.ForEachAsync(photosToDescribe, async (photo, _) =>
            {
                var inputImage = ImageBuffer.CreateCopyFromBitmap(photo.Bitmap);
                photo.Description = await DescribeImageAsync(inputImage);
            });
        }
        public async Task<string> DescribeImageAsync(ImageBuffer inputImage)
        {
            Debug.WriteLine("DescribeImageAsync");
            // Ensure the generator is created only when needed
            if (_imageDescriptionGenerator == null)
            {
                _imageDescriptionGenerator = await ImageDescriptionGenerator.CreateAsync();
            }

            var filterOptions = new ContentFilterOptions
            {
                PromptMinSeverityLevelToBlock = { ViolentContentSeverity = SeverityLevel.Medium },
                ResponseMinSeverityLevelToBlock = { ViolentContentSeverity = SeverityLevel.Medium }
            };

            var response = await _imageDescriptionGenerator.DescribeAsync(inputImage, ImageDescriptionScenario.Caption, filterOptions);
            Debug.WriteLine("DescribeImageAsync response: " + response.Response);
            return response.Response;
        }

    }
}
