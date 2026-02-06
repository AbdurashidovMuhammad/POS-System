using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WPF.Services;

namespace WPF.ViewModels;

public partial class DashboardViewModel : ViewModelBase
{
    private readonly INavigationService _navigationService;
    private readonly IAuthService _authService;

    public DashboardViewModel(INavigationService navigationService, IAuthService authService)
    {
        _navigationService = navigationService;
        _authService = authService;
    }

    [RelayCommand]
    private void NavigateToProducts()
    {
        _navigationService.NavigateTo<ProductViewModel>();
    }

    [RelayCommand]
    private void NavigateToSales()
    {
        _navigationService.NavigateTo<SalesViewModel>();
    }

    [RelayCommand]
    private void NavigateToReports()
    {
        _navigationService.NavigateTo<ReportViewModel>();
    }

    [RelayCommand]
    private void Logout()
    {
        _authService.Logout();
        _navigationService.NavigateTo<LoginViewModel>();
    }
}
