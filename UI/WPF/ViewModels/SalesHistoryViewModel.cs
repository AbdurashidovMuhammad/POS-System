using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WPF.Models;
using WPF.Services;

namespace WPF.ViewModels;

public partial class SalesHistoryViewModel : ViewModelBase
{
    private readonly IApiService _apiService;

    public SalesHistoryViewModel(IApiService apiService)
    {
        _apiService = apiService;
        StartDate = DateTime.Today;
        EndDate = DateTime.Today;
        _ = InitializeAsync();
    }

    // Dashboard stats
    [ObservableProperty]
    private string _todaySalesAmount = "0 so'm";

    [ObservableProperty]
    private string _todayOrdersCount = "0 ta";

    // Date filters
    [ObservableProperty]
    private DateTime _startDate;

    [ObservableProperty]
    private DateTime _endDate;

    // Sales list
    [ObservableProperty]
    private ObservableCollection<SalesReportItemDto> _salesItems = [];

    [ObservableProperty]
    private bool _hasSales;

    [ObservableProperty]
    private decimal _totalSalesAmount;

    [ObservableProperty]
    private int _salesItemCount;

    [ObservableProperty]
    private int _periodOrdersCount;

    // Pagination
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(PreviousPageCommand))]
    [NotifyCanExecuteChangedFor(nameof(NextPageCommand))]
    private int _currentPage = 1;

    [ObservableProperty]
    private int _pageSize = 15;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(PreviousPageCommand))]
    [NotifyCanExecuteChangedFor(nameof(NextPageCommand))]
    private int _totalPages = 1;

    [ObservableProperty]
    private int _totalCount;

    [ObservableProperty]
    private bool _isRefreshing;

    private bool CanGoToPreviousPage() => CurrentPage > 1 && !IsLoading;
    private bool CanGoToNextPage() => CurrentPage < TotalPages && !IsLoading;

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        if (e.PropertyName == nameof(IsLoading))
        {
            PreviousPageCommand.NotifyCanExecuteChanged();
            NextPageCommand.NotifyCanExecuteChanged();
        }
    }

    private async Task InitializeAsync()
    {
        await Task.WhenAll(
            LoadDashboardStatsAsync(),
            FetchSalesAsync()
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
            }
        }
        catch { }
    }

    [RelayCommand]
    private async Task LoadSalesAsync()
    {
        CurrentPage = 1;
        await FetchSalesAsync();
    }

    private async Task FetchSalesAsync()
    {
        if (StartDate.Date > EndDate.Date)
        {
            ErrorMessage = "Boshlanish sanasi tugash sanasidan katta bo'lishi mumkin emas";
            return;
        }

        IsLoading = true;
        ClearMessages();

        try
        {
            var from = StartDate.ToString("yyyy-MM-dd");
            var to = EndDate.ToString("yyyy-MM-dd");

            var result = await _apiService.GetAsync<SalesReportDto>(
                $"api/sales/my-history?from={from}&to={to}&page={CurrentPage}&pageSize={PageSize}");

            if (result?.Succeeded == true && result.Result is not null)
            {
                AssignDateGroupColors(result.Result.Items);
                SalesItems = new ObservableCollection<SalesReportItemDto>(result.Result.Items);
                TotalSalesAmount = result.Result.TotalAmount;
                SalesItemCount = result.Result.TotalCount;
                PeriodOrdersCount = result.Result.OrderCount;
                TotalPages = result.Result.TotalPages == 0 ? 1 : result.Result.TotalPages;
                TotalCount = result.Result.TotalCount;
                HasSales = SalesItems.Count > 0;
            }
            else
            {
                ErrorMessage = result?.Errors?.FirstOrDefault() ?? "Ma'lumotlarni yuklashda xatolik";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsLoading = false;
        }
    }

    private static void AssignDateGroupColors(IList<SalesReportItemDto> items)
    {
        DateTime? currentDate = null;
        bool isAlternate = false;

        foreach (var item in items)
        {
            var date = item.Date.Date;
            if (currentDate != date)
            {
                if (currentDate != null)
                    isAlternate = !isAlternate;
                currentDate = date;
            }
            item.DateGroupIsAlternate = isAlternate;
        }
    }

    [RelayCommand(CanExecute = nameof(CanGoToPreviousPage))]
    private async Task PreviousPageAsync()
    {
        CurrentPage--;
        await FetchSalesAsync();
    }

    [RelayCommand(CanExecute = nameof(CanGoToNextPage))]
    private async Task NextPageAsync()
    {
        CurrentPage++;
        await FetchSalesAsync();
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        IsRefreshing = true;
        try
        {
            await Task.WhenAll(
                LoadDashboardStatsAsync(),
                FetchSalesAsync()
            );
        }
        finally
        {
            IsRefreshing = false;
        }
    }
}
