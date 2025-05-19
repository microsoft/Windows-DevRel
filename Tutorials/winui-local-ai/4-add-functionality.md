# Add Functionality

In this section, you will build the app to upload images and display these images in a gallery. You will explore the **WinUI Gallery** and **AI Dev Gallery** to leverage existing code, implementing a file upload button. This project will also guide you through using a ViewModel to manage data and display uploaded images, setting the stage for generating poems based on these images.

## Explore & Use Galleries

<!-- Add instructions to install `WinUI 3 Gallery` and `AI Dev Gallery` apps for post build -->

Go to the computers Start bar and open `WinUI 3 Gallery` and `AI Dev Gallery` apps. Take a few minutes to explore these.

For this project you'll use portions from a provided sample:

1. In the **AI Dev Gallery**, go to **Samples** > **Image** > **Describe Image**
1. On the top right corner, **Click** on **`</> Code`**
1. Click on the **Sample.xaml.cs** tab.

You can use some of this functionality to implement simple file upload button. This logic lives around line 70 in `LoadImage_Click()`.

4. Copy this entire `LoadImage_Click` function from the gallery into our project in `MainPage.xaml.cs`, replacing the current empty `LoadImage_Click` function

Since we haven’t defined the logic for SetImage yet, we can comment out that line.

5. Comment out `SetImage`, by adding ``//``
6. Add to your imports, in the `MainPage.xaml.cs` :

```c#
using Windows.Storage.Pickers;
using System;
```

<details>
  <summary>Your code should look like the following:</summary>
  
  ```c#
using Windows.Storage.Pickers;
public sealed partial class MainPage : Page
{
    public MainPage()
    {
        this.InitializeComponent();
    }
}

private void MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
{
    
}

private async void LoadImage_Click(object sender, RoutedEventArgs e)
    {
        var window = new Window();
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);

        var picker = new FileOpenPicker();

        WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

        picker.FileTypeFilter.Add(".png");
        picker.FileTypeFilter.Add(".jpeg");
        picker.FileTypeFilter.Add(".jpg");

        picker.ViewMode = PickerViewMode.Thumbnail;

        var file = await picker.PickSingleFileAsync();
        if (file != null)
        {
            using var stream = await file.OpenReadAsync();
            //await SetImage(stream);
        }
    }
}
```
</details>

This projects allows users to multi-select images. 
<!-- You can use the WinUI Gallery app to reference how to do this:

1. In the **WinUI Gallery**
1. On the top Left corner, Search  **FilePicker**
1. Locate the **Pick multiple files** section
1. **Click** on the **Source Code**
1. **Click** on **C#**

Looking at the source code, you'll see that the function call needs to change from `PickSingleFileAsync()` to `PickMultipleFilesAsync()`. As well as it needs to iterate and load each file in a loop. -->

7. In your `MainPage.xaml.cs` in the `LoadImage_Click` function locate:

```c#
var file = await picker.PickSingleFileAsync();
if (file != null)
{
    using var stream = await file.OpenReadAsync();
    //await SetImage(stream);
}
```

8. Replace from `var file` & `if` statment with:

```c#
var files = await picker.PickMultipleFilesAsync();
if (files != null && files.Count > 0)
{
    foreach (var file in files) {
        using var stream = await file.OpenReadAsync();
        //await SetImage(stream)
    }
}
```

Before you try it out, the lab has provided sample images that you can use to test out this app.

9. In a new tab, open [repo](./children-arts-craft-samples/)
10. Download images to the `Desktop`

Now try it out:

11. On the title bar, Click on **Debug** > **Start Debugging** OR on your keyboard press **F5** key
12. Click on `Upload Images`
13. Select multiple images from `Desktop`
14. Close App

## Add Saving & Displaying images

Before going any further, a ViewModel can be used to manage the presentation logic as well as transforms data from the Model into a form that the View can easily display. The LoadImage_Click will trigger the reaction of displaying images, which the ViewModel can handle.

Add a ViewModel

15. In the Solution Explorer, . **Right-click** on the project node (**PoemGenerator**)
16. Click Add > New Item
17. Select **Class**
    1. If you do not see **Class**, on the top right, there is a Search bar, enter **Class**
