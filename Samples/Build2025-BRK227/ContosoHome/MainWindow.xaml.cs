using ContosoHome.Pages;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;

namespace ContosoHome;

public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        this.InitializeComponent();
        this.ExtendsContentIntoTitleBar = true;
        this.Title = "Contoso Home";
        this.AppWindow.Title = "Contoso Home";
        this.AppWindow.SetIcon("ms-appx:///Assets/Icon.ico");
        this.SetTitleBar(TitleBar);
    }

    private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        Type selectedPage = typeof(DevicePage);

        if (args.IsSettingsSelected)
        {
            selectedPage = typeof(SettingsPage);
        }

        NavFrame.Navigate(selectedPage);
    }
}