using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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

    [ObservableProperty]
    private DateTime _startDate;

    [ObservableProperty]
    private DateTime _endDate;

    [ObservableProperty]
    private decimal _totalSales;

    [ObservableProperty]
    private int _totalTransactions;

    [RelayCommand]
    private async Task LoadReportAsync()
    {
        IsLoading = true;
        ClearError();

        try
        {
            // TODO: Implement report API call
            // var result = await _apiService.GetAsync<ReportDto>($"api/reports?startDate={StartDate:yyyy-MM-dd}&endDate={EndDate:yyyy-MM-dd}");
            // if (result?.Succeeded == true && result.Result is not null)
            // {
            //     TotalSales = result.Result.TotalSales;
            //     TotalTransactions = result.Result.TotalTransactions;
            // }
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
    private void GoBack()
    {
        _navigationService.GoBack();
    }
}
