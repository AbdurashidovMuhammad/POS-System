using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using WPF.Messages;
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
        WeakReferenceMessenger.Default.Send(new NavigateToViewMessage(nameof(ProductViewModel)));
    }

    [RelayCommand]
    private void NavigateToSales()
    {
        WeakReferenceMessenger.Default.Send(new NavigateToViewMessage(nameof(SalesViewModel)));
    }

    [RelayCommand]
    private void NavigateToReports()
    {
        WeakReferenceMessenger.Default.Send(new NavigateToViewMessage(nameof(ReportViewModel)));
    }

    [RelayCommand]
    private void Logout()
    {
        _authService.Logout();
        _navigationService.ClearCache();
        _navigationService.NavigateTo<LoginViewModel>();
    }
}
