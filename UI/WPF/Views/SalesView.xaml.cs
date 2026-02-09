using System.Windows;
using System.Windows.Controls;
using WPF.ViewModels;

namespace WPF.Views;

public partial class SalesView : UserControl
{
    public SalesView()
    {
        InitializeComponent();
        Loaded += SalesView_Loaded;
        Unloaded += SalesView_Unloaded;
    }

    private void SalesView_Loaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is SalesViewModel vm)
        {
            vm.SearchDialogClosed += OnSearchDialogClosed;
            vm.PropertyChanged += Vm_PropertyChanged;
        }
        BarcodeTextBox.Focus();
    }

    private void SalesView_Unloaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is SalesViewModel vm)
        {
            vm.SearchDialogClosed -= OnSearchDialogClosed;
            vm.PropertyChanged -= Vm_PropertyChanged;
        }
    }

    private void Vm_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SalesViewModel.IsSearchDialogOpen) && DataContext is SalesViewModel vm && vm.IsSearchDialogOpen)
        {
            Dispatcher.BeginInvoke(new System.Action(() => SearchDialogTextBox.Focus()),
                System.Windows.Threading.DispatcherPriority.Render);
        }

        if (e.PropertyName == nameof(SalesViewModel.SelectedSearchProduct) && DataContext is SalesViewModel vm2 && vm2.SelectedSearchProduct is not null)
        {
            Dispatcher.BeginInvoke(new System.Action(() =>
            {
                ManualQuantityTextBox.Focus();
                ManualQuantityTextBox.SelectAll();
            }), System.Windows.Threading.DispatcherPriority.Render);
        }
    }

    private void OnSearchDialogClosed()
    {
        Dispatcher.BeginInvoke(new System.Action(() => BarcodeTextBox.Focus()),
            System.Windows.Threading.DispatcherPriority.Render);
    }

    private void SearchOverlay_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (DataContext is SalesViewModel vm)
            vm.CloseSearchDialogCommand.Execute(null);
    }

    private void SearchDialog_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        e.Handled = true;
    }
}
