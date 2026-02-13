using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace WPF.Converters;

public class IndexConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is ListViewItem item)
        {
            var listView = ItemsControl.ItemsControlFromItemContainer(item);
            if (listView != null)
            {
                int index = listView.ItemContainerGenerator.IndexFromContainer(item);
                return (index + 1).ToString();
            }
        }
        return "0";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
