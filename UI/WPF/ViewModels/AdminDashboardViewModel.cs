using System.Collections.ObjectModel;
using System.Windows.Media;
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

    [ObservableProperty]
    private string _salesChangeText = "kechagidan";

    [ObservableProperty]
    private SolidColorBrush _salesChangeBrush = new(Color.FromRgb(0x66, 0x66, 0x66));

    [ObservableProperty]
    private string _ordersChangeText = "kechagidan";

    [ObservableProperty]
    private SolidColorBrush _ordersChangeBrush = new(Color.FromRgb(0x66, 0x66, 0x66));

    // Ro'yxatlar
    [ObservableProperty]
    private ObservableCollection<TopSellingProductDto> _topSellingProducts = [];

    [ObservableProperty]
    private ObservableCollection<CashierSalesDto> _cashierSales = [];

    [ObservableProperty]
    private ObservableCollection<LowStockProductDto> _lowStockProducts = [];

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

                UpdateChange(stats.TodaySalesAmount, stats.YesterdaySalesAmount,
                    v => SalesChangeText = v, b => SalesChangeBrush = b);
                UpdateChange(stats.TodayOrdersCount, stats.YesterdayOrdersCount,
                    v => OrdersChangeText = v, b => OrdersChangeBrush = b);

                TopSellingProducts = new ObservableCollection<TopSellingProductDto>(stats.TopSellingProducts);
                HasTopProducts = TopSellingProducts.Count > 0;

                CashierSales = new ObservableCollection<CashierSalesDto>(stats.CashierSales);
                HasCashierSales = CashierSales.Count > 0;

                LowStockProducts = new ObservableCollection<LowStockProductDto>(stats.LowStockProducts);
                HasLowStockProducts = LowStockProducts.Count > 0;
                LowStockCount = LowStockProducts.Count;
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

    private static readonly SolidColorBrush GreenBrush = new(Color.FromRgb(0x4C, 0xAF, 0x50));
    private static readonly SolidColorBrush RedBrush = new(Color.FromRgb(0xF4, 0x43, 0x36));
    private static readonly SolidColorBrush GrayBrush = new(Color.FromRgb(0x66, 0x66, 0x66));

    private static void UpdateChange(decimal today, decimal yesterday,
        Action<string> setText, Action<SolidColorBrush> setBrush)
    {
        if (yesterday == 0)
        {
            setText(today > 0 ? "Kecha bo'lmagan" : "Hali yo'q");
            setBrush(today > 0 ? GreenBrush : GrayBrush);
            return;
        }

        var change = (today - yesterday) / yesterday * 100;
        setText(change >= 0 ? $"+{change:N0}% kechagidan" : $"{change:N0}% kechagidan");
        setBrush(change >= 0 ? GreenBrush : RedBrush);
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
