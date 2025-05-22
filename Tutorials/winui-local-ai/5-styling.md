# Add Icon & Styling

This section you begin by adding an icon to the application. Then, you transform the look and feel of the page by applying styles and theme resources to UI elements like containers, buttons, and text. This transformation ensures that your app adheres to Windows design principles, offers a consistent user experience, and is easier to maintain and update.

## Add Icon to App

In the `MainPage.xaml` there is a `Image` element that contains `Source="/Assets/AppIcon.svg"`. Now its time to add the image to the project.

1. Open in a new tab: [directory](./assets/)
1. Locate AppIcon.svg
1. Download image to Desktop
1. Have Visual Studio display `Solution Explorer` and open `Asset` directory
1. Drag and drop image from Desktop to Visual Studio into the `Asset` directory
1. **Right click** on the image
1. **Click** on **Properties**
1. A Properties panel will open
1. **Change** the Copy to Output Directory to `Copy if newer`

Now try it out:

10. On the title bar, Click on **Debug** > **Start Debugging** OR on your keyboard press **F5** key

![Screenshot of App](assets/add-image.png)

11. Close App

## Adding Style

Now to add WinUI default styling.

12. Open `MainPage.xaml`
13. Replace the `Grid` with the following:

```xml
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
                Style="{StaticResource BodyStrongTextBlockStyle}"
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
        Background="{ThemeResource CardBackgroundFillColorDefaultBrush}"
        BorderBrush="{ThemeResource CardStrokeColorDefaultBrush}"
        BorderThickness="1"
        CornerRadius="{StaticResource OverlayCornerRadius}">

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
              Style="{StaticResource AccentButtonStyle}"
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
                Foreground="{ThemeResource TextFillColorSecondaryBrush}"
                TextAlignment="Center">
                <Paragraph>
                    <Run Text="{x:Bind ViewModel.GeneratedPoem, Mode=OneWay}"/>
                </Paragraph>
            </RichTextBlock>
        </StackPanel>
    </Grid>
</Grid>
```

> **_NOTE:_** It might take Visual Studio a minute to update the ViewModel linking

Now try it out:

14. On the title bar, Click on **Debug** > **Start Debugging** OR on your keyboard press **F5** key

![Screenshot of App](assets/with-style.png)

15. Close App

## Tips and Best Practices

- **Prefer built-in styles and theme resources**: This keeps your app aligned with Windows updates and accessibility improvements.
- **Define custom styles only when necessary**: Base them on the default control style for maintainability.
- **Test in light, dark, and high-contrast modes**: Ensure your styles respond correctly to theme changes.
- **Keep resource scope in mind**: Page-level resources override app-level ones. Use app-level resources for global styles.
- **Use clear naming for styles and resources**: This makes your XAML easier to read and maintain.

Next [Add AI](./6-phi-silica.md)