using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using WPF.Models;
using WPF.Services;

namespace WPF.ViewModels;

public partial class ReportViewModel : ViewModelBase
{
    private readonly IApiService _apiService;
    private readonly INavigationService _navigationService;

    public ReportViewModel(IApiService apiService, INavigationService navigationService)
    {
        _apiService = apiService;
        _navigationService = navigationService;
        StartDate = DateTime.Today.AddDays(-30);
        EndDate = DateTime.Today;
        _ = LoadUsersAsync();
    }

    // Date filters
    [ObservableProperty]
    private DateTime _startDate;

    [ObservableProperty]
    private DateTime _endDate;

    // Tab selection: 0 = Sales, 1 = StockIn
    [ObservableProperty]
    private int _selectedTabIndex;

    // User filter
    [ObservableProperty]
    private ObservableCollection<UserFilterItem> _userFilterItems = new();

    [ObservableProperty]
    private UserFilterItem? _selectedUserFilter;

    // Sales report data
    [ObservableProperty]
    private ObservableCollection<SalesReportItemDto> _salesItems = new();

    [ObservableProperty]
    private decimal _totalSalesAmount;

    [ObservableProperty]
    private int _salesItemCount;

    // Stock-in report data
    [ObservableProperty]
    private ObservableCollection<StockInReportItemDto> _stockInItems = new();

    [ObservableProperty]
    private decimal _totalStockInQuantity;

    [ObservableProperty]
    private int _stockInItemCount;

    // Export loading state
    [ObservableProperty]
    private bool _isExporting;

