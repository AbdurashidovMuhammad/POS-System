using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
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

    private void CopyBarcode_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is string barcode && !string.IsNullOrEmpty(barcode))
        {
            try
            {
                Clipboard.SetText(barcode);
            }
            catch (System.Runtime.InteropServices.COMException)
            {
            }
        }
    }

    private void ActionMenuButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button) return;
        var viewModel = button.Tag as ProductViewModel;
        if (viewModel is null) return;

        // Select the row first
        var row = FindParent<DataGridRow>(button);
        if (row is not null)
        {
            ProductsDataGrid.SelectedItem = row.DataContext;
        }

        var menu = new ContextMenu();

        var barcodeItem = new MenuItem
        {
            Header = "Shtrix kod",
            Icon = CreateIcon("M2,6H4V18H2V6M5,6H6V18H5V6M7,6H10V18H7V6M11,6H12V18H11V6M14,6H16V18H14V6M17,6H20V18H17V6M21,6H22V18H21V6Z", "#333")
        };
        barcodeItem.Click += (_, _) => viewModel.ShowBarcodeCommand.Execute(null);
        menu.Items.Add(barcodeItem);

        menu.Items.Add(new Separator());

        var editItem = new MenuItem
        {
            Header = "Tahrirlash",
            Icon = CreateIcon("M20.71,7.04C21.1,6.65 21.1,6 20.71,5.63L18.37,3.29C18,2.9 17.35,2.9 16.96,3.29L15.12,5.12L18.87,8.87M3,17.25V21H6.75L17.81,9.93L14.06,6.18L3,17.25Z", "#2196F3")
        };
        editItem.Click += (_, _) => viewModel.EditProductCommand.Execute(null);
        menu.Items.Add(editItem);

        var stockItem = new MenuItem
        {
            Header = "Zaxira qo'shish",
            Icon = CreateIcon("M19,13H13V19H11V13H5V11H11V5H13V11H19V13Z", "#FF9800")
        };
        stockItem.Click += (_, _) => viewModel.ShowStockInPanelCommand.Execute(null);
        menu.Items.Add(stockItem);

        menu.Items.Add(new Separator());

        var deleteItem = new MenuItem
        {
            Header = "O'chirish",
            Foreground = new SolidColorBrush(Color.FromRgb(0xf4, 0x43, 0x36)),
            Icon = CreateIcon("M19,4H15.5L14.5,3H9.5L8.5,4H5V6H19M6,19A2,2 0 0,0 8,21H16A2,2 0 0,0 18,19V7H6V19Z", "#f44336")
        };
        deleteItem.Click += (_, _) => viewModel.DeleteProductCommand.Execute(null);
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
            Width = 14,
            Height = 14,
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

    private void BarcodeOverlay_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (DataContext is ProductViewModel viewModel)
        {
            viewModel.CloseBarcodeDialogCommand.Execute(null);
        }
    }

    private void BarcodeDialog_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        e.Handled = true;
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
