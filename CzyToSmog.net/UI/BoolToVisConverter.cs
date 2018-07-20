using System;
using System.Windows;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace CzyToSmog.net.UI
{
    public class BoolToVisConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var bv = value as bool?;
            if (bv != null)
            {
                return (bool)bv ? Visibility.Visible : Visibility.Collapsed;
            }

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            Visibility visibility = (Visibility)value;
            return visibility != Visibility.Visible;
        }
    }
}