18. Name it `MainViewModel.cs`
19. Locate the following text which makes up the Class Header:

```c#
internal class MainViewModel
```

20. Replace the class header with the following:

```c#
public partial class MainViewModel() : ObservableObject
```

The ObservableObject in the Community Toolkit is a base class that makes this ViewModel observable by implementing the `INotifyPropertyChanged` and `INotifyPropertyChanging` interfaces. This class provides automatic notification when property values change, which helps the UI update itself whenever data changes in the view model.

<!-- When this MainViewModel class inherits from ObservableObject, this class gains:

- Support for property change notifications through the PropertyChanged and PropertyChanging events.
- Access to helper methods like SetProperty, which update property values and raise notifications automatically.
- The ability to use attributes like `ObservableProperty` to simplify property declarations and generate boilerplate code for observable properties. -->

21. Move the `LoadImage_Click` function from the **MainPage.xaml.cs** to **MainViewModel.cs**
22. Rename it to `LoadImages`, make it a `Task` and remove the parameters

```c#
private async Task LoadImages()
```

23. Above the new `LoadImages` add `[RelayCommand]`
24. Add to your imports:

```csharp
using Windows.Storage.Pickers;
```


<details>
  <summary>Your code should look like the following:</summary>
  
  ```c#
using Windows.Storage.Pickers;
namespace PoemGenerator
{
    public partial class MainViewModel() : ObservableObject
    {

        [RelayCommand]
        private async Task LoadImages()
        {
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
                    using var stream = await file.OpenReadAsync();
                    //await SetImage(stream)
                }
            }

        }
    }
}
```
</details>

The `RelayCommand` automatically turns this into a command that can be bound to UI elements, specifically the buttons the View. The `RelayCommand` allows the ViewModel to handle user interactions cleanly, without requiring event handlers in the code-behind. When the  button is clicked, this action triggers this command, executing the `LoadImages` method.

Now to connect the MainViewModel to MainPage.

25. Open `MainPage.xaml.cs`
26. Above the constructor for MainPage, add a line that instantiates our view model:

```c#
public MainViewModel ViewModel { get; } = new();
```

27. In the constructor for MainPage, create an instant of the MainviewModel

```c#
ViewModel = new MainViewModel();
```

28. Save the file by pressing `Ctrl + S`

<details>
  <summary>Your code should look like the following:</summary>
  
  ```c#
public sealed partial class MainPage : Page
{
    public MainViewModel ViewModel { get; } = new();
    public MainPage()
    {
        this.InitializeComponent();
        ViewModel = new MainViewModel();
    }
```
</details>

In MainPage.xaml, we can reference our view model using x:Bind.

**Data binding** connects UI elements to data sources, enabling seamless synchronization between the user interface and underlying data. In WinUI 3, Windows App SDK, and the Community Toolkit, there are two primary ways to do data binding:

- **{x:Bind}**:
  - is faster and uses less memory because it generates special-purpose code at compile time.
  - is strongly typed and will resolve the type of each step in a path at compile time, which means it will fail at compile time if the type returned doesn’t have the member.
  - supports better debugging by enabling you to set breakpoints in the code files that are generated as the partial class for your page.

- **{Binding}**:
  - uses general-purpose runtime object inspection.
  - is more flexible but less type-safe.
  - provides runtime flexibility and supports advanced scenarios like relative source bindings and traversing the visual tree.

We use both in this lab.

> [!TIP]
> A common pitfall for new devs is using x:Bind and Binding interchangeably. X:Bind is the preferred method and Binding is generally used for complex binding scenarios that involve relative source bindings or traversing the visual tree.

Now to connect the `LoadImages()` from the ViewModel to the `MainPage.xaml`

29. Open `MainPage.xaml`
30. Locate `Click="LoadImage_Click"`
31. Replace it with:

```c#
Command="{x:Bind ViewModel.LoadImagesCommand}"
```

Now try it out:

32. On the title bar, Click on **Debug** > **Start Debugging** OR on your keyboard press **F5** key
33. Click on `Upload Images`
34. Select multiple images
35. Close App

## Saving image data

