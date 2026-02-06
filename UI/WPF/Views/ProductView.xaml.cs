using System.Windows;
using System.Windows.Controls;
using WPF.ViewModels;

namespace WPF.Views;

public partial class ProductView : UserControl
{
    public ProductView()
    {
        InitializeComponent();
    }

    private async void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is ProductViewModel viewModel)
        {
            await viewModel.LoadCategoriesCommand.ExecuteAsync(null);
            await viewModel.LoadProductsCommand.ExecuteAsync(null);
        }
    }

    private void ProductsDataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
    {
        if (DataContext is ProductViewModel viewModel)
        {
            var rowNumber = (viewModel.CurrentPage - 1) * viewModel.PageSize + e.Row.GetIndex() + 1;
            e.Row.Header = rowNumber.ToString();
        }
        else
        {
            e.Row.Header = (e.Row.GetIndex() + 1).ToString();
        }
    }

    private void SearchTextBox_LostFocus(object sender, RoutedEventArgs e)
    {
        // Delay closing to allow click on suggestion item
        Dispatcher.BeginInvoke(new Action(() =>
        {
            if (DataContext is ProductViewModel viewModel && !SearchTextBox.IsKeyboardFocusWithin)
            {
                viewModel.CloseSuggestionsCommand.Execute(null);
            }
        }), System.Windows.Threading.DispatcherPriority.Background);
    }
}
