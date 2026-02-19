using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WPF.ViewModels;

namespace WPF.Views;

public partial class ReportView : UserControl
{
    public ReportView()
    {
        InitializeComponent();
    }

    private void SalesTab_Checked(object sender, RoutedEventArgs e)
    {
        if (DataContext is ReportViewModel vm)
        {
            vm.SelectedTabIndex = 0;
            vm.CurrentPage = 1;
        }
    }

    private void OrdersTab_Checked(object sender, RoutedEventArgs e)
    {
        if (DataContext is ReportViewModel vm)
        {
            vm.SelectedTabIndex = 1;
            vm.CurrentPage = 1;
        }
    }

    private void StockInTab_Checked(object sender, RoutedEventArgs e)
    {
        if (DataContext is ReportViewModel vm)
        {
            vm.SelectedTabIndex = 2;
            vm.CurrentPage = 1;
        }
    }

    // Toggle: xuddi tanlangan qatorni bosganda yopish
    private void OrdersDataGrid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is not DataGrid grid) return;

        var row = FindVisualParent<DataGridRow>(e.OriginalSource as DependencyObject);
        if (row is null) return;

        if (row.IsSelected)
        {
            grid.SelectedItem = null;
            if (DataContext is ReportViewModel vm)
                vm.SelectedOrderItem = null;
            e.Handled = true;
        }
    }

    private static T? FindVisualParent<T>(DependencyObject? child) where T : DependencyObject
    {
        while (child is not null)
        {
            if (child is T parent) return parent;
            child = child is System.Windows.Media.Visual or System.Windows.Media.Media3D.Visual3D
                ? System.Windows.Media.VisualTreeHelper.GetParent(child)
                : LogicalTreeHelper.GetParent(child);
        }
        return null;
    }
}