With the view model set up, we can create an `ObservableCollection` that stores references to the uploaded images. In MVVM, an observable is a variable that can trigger a UI update when its value changes. In this case, whenever our list of photos changes, we want to update the UI to reflect that. We also want to store the associated bitmap data for each image so we can feed it into the Windows AI Foundry​ APIs.

36. In the Solution Explorer, **Right Click** on the `Models` directory
37. Add > Class
38. Name it `PhotoItem.cs`
39. Update the class header to be from `internal` to `public`:

```c#
public class PhotoItem
```

40. Add the following properties inside the class:

```c#
public SoftwareBitmapSource? BitmapSource { get; internal set; }
public SoftwareBitmap? Bitmap { get; set; }
public string? Description { get; set; }
```

<details>
  <summary>Your code should look like the following:</summary>
  
  ```c#
namespace PoemGenerator.Models
{
    public class PhotoItem
    {
        public SoftwareBitmapSource? BitmapSource { get; internal set; }
        public SoftwareBitmap? Bitmap { get; set; }
        public string? Description { get; set; }
    }
}
```
</details>

41. Open `MainViewModel.cs`
42. Add to imports:

```c#
using PoemGenerator.Models;
```

43. Add a `Photos` property:

```c#
public ObservableCollection<PhotoItem> Photos { get; set; } = new();
```

When iterating through the list of images in `LoadImages`, we can replace the commented line calling `SetImage` with logic that processes each image, loads the bitmap, and saves it as a `PhotoItem`.

We can reference this logic in the **Describe Image sample** in the `AI Dev Gallery` app, from the `SetImage` function on lines 123-139, which converts the image stream from the FilePicker and loads it into a bitmap. But for this app it will be called `AddImage` and add new PhotoItem object to `Photos`.

44. Replace `//await SetImage(stream)` with:

```c#
await AddImage(stream);
```

45. Add the `AddImage` function:

```c#
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

    Photos.Add(new PhotoItem {
        BitmapSource = bitmapSource,
        Bitmap = convertedImage
    });
}
```

## Displaying images

There are a number of ways we can display a group of images in WinUI.

In the **WinUI Gallery app**, we can use the `LinedFlowLayout`, which is used by multiple Windows native apps to display images, such as the Photos and FileExplorer apps.

46. Open WinUI Gallery app
47. Search for `ItemsView`
48. Scroll down to the second sample on the page titled **ItemsView with swappable layouts**.

Here, we can see an example of the LinedFlowLayout being used within an ItemsView.  

At the top of the source XAML, we can also see a `DataTemplate` defined in the Page’s resource section. A `DataTemplate` is defined when we’re binding a collection of items to tell our app how each individual item, or model, should be displayed within the UI collection. In this case, we have a collection of PhotoItem objects. Each PhotoItem object should be displayed as a simple WinUI Image component with its BitmapSource set to the Image’s “Source” property.

> **_NOTE:_** The LinedFlowLayout lives inside of an ItemsView component, which is where we will utilize the data template mentioned above

49. Open `MainPage.xaml`
50. Before the first `Grid` element add:

```xml
<Page.Resources>
    <DataTemplate x:Key="PhotoTemplate"
                  x:DataType="model:PhotoItem">
        <ItemContainer>
            <Image Source="{x:Bind BitmapSource}"
                   Stretch="UniformToFill"/>
        </ItemContainer>
    </DataTemplate>
</Page.Resources>
```

This defines data template within the page’s resource section. The `Key` property is where you define the name of the data template. `DataType` is the class/object type that we’re defining the template for.

51. Locate the following:

```xml
xmlns:local="using:PoemGenerator.Pages"
```

52. Replace it with:

```xml
xmlns:model="using:PoemGenerator.Models"
```

By replacing the `local` namespace variable and adding model, Visual Studio should be able to locate the `PhotoItem` class.

53. Locate:

```xml
<!--Images-->
<Image></Image>
```

54. Replace it with:

```xml
<ItemsView
    ItemTemplate="{StaticResource PhotoTemplate}"
    ItemsSource="{x:Bind ViewModel.Photos, Mode=OneWay}"
    Visibility="{x:Bind ViewModel.PhotosLoaded, Mode=OneWay}">
    <ItemsView.Layout>
        <LinedFlowLayout
            ItemsStretch="Fill"
            LineHeight="160"
            LineSpacing="16"
            MinItemSpacing="16"/>
    </ItemsView.Layout>
</ItemsView>
```

