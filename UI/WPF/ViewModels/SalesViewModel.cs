using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WPF.Enums;
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
        CartItems.CollectionChanged += CartItems_CollectionChanged;
    }

    [ObservableProperty]
    private ObservableCollection<CartItem> _cartItems = [];

    [ObservableProperty]
    private string _barcodeInput = string.Empty;

    [ObservableProperty]
    private decimal _totalAmount;

    [ObservableProperty]
    private decimal _totalQuantity;

    // --- Payment type selection ---
    [ObservableProperty]
    private int _selectedPaymentType = 1; // Default: Naqd

    [RelayCommand]
    private void SelectPaymentType(string paymentType)
    {
        if (int.TryParse(paymentType, out var value))
            SelectedPaymentType = value;
    }

    // --- Manual search dialog ---

    [ObservableProperty]
    private bool _isSearchDialogOpen;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private ObservableCollection<ProductDto> _searchResults = [];

    [ObservableProperty]
    private ProductDto? _selectedSearchProduct;

    [ObservableProperty]
    private string _manualQuantityText = "1";

    [ObservableProperty]
    private string? _searchError;

    /// <summary>
    /// Event raised when the search dialog closes so the view can refocus the barcode textbox.
    /// </summary>
    public event Action? SearchDialogClosed;

    private CancellationTokenSource? _searchCts;

    partial void OnSearchTextChanged(string value)
    {
        _searchCts?.Cancel();
        _searchCts = new CancellationTokenSource();
        var token = _searchCts.Token;

        _ = DebounceSearchAsync(value, token);
    }

    private async Task DebounceSearchAsync(string query, CancellationToken token)
    {
        try
        {
            await Task.Delay(300, token);
            if (token.IsCancellationRequested) return;
            await SearchProductsAsync();
        }
        catch (TaskCanceledException) { }
    }

    private void CartItems_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems is not null)
        {
            foreach (CartItem item in e.NewItems)
                item.PropertyChanged += CartItem_PropertyChanged;
        }
        if (e.OldItems is not null)
        {
            foreach (CartItem item in e.OldItems)
                item.PropertyChanged -= CartItem_PropertyChanged;
        }
        CalculateTotal();
    }

    private void CartItem_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(CartItem.Quantity))
            CalculateTotal();
    }

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
                var product = result.Result;
                var existingItem = CartItems.FirstOrDefault(x => x.Product.Id == product.Id);
                var currentQty = existingItem?.Quantity ?? 0;

                if (currentQty + 1 > product.StockQuantity)
                {
                    ErrorMessage = $"Omborda mahsulot qolmadi! (Mavjud: {product.StockQuantity})";
                }
                else
                {
                    if (existingItem is not null)
                    {
                        existingItem.Quantity++;
                    }
                    else
                    {
                        CartItems.Add(new CartItem { Product = product, Quantity = 1 });
                    }
                    CalculateTotal();
                }
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
    private void OpenSearchDialog()
    {
        SearchText = string.Empty;
        SearchResults.Clear();
        SelectedSearchProduct = null;
        ManualQuantityText = "1";
        SearchError = null;
        IsSearchDialogOpen = true;
    }

    [RelayCommand]
    private void CloseSearchDialog()
    {
        IsSearchDialogOpen = false;
        SearchDialogClosed?.Invoke();
    }

    [RelayCommand]
    private async Task SearchProductsAsync()
    {
        SearchError = null;
        SelectedSearchProduct = null;

        if (string.IsNullOrWhiteSpace(SearchText))
        {
            SearchResults.Clear();
            return;
        }

        try
        {
            var url = $"api/products/search/full?query={Uri.EscapeDataString(SearchText)}&page=1&pageSize=20";
            var result = await _apiService.GetAsync<PagedResult<ProductDto>>(url);

            if (result?.Succeeded == true && result.Result is not null)
            {
                SearchResults = new ObservableCollection<ProductDto>(result.Result.Items);
                if (SearchResults.Count == 0)
                    SearchError = "Mahsulot topilmadi";
            }
            else
            {
                SearchResults.Clear();
                SearchError = "Mahsulot topilmadi";
            }
        }
        catch
        {
            SearchResults.Clear();
            SearchError = "Qidiruvda xatolik yuz berdi";
        }
    }

    [RelayCommand]
    private void SelectSearchProduct(ProductDto product)
    {
        SelectedSearchProduct = product;
        // Set default quantity based on unit type
        ManualQuantityText = product.UnitType == UnitType.Dona
            || product.UnitType == UnitType.Quti
            || product.UnitType == UnitType.Paket
            || product.UnitType == UnitType.Shisha
            || product.UnitType == UnitType.Oram
            || product.UnitType == UnitType.Juft
            ? "1"
            : "";
        SearchError = null;
    }

    [RelayCommand]
    private void AddManualProduct()
    {
        if (SelectedSearchProduct is null) return;

        var qtyText = ManualQuantityText.Trim().Replace(',', '.');
        if (!decimal.TryParse(qtyText, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var qty) || qty <= 0)
        {
            SearchError = "Miqdorni to'g'ri kiriting";
            return;
        }

        var product = SelectedSearchProduct;
        var existingItem = CartItems.FirstOrDefault(x => x.Product.Id == product.Id);
        var currentQty = existingItem?.Quantity ?? 0;

        if (currentQty + qty > product.StockQuantity)
        {
            SearchError = $"Omborda yetarli mahsulot yo'q! (Mavjud: {product.StockQuantity})";
            return;
        }

        if (existingItem is not null)
        {
            existingItem.Quantity += qty;
        }
        else
        {
            CartItems.Add(new CartItem { Product = product, Quantity = qty });
        }
        CalculateTotal();

        // Close dialog and return focus to barcode
        IsSearchDialogOpen = false;
        SearchDialogClosed?.Invoke();
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
        TotalQuantity = 0;
        SelectedPaymentType = 1;
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
        ClearMessages();

        try
        {
            var createSaleDto = new CreateSaleDto
            {
                Items = CartItems.Select(x => new CreateSaleItemDto
                {
                    ProductId = x.Product.Id,
                    Quantity = x.Quantity
                }).ToList(),
                PaymentType = SelectedPaymentType
            };

            var result = await _apiService.PostAsync<SaleDto>("api/sales", createSaleDto);

            if (result?.Succeeded == true)
            {
                SuccessMessage = $"Sotuv muvaffaqiyatli yakunlandi! Chek #{result.Result?.Id}";
                ClearCart();
            }
            else
            {
                ErrorMessage = result?.Errors?.FirstOrDefault() ?? "Sotuvda xatolik yuz berdi";
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

    private void CalculateTotal()
    {
        TotalAmount = CartItems.Sum(x => x.Product.UnitPrice * x.Quantity);
        TotalQuantity = CartItems.Sum(x => x.Quantity);
    }
}

public partial class CartItem : ObservableObject
{
    [ObservableProperty]
    private ProductDto _product = null!;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Subtotal))]
    private decimal _quantity;

    public decimal Subtotal => Product.UnitPrice * Quantity;
}
