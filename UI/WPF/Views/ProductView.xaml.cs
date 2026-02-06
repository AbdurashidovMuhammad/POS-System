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
            await viewModel.LoadProductsCommand.ExecuteAsync(null);
        }
    }
}
