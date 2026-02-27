using System.Windows;
using WPF.ViewModels;

namespace WPF.Views;

public partial class UserPermissionView : Window
{
    public UserPermissionView(UserPermissionViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        viewModel.CloseRequested += () => Close();
    }
}
