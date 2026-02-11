using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using WPF.Messages;
using WPF.Models;
using WPF.Services;

namespace WPF.ViewModels;

public partial class DashboardViewModel : ViewModelBase
{
    private readonly INavigationService _navigationService;
    private readonly IAuthService _authService;
    private readonly IApiService _apiService;

    [ObservableProperty]
    private ObservableCollection<AuditLogDto> _recentActivities = [];

    [ObservableProperty]
    private bool _hasActivities;

    public DashboardViewModel(INavigationService navigationService, IAuthService authService, IApiService apiService)
    {
        _navigationService = navigationService;
        _authService = authService;
        _apiService = apiService;

        _ = LoadRecentActivitiesAsync();
    }

    private async Task LoadRecentActivitiesAsync()
    {
        try
        {
            var result = await _apiService.GetAsync<PagedResult<AuditLogDto>>("audit-logs/my?pageSize=10");
            if (result?.Succeeded == true && result.Result?.Items != null)
            {
                RecentActivities = new ObservableCollection<AuditLogDto>(result.Result.Items);
                HasActivities = RecentActivities.Count > 0;
            }
        }
        catch
        {
            // silently fail - dashboard should still work without activity
        }
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