In the `ItemsView` component, it is referencing the `PhotoTemplate` defined in the page’s resource and binding the list of photos that are created in our ViewModel.

The `ItemsView` component will be `PhotosLoaded` to manage it's Visibility. It should default to `false` and become `true` when the images are loaded.

55. Open `MainViewModel.cs`
56. Add the `PhotosLoaded` property:

```c#
[ObservableProperty]
public partial bool PhotosLoaded { get; set; } = false;
```

57. Locate the end of `LoadImages` funtion and after the for loop add:

```c#
PhotosLoaded = true;
```

<details>
  <summary>Your code should look like the following:</summary>
  
  ```c#
using Windows.Storage.Pickers;
using System;

namespace PoemGenerator
{
    public partial class MainViewModel() : ObservableObject
    {
        public ObservableCollection<PhotoItem> Photos { get; set; } = new();

        [ObservableProperty]
        public partial bool PhotosLoaded { get; set; } = true;

        [RelayCommand]
        private async Task LoadImages()
        {
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
                    using var stream = await file.OpenReadAsync();
                    await AddImage(stream);
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

            Photos.Add(new PhotoItem
            {
                BitmapSource = bitmapSource,
                Bitmap = convertedImage
            });
        }
    }
}
```
</details>

Now try it out:

58. On the title bar, Click on **Debug** > **Start Debugging** OR on your keyboard press **F5** key
59. Click on `Upload Images`
60. Select multiple images
61. Close App

## Generating Poem Setup

Next, you will add some databinding to make the UI reactive to various states in our application.  A few things should happen:

- The “Select poem type” and “Generate” buttons should only be visible if the user has loaded photos in the photo viewer
- While the model is generating the poem, the user should see a loading indicator
- Selecting one of the poem options should trigger a click event that updates the selected poem option
- The text generated by our AI model will be reflected in the poem viewer

You will add observable properties in the ViewModel that manage:

- If a poem is currently being generated, and
- If there are currently photos loaded in the photo viewer, and
- The text that should be displayed in the poem viewer.

62. Open `MainViewModel.cs`
63. Change `PhotosLoaded` from `true` to `false`
64. Add the following ObservableProperties:

```c#
[ObservableProperty]
public partial bool IsGeneratingPoem { get; set; } = false;

[ObservableProperty]
public partial string GeneratedPoem { get; set; } = "Select up to 5 images to generate a poem...";

public string SelectedPoemType = "Haiku";
```

65. Open `MainPage.xaml`
66. Locate the `Grid` that contains the `DropDownButton` (around line 85)
67. Replace the `Grid` opening element with:

```xml
<Grid Visibility="{x:Bind ViewModel.PhotosLoaded, Mode=OneWay}">
```

68. Locate the `ProgressRing` element
69. Replace the `IsActive` and `Visibility` with:

```xml
IsActive="{x:Bind ViewModel.IsGeneratingPoem, Mode=OneWay}"
Visibility="{x:Bind ViewModel.IsGeneratingPoem, Mode=OneWay}" />
```

70. Locate `<Run Text="Your poem will generate here"/>`
71. Replace it with:

```xml
<Run Text="{x:Bind ViewModel.GeneratedPoem, Mode=OneWay}"/>
```

A `Run` element in XAML represents a section of formatted or unformatted text within a text container, such as a TextBlock or RichTextBlock. You use the `Run` element to define a specific portion of text that can have its own formatting, separate from other text in the same container. This allows you to apply different styles to different parts of the text, such as changing the color or font of a word or phrase within a single TextBlock

72. Locate the `Button` that has the property `AutomationProperties.Name="Generate poem from selected images"`
73. Add a `command` to it:

```xml
Command="{x:Bind ViewModel.GeneratePoemCommand}"
```

<details>
  <summary>Your code should look like the following:</summary>
  
  ```xml
  <Button HorizontalAlignment="Right"
  AutomationProperties.Name="Generate poem from selected images"
  ToolTipService.ToolTip="Select images"
  Command="{x:Bind ViewModel.GeneratePoemCommand}">
    <StackPanel Orientation="Horizontal" Spacing="8">
        <FontIcon FontSize="16" Glyph="&#xE768;" />
        <TextBlock Text="Generate" />
    </StackPanel>
</Button>

```
</details>

