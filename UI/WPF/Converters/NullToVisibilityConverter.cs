using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace WPF.Converters;

public class NullToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool hasValue = value is string str
            ? !string.IsNullOrWhiteSpace(str)
            : value != null;
        bool invert = parameter?.ToString()?.ToLower() == "invert";
        if (invert) hasValue = !hasValue;
        return hasValue ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
