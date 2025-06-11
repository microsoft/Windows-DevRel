using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ContosoHome.Controls
{
    public sealed partial class RoomTile : UserControl
    {
        public RoomTile()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty LabelProperty = DependencyProperty.Register(
         nameof(Label),
         typeof(string),
         typeof(RoomTile),
         new PropertyMetadata(defaultValue: string.Empty));

        public string Label
        {
            get => (string)GetValue(LabelProperty);
            set => SetValue(LabelProperty, value);

        }

        public static readonly DependencyProperty ImageUrlProperty = DependencyProperty.Register(
   nameof(ImageUrl),
   typeof(string),
   typeof(RoomTile),
   new PropertyMetadata(null));

        public string ImageUrl
        {
            get => (string)GetValue(ImageUrlProperty);
            set => SetValue(ImageUrlProperty, value);
        }
    }
}
