using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace WPF.Converters;

public class IntToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        int count = 0;
        if (value is int intValue)
            count = intValue;

        bool invert = parameter?.ToString()?.ToLower() == "invert";
        bool hasItems = count > 0;

        if (invert)
            return hasItems ? Visibility.Collapsed : Visibility.Visible;

        return hasItems ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
