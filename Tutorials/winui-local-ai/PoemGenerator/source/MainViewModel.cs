using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using PoemGenerator.Model;

namespace PoemGenerator
{
    public partial class MainViewModel() : ObservableObject
    {
        public ObservableCollection<PhotoItem> Photos { get; set; } = new();

        [ObservableProperty]
        public partial bool PhotosLoaded { get; set; } = false;

        [ObservableProperty]
        public partial bool IsGeneratingPoem { get; set; } = false;

        [ObservableProperty]
        public partial string GeneratedPoem { get; set; } = "Select up to 5 images to generate a poem...";

        public string SelectedPoemType = "Haiku";

        [RelayCommand]
        public async Task LoadImages()
        {
            Photos.Clear();
            var window = new Window();
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);

            var picker = new FileOpenPicker();

            WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

            picker.FileTypeFilter.Add(".png");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".jpg");

            picker.ViewMode = PickerViewMode.Thumbnail;

            var files = await picker.PickMultipleFilesAsync();
            if (files != null && files.Count > 0)
            {
                foreach (var file in files)
                {
                    if (file != null)
                    {
                        using var stream = await file.OpenReadAsync();
                        await AddImage(stream);
                    }
                }
                PhotosLoaded = true;
            }
        }

        private async Task AddImage(IRandomAccessStream stream)
        {
            var decoder = await BitmapDecoder.CreateAsync(stream);
            SoftwareBitmap inputBitmap = await decoder.GetSoftwareBitmapAsync(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);

            if (inputBitmap == null)
            {
                return;
            }

            var bitmapSource = new SoftwareBitmapSource();

            // This conversion ensures that the image is Bgra8 and Premultiplied
            SoftwareBitmap convertedImage = SoftwareBitmap.Convert(inputBitmap, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
            await bitmapSource.SetBitmapAsync(convertedImage);

            Photos.Add(new PhotoItem { BitmapSource = bitmapSource });
        }

        [RelayCommand]
        public async Task GeneratePoem()
        {
            IsGeneratingPoem = true;
            GeneratedPoem = "Generating poem…";
	        // Processing image via Foundry APIs here

        }
    }
}
