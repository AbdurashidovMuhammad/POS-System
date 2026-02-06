using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WPF.Models;
using WPF.Services;

namespace WPF.ViewModels;

public partial class ProductViewModel : ViewModelBase
{
    private readonly IApiService _apiService;
    private readonly INavigationService _navigationService;

    public ProductViewModel(IApiService apiService, INavigationService navigationService)
    {
        _apiService = apiService;
        _navigationService = navigationService;
    }

    [ObservableProperty]
    private ObservableCollection<ProductDto> _products = [];

    [ObservableProperty]
    private ProductDto? _selectedProduct;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [RelayCommand]
    private async Task LoadProductsAsync()
    {
        IsLoading = true;
        ClearError();

        try
        {
            var result = await _apiService.GetAsync<List<ProductDto>>("api/products");
            if (result?.Succeeded == true && result.Result is not null)
            {
                Products = new ObservableCollection<ProductDto>(result.Result);
            }
            else
            {
                ErrorMessage = result?.Errors.FirstOrDefault() ?? "Mahsulotlarni yuklashda xatolik";
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
    private async Task SearchProductsAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchText))
        {
            await LoadProductsAsync();
            return;
        }

        IsLoading = true;
        ClearError();

        try
        {
            var result = await _apiService.GetAsync<List<ProductDto>>($"api/products?search={SearchText}");
            if (result?.Succeeded == true && result.Result is not null)
            {
                Products = new ObservableCollection<ProductDto>(result.Result);
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
    private void GoBack()
    {
        _navigationService.GoBack();
    }
}
