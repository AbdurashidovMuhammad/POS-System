using System.Windows.Controls;
using WPF.ViewModels;

namespace WPF.Views;

public partial class ReportView : UserControl
{
    public ReportView()
    {
        InitializeComponent();
    }

    private void SalesTab_Checked(object sender, System.Windows.RoutedEventArgs e)
    {
        if (DataContext is ReportViewModel vm)
        {
            vm.SelectedTabIndex = 0;
        }
    }

    private void StockInTab_Checked(object sender, System.Windows.RoutedEventArgs e)
    {
        if (DataContext is ReportViewModel vm)
        {
            vm.SelectedTabIndex = 1;
        }
    }
}
