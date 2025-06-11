using ContosoHome.Models;
using System.Collections.Generic;

namespace ContosoHome.Helpers
{
    public static class ScenesLoader
    {
        public static List<Scene> Load(int i)
        {
            return new List<Scene>
            {
                new Scene() { Path = "ms-appx:///Assets/Pictures/Picture1.png", Title = "Image", Id = i * 40, Location = Location.Seattle, AspectRatio = 1.497 },
                new Scene() { Path = "ms-appx:///Assets/Pictures/Picture2.png", Title = "Image", Id = i * 40 + 1, Location = Location.LA, AspectRatio = 1.529 },
                new Scene() { Path = "ms-appx:///Assets/Pictures/Picture3.png", Title = "Image", Id = i * 40 + 2, Location = Location.Miami, AspectRatio = 0.820 },
                new Scene() { Path = "ms-appx:///Assets/Pictures/Picture4.png", Title = "Image", Id = i * 40 + 3, Location = Location.Seattle, AspectRatio = 1.504 },
                new Scene() { Path = "ms-appx:///Assets/Pictures/Picture5.png", Title = "Image", Id = i * 40 + 4, Location = Location.LA, AspectRatio = 1.267 },
                new Scene() { Path = "ms-appx:///Assets/Pictures/Picture6.png", Title = "Image", Id = i * 40 + 5, Location = Location.Miami, AspectRatio = 0.740 },
                new Scene() { Path = "ms-appx:///Assets/Pictures/Picture7.png", Title = "Image", Id = i * 40 + 6, Location = Location.Seattle, AspectRatio = 1.488 },
                new Scene() { Path = "ms-appx:///Assets/Pictures/Picture8.png", Title = "Image", Id = i * 40 + 7, Location = Location.LA, AspectRatio = 0.727 },
                new Scene() { Path = "ms-appx:///Assets/Pictures/Picture9.png", Title = "Image", Id = i * 40 + 8, Location = Location.Miami, AspectRatio = 1.488 },
                new Scene() { Path = "ms-appx:///Assets/Pictures/Picture10.png", Title = "Image", Id = i * 40 + 9, Location = Location.Seattle, AspectRatio = 0.813 },
                new Scene() { Path = "ms-appx:///Assets/Pictures/Picture11.png", Title = "Image", Id = i * 40 + 10, Location = Location.Seattle, AspectRatio = 0.797 },
                new Scene() { Path = "ms-appx:///Assets/Pictures/Picture12.png", Title = "Image", Id = i * 40 + 11, Location = Location.LA, AspectRatio = 1.496 },
                new Scene() { Path = "ms-appx:///Assets/Pictures/Picture13.png", Title = "Image", Id = i * 40 + 12, Location = Location.Miami, AspectRatio = 0.555 },
                new Scene() { Path = "ms-appx:///Assets/Pictures/Picture14.png", Title = "Image", Id = i * 40 + 13, Location = Location.Seattle, AspectRatio = 1.777 },
                new Scene() { Path = "ms-appx:///Assets/Pictures/Picture15.png", Title = "Image", Id = i * 40 + 14, Location = Location.LA, AspectRatio = 0.858 },
                new Scene() { Path = "ms-appx:///Assets/Pictures/Picture16.png", Title = "Image", Id = i * 40 + 15, Location = Location.Miami, AspectRatio = 1.782 },
                new Scene() { Path = "ms-appx:///Assets/Pictures/Picture17.png", Title = "Image", Id = i * 40 + 16, Location = Location.Seattle, AspectRatio = 1.334 },
                new Scene() { Path = "ms-appx:///Assets/Pictures/Picture18.png", Title = "Image", Id = i * 40 + 17, Location = Location.LA, AspectRatio = 0.998 },
                new Scene() { Path = "ms-appx:///Assets/Pictures/Picture19.png", Title = "Image", Id = i * 40 + 18, Location = Location.Miami, AspectRatio = 1.790 },
                new Scene() { Path = "ms-appx:///Assets/Pictures/Picture20.png", Title = "Image", Id = i * 40 + 19, Location = Location.Seattle, AspectRatio = 0.569 },
                new Scene() { Path = "ms-appx:///Assets/Pictures/Picture21.png", Title = "Image", Id = i * 40 + 20, Location = Location.Seattle, AspectRatio = 1.787 },
                new Scene() { Path = "ms-appx:///Assets/Pictures/Picture22.png", Title = "Image", Id = i * 40 + 21, Location = Location.LA, AspectRatio = 0.742 },
                new Scene() { Path = "ms-appx:///Assets/Pictures/Picture23.png", Title = "Image", Id = i * 40 + 22, Location = Location.Miami, AspectRatio = 0.555 },
                new Scene() { Path = "ms-appx:///Assets/Pictures/Picture24.png", Title = "Image", Id = i * 40 + 23, Location = Location.Seattle, AspectRatio = 1.778 },
                new Scene() { Path = "ms-appx:///Assets/Pictures/Picture25.png", Title = "Image", Id = i * 40 + 24, Location = Location.LA, AspectRatio = 0.998 },
                new Scene() { Path = "ms-appx:///Assets/Pictures/Picture26.png", Title = "Image", Id = i * 40 + 25, Location = Location.Miami, AspectRatio = 1.002 },
                new Scene() { Path = "ms-appx:///Assets/Pictures/Picture27.png", Title = "Image", Id = i * 40 + 26, Location = Location.Seattle, AspectRatio = 0.745 },
                new Scene() { Path = "ms-appx:///Assets/Pictures/Picture28.png", Title = "Image", Id = i * 40 + 27, Location = Location.LA, AspectRatio = 0.748 },
                new Scene() { Path = "ms-appx:///Assets/Pictures/Picture29.png", Title = "Image", Id = i * 40 + 28, Location = Location.Miami, AspectRatio = 1.512 },
                new Scene() { Path = "ms-appx:///Assets/Pictures/Picture30.png", Title = "Image", Id = i * 40 + 29, Location = Location.Seattle, AspectRatio = 0.997 },
                new Scene() { Path = "ms-appx:///Assets/Pictures/Picture31.png", Title = "Image", Id = i * 40 + 30, Location = Location.Seattle, AspectRatio = 0.798 },
                new Scene() { Path = "ms-appx:///Assets/Pictures/Picture32.png", Title = "Image", Id = i * 40 + 31, Location = Location.LA, AspectRatio = 1.501 },
                new Scene() { Path = "ms-appx:///Assets/Pictures/Picture33.png", Title = "Image", Id = i * 40 + 32, Location = Location.Miami, AspectRatio = 1.509 },
                new Scene() { Path = "ms-appx:///Assets/Pictures/Picture34.png", Title = "Image", Id = i * 40 + 33, Location = Location.Seattle, AspectRatio = 1.340 },
                new Scene() { Path = "ms-appx:///Assets/Pictures/Picture35.png", Title = "Image", Id = i * 40 + 34, Location = Location.LA, AspectRatio = 1.327 },
                new Scene() { Path = "ms-appx:///Assets/Pictures/Picture36.png", Title = "Image", Id = i * 40 + 35, Location = Location.Miami, AspectRatio = 1.848 },
                new Scene() { Path = "ms-appx:///Assets/Pictures/Picture37.png", Title = "Image", Id = i * 40 + 36, Location = Location.Seattle, AspectRatio = 1.526 },
                new Scene() { Path = "ms-appx:///Assets/Pictures/Picture38.png", Title = "Image", Id = i * 40 + 37, Location = Location.LA, AspectRatio = 0.743 },
                new Scene() { Path = "ms-appx:///Assets/Pictures/Picture39.png", Title = "Image", Id = i * 40 + 38, Location = Location.Miami, AspectRatio = 0.742 },
                new Scene() { Path = "ms-appx:///Assets/Pictures/Picture40.png", Title = "Image", Id = i * 40 + 39, Location = Location.Seattle, AspectRatio = 0.742 }
            };
        }
    }
}