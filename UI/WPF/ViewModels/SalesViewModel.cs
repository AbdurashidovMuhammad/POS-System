using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WPF.Models;
using WPF.Services;

namespace WPF.ViewModels;

public partial class SalesViewModel : ViewModelBase
{
    private readonly IApiService _apiService;
    private readonly INavigationService _navigationService;

    public SalesViewModel(IApiService apiService, INavigationService navigationService)
    {
        _apiService = apiService;
        _navigationService = navigationService;
    }

    [ObservableProperty]
    private ObservableCollection<CartItem> _cartItems = [];

    [ObservableProperty]
    private string _barcodeInput = string.Empty;

    [ObservableProperty]
    private decimal _totalAmount;

    [RelayCommand]
    private async Task AddProductByBarcodeAsync()
    {
        if (string.IsNullOrWhiteSpace(BarcodeInput)) return;

        IsLoading = true;
        ClearError();

        try
        {
            var result = await _apiService.GetAsync<ProductDto>($"api/products/barcode/{BarcodeInput}");
            if (result?.Succeeded == true && result.Result is not null)
            {
                var existingItem = CartItems.FirstOrDefault(x => x.Product.Id == result.Result.Id);
                if (existingItem is not null)
                {
                    existingItem.Quantity++;
                }
                else
                {
                    CartItems.Add(new CartItem { Product = result.Result, Quantity = 1 });
                }
                CalculateTotal();
            }
            else
            {
                ErrorMessage = "Mahsulot topilmadi";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsLoading = false;
            BarcodeInput = string.Empty;
        }
    }

    [RelayCommand]
    private void RemoveItem(CartItem item)
    {
        CartItems.Remove(item);
        CalculateTotal();
    }

    [RelayCommand]
    private void ClearCart()
    {
        CartItems.Clear();
        TotalAmount = 0;
    }

    [RelayCommand]
    private async Task CompleteSaleAsync()
    {
        if (CartItems.Count == 0)
        {
            ErrorMessage = "Savat bo'sh";
            return;
        }

        IsLoading = true;
        ClearError();

        try
        {
            // TODO: Implement sale completion API call
            // var saleItems = CartItems.Select(x => new { ProductId = x.Product.Id, Quantity = x.Quantity });
            // await _apiService.PostAsync<object>("api/sales", new { Items = saleItems });

            ClearCart();
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

    private void CalculateTotal()
    {
        TotalAmount = CartItems.Sum(x => x.Product.UnitPrice * x.Quantity);
    }
}

public partial class CartItem : ObservableObject
{
    [ObservableProperty]
    private ProductDto _product = null!;

    [ObservableProperty]
    private decimal _quantity;

    public decimal Subtotal => Product.UnitPrice * Quantity;
}
