using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using WPF.ViewModels;

namespace WPF.Views;

public partial class UserView : UserControl
{
    public UserView()
    {
        InitializeComponent();
    }

    private async void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is UserViewModel viewModel)
        {
            await viewModel.LoadUsersCommand.ExecuteAsync(null);
        }
    }

    private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is UserViewModel viewModel && sender is PasswordBox passwordBox)
        {
            viewModel.FormPassword = passwordBox.Password;
        }
    }

    private void ActionMenuButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button) return;
        var viewModel = button.Tag as UserViewModel;
        if (viewModel is null) return;

        // Select the row first
        var listViewItem = FindParent<ListViewItem>(button);
        if (listViewItem is not null)
        {
            UsersListView.SelectedItem = listViewItem.DataContext;
        }

        var menu = new ContextMenu();

        var editItem = new MenuItem
        {
            Header = "Tahrirlash",
            Icon = CreateIcon("M20.71,7.04C21.1,6.65 21.1,6 20.71,5.63L18.37,3.29C18,2.9 17.35,2.9 16.96,3.29L15.12,5.12L18.87,8.87M3,17.25V21H6.75L17.81,9.93L14.06,6.18L3,17.25Z", "#4361ee")
        };
        editItem.Click += (_, _) => viewModel.EditUserCommand.Execute(null);
        menu.Items.Add(editItem);

        menu.Items.Add(new Separator());

        var isActive = viewModel.SelectedUser?.IsActive ?? true;
        var toggleItem = new MenuItem
        {
            Header = isActive ? "Nofaollashtirish" : "Faollashtirish",
            Icon = isActive
                ? CreateIcon("M12,2C17.53,2 22,6.47 22,12C22,17.53 17.53,22 12,22C6.47,22 2,17.53 2,12C2,6.47 6.47,2 12,2M15.59,7L12,10.59L8.41,7L7,8.41L10.59,12L7,15.59L8.41,17L12,13.41L15.59,17L17,15.59L13.41,12L17,8.41L15.59,7Z", "#FF9800")
                : CreateIcon("M12,2A10,10 0 0,1 22,12A10,10 0 0,1 12,22A10,10 0 0,1 2,12A10,10 0 0,1 12,2M11,16.5L18,9.5L16.59,8.09L11,13.67L7.91,10.59L6.5,12L11,16.5Z", "#4CAF50")
        };
        toggleItem.Click += (_, _) => viewModel.ToggleActiveCommand.Execute(null);
        menu.Items.Add(toggleItem);

        menu.PlacementTarget = button;
        menu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
        menu.IsOpen = true;
    }

    private static System.Windows.Shapes.Path CreateIcon(string data, string color)
    {
        return new System.Windows.Shapes.Path
        {
            Data = Geometry.Parse(data),
            Fill = new BrushConverter().ConvertFromString(color) as Brush,
            Width = 14,
            Height = 14,
            Stretch = Stretch.Uniform
        };
    }

    private static T? FindParent<T>(DependencyObject child) where T : DependencyObject
    {
        var parent = VisualTreeHelper.GetParent(child);
        while (parent is not null and not T)
        {
            parent = VisualTreeHelper.GetParent(parent);
        }
        return parent as T;
    }
}