    // Pagination
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(PreviousPageCommand))]
    [NotifyCanExecuteChangedFor(nameof(NextPageCommand))]
    private int _currentPage = 1;

    [ObservableProperty]
    private int _pageSize = 10;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(PreviousPageCommand))]
    [NotifyCanExecuteChangedFor(nameof(NextPageCommand))]
    private int _totalPages = 1;

    [ObservableProperty]
    private int _totalCount;

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

    private async Task LoadUsersAsync()
    {
        try
        {
            var result = await _apiService.GetAsync<List<UserDto>>($"api/user/all-list");
            if (result?.Succeeded == true && result.Result is not null)
            {
                var items = new ObservableCollection<UserFilterItem>
                {
                    new() { Id = null, DisplayName = "Barchasi" }
                };
                foreach (var user in result.Result)
                {
                    items.Add(new UserFilterItem
                    {
                        Id = user.Id,
                        DisplayName = $"{user.Username} ({user.Role})"
                    });
                }
                UserFilterItems = items;
                SelectedUserFilter = items[0];
            }
        }
        catch { }
    }

    private static string FormatDateParam(DateTime date) => date.ToString("yyyy-MM-dd");

    private static void AssignDateGroupColors<T>(IList<T> items, Func<T, DateTime> dateSelector, Action<T, bool> setAlternate)
    {
        DateTime? currentDate = null;
        bool isAlternate = false;

        foreach (var item in items)
        {
            var date = dateSelector(item).Date;
            if (currentDate != date)
            {
                if (currentDate != null)
                    isAlternate = !isAlternate;
                currentDate = date;
            }
            setAlternate(item, isAlternate);
        }
    }

    [RelayCommand]
    private async Task LoadReportAsync()
    {
        CurrentPage = 1;
        await FetchReportAsync();
    }

    private async Task FetchReportAsync()
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
            var from = FormatDateParam(StartDate);
            var to = FormatDateParam(EndDate);
            var userIdParam = SelectedUserFilter?.Id is not null ? $"&userId={SelectedUserFilter.Id}" : "";

            if (SelectedTabIndex == 0)
            {
                var result = await _apiService.GetAsync<SalesReportDto>($"api/reports/sales?from={from}&to={to}&page={CurrentPage}&pageSize={PageSize}{userIdParam}");

                if (result?.Succeeded == true && result.Result is not null)
                {
                    AssignDateGroupColors(result.Result.Items, x => x.Date, (x, alt) => x.DateGroupIsAlternate = alt);
                    SalesItems = new ObservableCollection<SalesReportItemDto>(result.Result.Items);
                    TotalSalesAmount = result.Result.TotalAmount;
                    SalesItemCount = result.Result.TotalCount;
                    TotalPages = result.Result.TotalPages == 0 ? 1 : result.Result.TotalPages;
                    TotalCount = result.Result.TotalCount;
                }
                else
                {
                    ErrorMessage = result?.Errors?.FirstOrDefault() ?? "Hisobot olishda xatolik";
                }
            }
            else
            {
                var result = await _apiService.GetAsync<StockInReportDto>($"api/reports/stock-in?from={from}&to={to}&page={CurrentPage}&pageSize={PageSize}{userIdParam}");

                if (result?.Succeeded == true && result.Result is not null)
                {
                    AssignDateGroupColors(result.Result.Items, x => x.Date, (x, alt) => x.DateGroupIsAlternate = alt);
                    StockInItems = new ObservableCollection<StockInReportItemDto>(result.Result.Items);
                    TotalStockInQuantity = result.Result.TotalQuantity;
                    StockInItemCount = result.Result.TotalCount;
                    TotalPages = result.Result.TotalPages == 0 ? 1 : result.Result.TotalPages;
                    TotalCount = result.Result.TotalCount;
                }
                else
                {
                    ErrorMessage = result?.Errors?.FirstOrDefault() ?? "Hisobot olishda xatolik";
                }
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

    // Pagination
    [RelayCommand(CanExecute = nameof(CanGoToPreviousPage))]
    private async Task PreviousPageAsync()
    {
        CurrentPage--;
        await FetchReportAsync();
    }

    [RelayCommand(CanExecute = nameof(CanGoToNextPage))]
    private async Task NextPageAsync()
    {
        CurrentPage++;
        await FetchReportAsync();
    }

    [RelayCommand]
    private async Task ExportToExcelAsync()
    {
        if (StartDate.Date > EndDate.Date)
        {
            ErrorMessage = "Boshlanish sanasi tugash sanasidan katta bo'lishi mumkin emas";
            return;
        }

        IsExporting = true;
        ClearMessages();

        try
        {
            var from = FormatDateParam(StartDate);
            var to = FormatDateParam(EndDate);
            var userIdParam = SelectedUserFilter?.Id is not null ? $"&userId={SelectedUserFilter.Id}" : "";

            string endpoint;
            string defaultFileName;

            if (SelectedTabIndex == 0)
            {
                endpoint = $"api/reports/sales/export?from={from}&to={to}{userIdParam}";
                defaultFileName = $"Sotilgan_mahsulotlar_{StartDate:dd.MM.yyyy}-{EndDate:dd.MM.yyyy}.xlsx";
            }
            else
            {
                endpoint = $"api/reports/stock-in/export?from={from}&to={to}{userIdParam}";
                defaultFileName = $"Kirim_mahsulotlar_{StartDate:dd.MM.yyyy}-{EndDate:dd.MM.yyyy}.xlsx";
            }

            var fileBytes = await _apiService.GetBytesAsync(endpoint);

            if (fileBytes is null || fileBytes.Length == 0)
            {
                ErrorMessage = "Excel faylni yuklab olishda xatolik yuz berdi";
                return;
            }

            var saveDialog = new SaveFileDialog
            {
                FileName = defaultFileName,
                Filter = "Excel fayl (*.xlsx)|*.xlsx",
                DefaultExt = ".xlsx"
            };

            if (saveDialog.ShowDialog() == true)
            {
                await File.WriteAllBytesAsync(saveDialog.FileName, fileBytes);
                SuccessMessage = $"Fayl saqlandi: {saveDialog.FileName}";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Export xatolik: {ex.Message}";
        }
        finally
        {
            IsExporting = false;
        }
    }

    [RelayCommand]
    private void GoBack()
    {
        _navigationService.GoBack();
    }
}
