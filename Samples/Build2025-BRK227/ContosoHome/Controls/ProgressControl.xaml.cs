using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;

namespace ContosoHome.Controls;

public sealed partial class ProgressControl : UserControl
{
    public event EventHandler? CancelButtonClicked;

    public static readonly DependencyProperty ProgressValueProperty = DependencyProperty.Register(
        nameof(ProgressValue),
        typeof(int),
        typeof(ProgressControl),
        new PropertyMetadata(defaultValue: 0));

    public int ProgressValue
    {
        get => (int)GetValue(ProgressValueProperty);
        set => SetValue(ProgressValueProperty, value);
    }

    public static readonly DependencyProperty LabelProperty = DependencyProperty.Register(
        nameof(Label),
        typeof(string),
        typeof(ProgressControl),
        new PropertyMetadata(defaultValue: string.Empty));

    public string Label
    {
        get => (string)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    public ProgressControl()
    {
        InitializeComponent();
    }

    private void cancelButton_Click(object sender, RoutedEventArgs e)
    {
        CancelButtonClicked?.Invoke(this, EventArgs.Empty);
    }
}
