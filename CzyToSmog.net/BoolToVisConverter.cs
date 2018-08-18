using System.Windows;
namespace CzyToSmog.net.UI
{

    public class BoolToVisConverter : IValueConverter
    {
        public object Convert(
            object value, Type targetType, object parameter, CultureInfo culture)
        {
            var bv = value as bool;
            if (bv != null)
            {
                return bv ? Visibility.Visible : Visibility.Hidden;
            }
        }

        public object ConvertBack(
            object value, Type targetType, object parameter, CultureInfo culture)
        {
            Visibility visibility = (Visibility)value;
            return visibility != Visibility.Visible;
        }
    }
}