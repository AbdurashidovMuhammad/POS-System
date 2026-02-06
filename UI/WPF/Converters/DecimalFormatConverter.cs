using System.Globalization;
using System.Windows.Data;

namespace WPF.Converters;

public class DecimalFormatConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is decimal decimalValue)
        {
            string format = parameter?.ToString() ?? "N2";
            return decimalValue.ToString(format);
        }
        return value?.ToString() ?? string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (decimal.TryParse(value?.ToString(), out var result))
        {
            return result;
        }
        return 0m;
    }
}