Now to add the GeneratePoemCommand to the ViewModel:

74. Open `MainViewModel.cs`
75. Add `GeneratePoem`:

```c#
[RelayCommand]
public async Task GeneratePoem()
{
    IsGeneratingPoem = true;
    GeneratedPoem = "Generating poem…";
    // Processing image via Windows AI Foundry​ APIs here

}
```

Now try it out:

76. On the title bar, Click on **Debug** > **Start Debugging** OR on your keyboard press **F5** key
77. Click on `Upload Images`
78. Select multiple images
79. Click Generate
80. Close App

There are two items to take care of:

- Clear the Photos when the user loads images
- Have the DropDownButton stay on the selection and pass that to the ViewModel.

To ensure that the list of photos is cleared prior to making a new selection of photos

81. Open `MainViewModel`
82. In beginning of the `LoadImages` function, add:

```c#
Photos.Clear();
```

Using code-behind, update the `PoemTypeDropdownText` & `SelectedPoemType`

83. Open `MainPage.xaml.cs`
84. Update the `MenuFlyoutItem_Click` function with:

```c#
private void MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
{
    ViewModel.SelectedPoemType = ((MenuFlyoutItem)sender).Text;
    PoemTypeDropdownText.Text = ((MenuFlyoutItem)sender).Text;
}
```

Now try it out:

1. On the title bar, Click on **Debug** > **Start Debugging** OR on your keyboard press **F5** key
1. Click on `Upload Images`
1. Select multiple images
1. Click Generate
1. Close App

In this section, you successfully added image upload and display functionality to your application. By exploring the WinUI and AI Dev Galleries, you implemented a file upload button and integrated a ViewModel to manage and display image data. This image gallery now provides a foundation for generating poems based on the uploaded images.

Next [Adding Style & Icon](./5-styling.md)


