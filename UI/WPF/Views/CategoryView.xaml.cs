using System.Windows;
using System.Windows.Controls;
using WPF.ViewModels;

namespace WPF.Views;

public partial class CategoryView : UserControl
{
    public CategoryView()
    {
        InitializeComponent();
    }

    private async void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is CategoryViewModel viewModel)
        {
            await viewModel.LoadCategoriesCommand.ExecuteAsync(null);
        }
    }

    private void CategoriesDataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
    {
        e.Row.Header = (e.Row.GetIndex() + 1).ToString();
    }

    private void SearchTextBox_LostFocus(object sender, RoutedEventArgs e)
    {
        Dispatcher.BeginInvoke(new Action(() =>
        {
            if (DataContext is CategoryViewModel viewModel && !SearchTextBox.IsKeyboardFocusWithin)
            {
                viewModel.CloseSuggestionsCommand.Execute(null);
            }
        }), System.Windows.Threading.DispatcherPriority.Background);
    }
}
