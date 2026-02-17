using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using WPF.Messages;
using WPF.Models;
using WPF.Services;

namespace WPF.ViewModels;

public partial class AdminDashboardViewModel : ViewModelBase
{
    private readonly INavigationService _navigationService;
    private readonly IAuthService _authService;
    private readonly IApiService _apiService;

    [ObservableProperty]
    private bool _isRefreshing;

    // Umumiy statistika
    [ObservableProperty]
    private string _todaySalesAmount = "0 so'm";

    [ObservableProperty]
    private string _todayOrdersCount = "0 ta";

    [ObservableProperty]
    private string _totalProductsCount = "0 ta";

    [ObservableProperty]
    private string _totalCategoriesCount = "0 ta";

    // Ro'yxatlar
    [ObservableProperty]
    private ObservableCollection<TopSellingProductDto> _topSellingProducts = [];

    [ObservableProperty]
    private ObservableCollection<CashierSalesDto> _cashierSales = [];

    [ObservableProperty]
    private ObservableCollection<LowStockProductDto> _lowStockProducts = [];

    [ObservableProperty]
    private ObservableCollection<LowStockProductDto> _pagedLowStockProducts = [];

    [ObservableProperty]
    private int _lowStockCurrentPage = 1;

    [ObservableProperty]
    private int _lowStockTotalPages = 1;

    [ObservableProperty]
    private string _lowStockPageInfo = "1 / 1";

    private const int LowStockPageSize = 10;

    [ObservableProperty]
    private ObservableCollection<AuditLogDto> _recentActivities = [];

    [ObservableProperty]
    private bool _hasTopProducts;

    [ObservableProperty]
    private bool _hasCashierSales;

    [ObservableProperty]
    private bool _hasLowStockProducts;

    [ObservableProperty]
    private bool _hasActivities;

    [ObservableProperty]
    private int _lowStockCount;

    private void UpdateLowStockPage()
    {
        LowStockTotalPages = Math.Max(1, (int)Math.Ceiling((double)LowStockProducts.Count / LowStockPageSize));
        if (LowStockCurrentPage > LowStockTotalPages)
            LowStockCurrentPage = LowStockTotalPages;

        var items = LowStockProducts
            .Skip((LowStockCurrentPage - 1) * LowStockPageSize)
            .Take(LowStockPageSize);
        PagedLowStockProducts = new ObservableCollection<LowStockProductDto>(items);
        LowStockPageInfo = $"{LowStockCurrentPage} / {LowStockTotalPages}";
        OnPropertyChanged(nameof(CanLowStockPrevious));
        OnPropertyChanged(nameof(CanLowStockNext));
    }

    public bool CanLowStockPrevious => LowStockCurrentPage > 1;
    public bool CanLowStockNext => LowStockCurrentPage < LowStockTotalPages;

    [RelayCommand]
    private void LowStockPreviousPage()
    {
        if (LowStockCurrentPage > 1)
        {
            LowStockCurrentPage--;
            UpdateLowStockPage();
        }
    }

    [RelayCommand]
    private void LowStockNextPage()
    {
        if (LowStockCurrentPage < LowStockTotalPages)
        {
            LowStockCurrentPage++;
            UpdateLowStockPage();
        }
    }

    public AdminDashboardViewModel(
        INavigationService navigationService,
        IAuthService authService,
        IApiService apiService)
    {
        _navigationService = navigationService;
        _authService = authService;
        _apiService = apiService;

        _ = LoadAllDataAsync();
    }

    private async Task LoadAllDataAsync()
    {
        await Task.WhenAll(
            LoadAdminStatsAsync(),
            LoadRecentActivitiesAsync()
        );
    }

    private async Task LoadAdminStatsAsync()
    {
        try
        {
            var result = await _apiService.GetAsync<AdminDashboardStatsDto>("api/dashboard/admin-stats");
            if (result?.Succeeded == true && result.Result != null)
            {
                var stats = result.Result;

                TodaySalesAmount = $"{stats.TodaySalesAmount:N0} so'm";
                TodayOrdersCount = $"{stats.TodayOrdersCount} ta";
                TotalProductsCount = $"{stats.TotalProductsCount} ta";
                TotalCategoriesCount = $"{stats.TotalCategoriesCount} ta";

                TopSellingProducts = new ObservableCollection<TopSellingProductDto>(stats.TopSellingProducts);
                HasTopProducts = TopSellingProducts.Count > 0;

                CashierSales = new ObservableCollection<CashierSalesDto>(stats.CashierSales);
                HasCashierSales = CashierSales.Count > 0;

                LowStockProducts = new ObservableCollection<LowStockProductDto>(stats.LowStockProducts);
                HasLowStockProducts = LowStockProducts.Count > 0;
                LowStockCount = LowStockProducts.Count;
                LowStockCurrentPage = 1;
                UpdateLowStockPage();
            }
        }
        catch
        {
            // silently fail
        }
    }

    private async Task LoadRecentActivitiesAsync()
    {
        try
        {
            var result = await _apiService.GetAsync<PagedResult<AuditLogDto>>("api/audit-logs?pageSize=10");
            if (result?.Succeeded == true && result.Result?.Items != null)
            {
                RecentActivities = new ObservableCollection<AuditLogDto>(result.Result.Items);
                HasActivities = RecentActivities.Count > 0;
            }
        }
        catch
        {
            // silently fail
        }
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        IsRefreshing = true;
        try
        {
            await LoadAllDataAsync();
        }
        finally
        {
            IsRefreshing = false;
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
    private void NavigateToUsers()
    {
        WeakReferenceMessenger.Default.Send(new NavigateToViewMessage(nameof(UserViewModel)));
    }

    [RelayCommand]
    private void Logout()
    {
        _authService.Logout();
        _navigationService.ClearCache();
        _navigationService.NavigateTo<LoginViewModel>();
    }
}
