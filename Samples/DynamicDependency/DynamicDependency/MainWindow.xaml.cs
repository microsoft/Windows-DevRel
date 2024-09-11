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

namespace DynamicDependency
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();
        }

        private void myButton_Click(object sender, RoutedEventArgs e)
        {
            myButton.Content = "Clicked";

            const string packageFamilyName = "khmyznikov.25605DF47F6B5_ggh13nganeqyr";
            var pkgVersion = 0x0001000000000000;
            string lifetimeArtifact = null; // As lifetimeKind is Process, this is null
            int options = (int)CreatePackageDependencyOptions.None;

            int hr_create = PackageDependency.TryCreate(
                packageFamilyName, pkgVersion, (int)PackageDependencyProcessorArchitectures.X64, (int)PackageDependencyLifetimeKind.Process,
                lifetimeArtifact, options, out string packageDependencyId);

            if (hr_create < 0)
            {
                Console.WriteLine($"Failed to create package dependency. HRESULT: {hr_create}");
                return;
            }
        }
    }
}
