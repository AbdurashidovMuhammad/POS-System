using System.ComponentModel;
using System.Windows;
using WPF.ViewModels;

namespace WPF;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue is MainViewModel vm)
            vm.PropertyChanged += OnViewModelPropertyChanged;
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainViewModel.CurrentViewModel)
            && sender is MainViewModel vm
            && vm.CurrentViewModel is ShellViewModel)
        {
            WindowState = WindowState.Maximized;
        }
    }
}