<details>
  <summary>Your code should look like the following:</summary>
  
  MainPage.xaml.cs
  ```c#
public sealed partial class MainPage : Page
{
    public MainViewModel ViewModel { get; } = new();
    public MainPage()
    {
        this.InitializeComponent();
        ViewModel = new MainViewModel();
    }

    private void MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.SelectedPoemType = ((MenuFlyoutItem)sender).Text;
        PoemTypeDropdownText.Text = ((MenuFlyoutItem)sender).Text;
    }
}
```
MainPage.xaml
  ```xml
<Page
    x:Class="PoemGenerator2.Pages.MainPage"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:model="using:PoemGenerator2.Models"
    xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
    xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
    mc:Ignorable="d"
    Background="{ThemeResource ApplicationPageBackgroundThemeBrush}">

    <Page.Resources>
        <DataTemplate x:Key="PhotoTemplate"
                  x:DataType="model:PhotoItem">
            <ItemContainer>
                <Image Source="{x:Bind BitmapSource}"
                   Stretch="UniformToFill"/>
            </ItemContainer>
        </DataTemplate>
    </Page.Resources>

    <!--Photo viewer-->
    <Grid Padding="36,96,36,36"
      ColumnSpacing="36">
        <Grid.ColumnDefinitions>
            <ColumnDefinition Width="480"/>
            <ColumnDefinition Width="*"/>
        </Grid.ColumnDefinitions>

        <Grid RowSpacing="36">
            <Grid.RowDefinitions>
                <RowDefinition Height="Auto"/>
                <RowDefinition Height="*"/>
            </Grid.RowDefinitions>

            <StackPanel
            HorizontalAlignment="Center"
            Orientation="Vertical"
            Spacing="16">
                <Image Width="96" Source="/Assets/AppIcon.svg" />
                <TextBlock
                HorizontalAlignment="Center"
                Text="Poem Generator" />
                <Button
                Margin="0,8,0,0"
                HorizontalAlignment="Center"
                AutomationProperties.Name="Select up to 5 images"
                Command="{x:Bind ViewModel.LoadImagesCommand}"
                ToolTipService.ToolTip="Select images">
                    <StackPanel Orientation="Horizontal"
                            Spacing="8">
                        <FontIcon FontSize="16"
                              Glyph="&#xEE71;"/>
                        <TextBlock Text="Upload images"/>
                    </StackPanel>
                </Button>
            </StackPanel>
            <ScrollView Margin="0,16,0,0" Grid.Row="1">
                <ItemsView
                    ItemTemplate="{StaticResource PhotoTemplate}"
                    ItemsSource="{x:Bind ViewModel.Photos, Mode=OneWay}"
                    Visibility="{x:Bind ViewModel.PhotosLoaded, Mode=OneWay}">  
                    <ItemsView.Layout>
                        <LinedFlowLayout
                            ItemsStretch="Fill"
                            LineHeight="160"
                            LineSpacing="16"
                            MinItemSpacing="16"/>
                    </ItemsView.Layout>
                </ItemsView>
            </ScrollView>
        </Grid>

        <!--Poem viewer-->
        <Grid
        Grid.Column="1"
        Padding="12"
        BorderThickness="1">

            <Grid.RowDefinitions>
                <RowDefinition Height="Auto" />
                <RowDefinition Height="*" />
            </Grid.RowDefinitions>

            <Grid Visibility="{x:Bind ViewModel.PhotosLoaded, Mode=OneWay}">
                <DropDownButton AutomationProperties.Name="Select poem type" ToolTipService.ToolTip="Select poem type">
                    <DropDownButton.Flyout>
                        <MenuFlyout Placement="Bottom">
                            <MenuFlyoutItem Text="Sonnet" Click="MenuFlyoutItem_Click"/>
                            <MenuFlyoutItem Text="Haiku" Click="MenuFlyoutItem_Click"/>
                            <MenuFlyoutItem Text="Elegy" Click="MenuFlyoutItem_Click"/>
                            <MenuFlyoutItem Text="Limerick" Click="MenuFlyoutItem_Click"/>
                            <MenuFlyoutItem Text="Ballad" Click="MenuFlyoutItem_Click"/>
                            <MenuFlyoutItem Text="Free verse" Click="MenuFlyoutItem_Click"/>
                        </MenuFlyout>
                    </DropDownButton.Flyout>
                    <StackPanel Orientation="Horizontal" Spacing="8">
                        <FontIcon FontSize="16" Glyph="&#xE771;" />
                        <TextBlock x:Name="PoemTypeDropdownText" Text="Select poem type" />
                    </StackPanel>
                </DropDownButton>

                <Button HorizontalAlignment="Right"
                  AutomationProperties.Name="Generate poem from selected images"
                  ToolTipService.ToolTip="Select images"
                  Command="{x:Bind ViewModel.GeneratePoemCommand}">
                    <StackPanel Orientation="Horizontal" Spacing="8">
                        <FontIcon FontSize="16" Glyph="&#xE768;" />
                        <TextBlock Text="Generate" />
                    </StackPanel>
                </Button>
            </Grid>

            <StackPanel
            Grid.Row="1"
            HorizontalAlignment="Center"
            VerticalAlignment="Center"
            Spacing="8">

                <ProgressRing
                x:Name="Loader"
                Width="32"
                Height="32"
                IsActive="{x:Bind ViewModel.IsGeneratingPoem, Mode=OneWay}"
                Visibility="{x:Bind ViewModel.IsGeneratingPoem, Mode=OneWay}" />

                <RichTextBlock
                Margin="16"
                TextAlignment="Center">
                    <Paragraph>
                        <Run Text="{x:Bind ViewModel.GeneratedPoem, Mode=OneWay}"/>
                    </Paragraph>
                </RichTextBlock>
            </StackPanel>
        </Grid>
    </Grid>
</Page>
```

MainViewModel.cs
  ```c#
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
    private async Task LoadImages()
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
                using var stream = await file.OpenReadAsync();
                await AddImage(stream);
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

        Photos.Add(new PhotoItem
        {
            BitmapSource = bitmapSource,
            Bitmap = convertedImage
        });
    }

    [RelayCommand]
    public async Task GeneratePoem()
    {
        IsGeneratingPoem = true;
        GeneratedPoem = "Generating poem…";
        // Processing image via Windows AI Foundry​ APIs here

    }
```
</details>