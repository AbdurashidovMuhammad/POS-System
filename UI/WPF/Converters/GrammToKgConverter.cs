using System.Globalization;
using System.Windows.Data;
using WPF.Enums;

namespace WPF.Converters;

public class GrammToKgConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length < 2 || values[0] is not decimal stockQuantity || values[1] is not UnitType unitType)
            return "0";

        if (unitType != UnitType.Gramm)
            return stockQuantity.ToString("0.##");

        var kg = Math.Floor(stockQuantity / 1000);
        var gramm = stockQuantity % 1000;

        if (kg > 0 && gramm > 0)
            return $"{kg:0} kg {gramm:0} gr";
        if (kg > 0)
            return $"{kg:0} kg";
        return $"{gramm:0} gr";
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
