using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoemGenerator.Model
{
    public class PhotoItem
    {
        public SoftwareBitmapSource? BitmapSource { get; internal set; }
        public SoftwareBitmap? Bitmap { get; set; }
        public string? Description { get; set; }
    }
}
