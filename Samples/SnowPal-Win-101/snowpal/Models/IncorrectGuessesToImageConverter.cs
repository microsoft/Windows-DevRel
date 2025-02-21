using Microsoft.UI.Xaml.Data;
using System;

namespace SnowPal.Models
{
    public class IncorrectGuessesToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is int incorrectGuesses)
            {
                return $"ms-appx:///Assets/snow-{incorrectGuesses}.png";
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
