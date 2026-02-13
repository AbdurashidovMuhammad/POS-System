using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
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

    private void ActionMenuButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button) return;
        var viewModel = button.Tag as CategoryViewModel;
        if (viewModel is null) return;

        var listViewItem = FindParent<ListViewItem>(button);
        if (listViewItem is not null)
        {
            CategoriesListView.SelectedItem = listViewItem.DataContext;
        }

        var menu = new ContextMenu
        {
            FontSize = 15
        };

        var editItem = new MenuItem
        {
            Header = "Tahrirlash",
            Icon = CreateIcon("M20.71,7.04C21.1,6.65 21.1,6 20.71,5.63L18.37,3.29C18,2.9 17.35,2.9 16.96,3.29L15.12,5.12L18.87,8.87M3,17.25V21H6.75L17.81,9.93L14.06,6.18L3,17.25Z", "#4361ee"),
            Padding = new Thickness(6, 8, 20, 8)
        };
        editItem.Click += (_, _) => viewModel.EditCategoryCommand.Execute(null);
        menu.Items.Add(editItem);

        var toggleItem = new MenuItem
        {
            Header = viewModel.SelectedCategory?.IsActive == true ? "Nofaollashtirish" : "Faollashtirish",
            Icon = CreateIcon("M12,2A10,10 0 0,1 22,12A10,10 0 0,1 12,22A10,10 0 0,1 2,12A10,10 0 0,1 12,2M12,4A8,8 0 0,0 4,12A8,8 0 0,0 12,20A8,8 0 0,0 20,12A8,8 0 0,0 12,4M12,6A6,6 0 0,1 18,12A6,6 0 0,1 12,18A6,6 0 0,1 6,12A6,6 0 0,1 12,6Z", "#FF9800"),
            Padding = new Thickness(6, 8, 20, 8)
        };
        toggleItem.Click += (_, _) => viewModel.ToggleActiveCommand.Execute(null);
        menu.Items.Add(toggleItem);

        menu.Items.Add(new Separator());

        var deleteItem = new MenuItem
        {
            Header = "O'chirish",
            Foreground = new SolidColorBrush(Color.FromRgb(0xf4, 0x43, 0x36)),
            Icon = CreateIcon("M19,4H15.5L14.5,3H9.5L8.5,4H5V6H19M6,19A2,2 0 0,0 8,21H16A2,2 0 0,0 18,19V7H6V19Z", "#f44336"),
            Padding = new Thickness(6, 8, 20, 8)
        };
        deleteItem.Click += (_, _) => viewModel.DeleteCategoryCommand.Execute(null);
        menu.Items.Add(deleteItem);

        menu.PlacementTarget = button;
        menu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
        menu.IsOpen = true;
    }

    private static System.Windows.Shapes.Path CreateIcon(string data, string color)
    {
        return new System.Windows.Shapes.Path
        {
            Data = System.Windows.Media.Geometry.Parse(data),
            Fill = new BrushConverter().ConvertFromString(color) as Brush,
            Width = 18,
            Height = 18,
            Stretch = System.Windows.Media.Stretch.Uniform
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
