using System.Collections.ObjectModel;
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
    }

    // Date filters
    [ObservableProperty]
    private DateTime _startDate;

    [ObservableProperty]
    private DateTime _endDate;

    // Tab selection: 0 = Sales, 1 = StockIn
    [ObservableProperty]
    private int _selectedTabIndex;

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

    private static string FormatDateParam(DateTime date) => date.ToString("yyyy-MM-dd");

    [RelayCommand]
    private async Task LoadReportAsync()
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

            if (SelectedTabIndex == 0)
            {
                var result = await _apiService.GetAsync<SalesReportDto>($"api/reports/sales?from={from}&to={to}");

                if (result?.Succeeded == true && result.Result is not null)
                {
                    SalesItems = new ObservableCollection<SalesReportItemDto>(result.Result.Items);
                    TotalSalesAmount = result.Result.TotalAmount;
                    SalesItemCount = result.Result.Items.Count;
                }
                else
                {
                    ErrorMessage = result?.Errors?.FirstOrDefault() ?? "Hisobot olishda xatolik";
                }
            }
            else
            {
                var result = await _apiService.GetAsync<StockInReportDto>($"api/reports/stock-in?from={from}&to={to}");

                if (result?.Succeeded == true && result.Result is not null)
                {
                    StockInItems = new ObservableCollection<StockInReportItemDto>(result.Result.Items);
                    TotalStockInQuantity = result.Result.TotalQuantity;
                    StockInItemCount = result.Result.Items.Count;
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

            string endpoint;
            string defaultFileName;

            if (SelectedTabIndex == 0)
            {
                endpoint = $"api/reports/sales/export?from={from}&to={to}";
                defaultFileName = $"Sotilgan_mahsulotlar_{StartDate:dd.MM.yyyy}-{EndDate:dd.MM.yyyy}.xlsx";
            }
            else
            {
                endpoint = $"api/reports/stock-in/export?from={from}&to={to}";
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
