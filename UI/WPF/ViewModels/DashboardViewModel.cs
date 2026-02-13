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

    [ObservableProperty]
    private string _todaySalesAmount = "0 so'm";

    [ObservableProperty]
    private string _todayOrdersCount = "0 ta";

    [ObservableProperty]
    private string _totalProductsCount = "0 ta";

    [ObservableProperty]
    private string _totalCategoriesCount = "0 ta";

    public DashboardViewModel(INavigationService navigationService, IAuthService authService, IApiService apiService)
    {
        _navigationService = navigationService;
        _authService = authService;
        _apiService = apiService;

        _ = LoadDashboardDataAsync();
    }

    private async Task LoadDashboardDataAsync()
    {
        await Task.WhenAll(
            LoadDashboardStatsAsync(),
            LoadRecentActivitiesAsync()
        );
    }

    private async Task LoadDashboardStatsAsync()
    {
        try
        {
            var result = await _apiService.GetAsync<DashboardStatsDto>("api/dashboard/stats");
            if (result?.Succeeded == true && result.Result != null)
            {
                var stats = result.Result;
                TodaySalesAmount = $"{stats.TodaySalesAmount:N0} so'm";
                TodayOrdersCount = $"{stats.TodayOrdersCount} ta";
                TotalProductsCount = $"{stats.TotalProductsCount} ta";
                TotalCategoriesCount = $"{stats.TotalCategoriesCount} ta";
            }
        }
        catch
        {
            // silently fail - dashboard should still work with default "0" values
        }
    }

    private async Task LoadRecentActivitiesAsync()
    {
        try
        {
            var result = await _apiService.GetAsync<PagedResult<AuditLogDto>>("api/audit-logs/my?pageSize=10");
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
