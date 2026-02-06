using CommunityToolkit.Mvvm.ComponentModel;
using WPF.Services;

namespace WPF.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly INavigationService _navigationService;

    public MainViewModel(INavigationService navigationService)
    {
        _navigationService = navigationService;
        _navigationService.CurrentViewModelChanged += HandleViewModelChanged;
    }

    [ObservableProperty]
    private object? _currentViewModel;

    private void HandleViewModelChanged(object viewModel)
    {
        CurrentViewModel = viewModel;
    }

    public void Initialize()
    {
        _navigationService.NavigateTo<LoginViewModel>();
    }
}
