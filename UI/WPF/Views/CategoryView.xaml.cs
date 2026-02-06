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
}
