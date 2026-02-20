using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WPF.Enums;
using WPF.Models;
using WPF.Services;

namespace WPF.ViewModels;

public partial class ProductViewModel : ViewModelBase
{
    private readonly IApiService _apiService;
    private readonly INavigationService _navigationService;
    private readonly IAuthService _authService;
    private CancellationTokenSource? _suggestionsCts;

    public ProductViewModel(IApiService apiService, INavigationService navigationService, IAuthService authService)
    {
        _apiService = apiService;
        _navigationService = navigationService;
        _authService = authService;

        UnitTypes = new ObservableCollection<UnitType>(Enum.GetValues<UnitType>());
    }

    // Collections
    [ObservableProperty]
    private ObservableCollection<ProductDto> _products = [];

    [ObservableProperty]
    private ObservableCollection<CategoryDto> _categories = [];

    [ObservableProperty]
    private ObservableCollection<UnitType> _unitTypes;

    // Selected item
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(EditProductCommand))]
    [NotifyCanExecuteChangedFor(nameof(DeleteProductCommand))]
    [NotifyCanExecuteChangedFor(nameof(ShowStockInPanelCommand))]
    [NotifyCanExecuteChangedFor(nameof(ShowBatchesCommand))]
    private ProductDto? _selectedProduct;

    // Search
    [ObservableProperty]
    private string _searchText = string.Empty;

    // Suggestions
    [ObservableProperty]
    private ObservableCollection<ProductSuggestDto> _suggestions = [];

    [ObservableProperty]
    private bool _isSuggestionsOpen;

    partial void OnSearchTextChanged(string value)
    {
        _suggestionsCts?.Cancel();

        if (string.IsNullOrWhiteSpace(value) || value.Trim().Length < 3)
        {
            Suggestions.Clear();
            IsSuggestionsOpen = false;
            return;
        }

        _suggestionsCts = new CancellationTokenSource();
        var token = _suggestionsCts.Token;
        _ = DebounceSuggestionsAsync(value, token);
    }

    private async Task DebounceSuggestionsAsync(string query, CancellationToken token)
    {
        try
        {
            await Task.Delay(300, token);
            if (token.IsCancellationRequested) return;
            await LoadSuggestionsAsync(query);
        }
        catch (TaskCanceledException) { }
    }

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

    // Panel visibility
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsPanelOpen))]
    private bool _isAddPanelOpen;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsPanelOpen))]
    private bool _isEditPanelOpen;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsPanelOpen))]
    private bool _isStockInPanelOpen;

    public bool IsPanelOpen => IsAddPanelOpen || IsEditPanelOpen || IsStockInPanelOpen;

    // SuperAdmin check
    public bool IsSuperAdmin =>
        string.Equals(_authService.Role, "SuperAdmin", StringComparison.OrdinalIgnoreCase);

    // Batch dialog
    [ObservableProperty]
    private bool _isBatchDialogOpen;

    [ObservableProperty]
    private string _batchDialogProductName = string.Empty;

    [ObservableProperty]
    private ObservableCollection<ProductBatchDto> _batches = [];

    [ObservableProperty]
    private bool _isBatchLoading;

    // Barcode image dialog
    [ObservableProperty]
    private bool _isBarcodeDialogOpen;

    [ObservableProperty]
    private BitmapImage? _barcodeImage;

    [ObservableProperty]
    private string? _barcodeDialogProductName;

    private byte[]? _barcodeImageBytes;

    // Form fields
    [ObservableProperty]
    private string _formName = string.Empty;

    [ObservableProperty]
    private int? _formCategoryId;

    [ObservableProperty]
    private decimal _formSellPrice;

    [ObservableProperty]
    private decimal _formBuyPrice;

    [ObservableProperty]
    private decimal _stockInBuyPrice;

    [ObservableProperty]
    private UnitType _formUnitType = UnitType.Dona;

    [ObservableProperty]
    private decimal _formStockQuantity;

    // Gramm uchun kg + gramm split input
    [ObservableProperty]
    private int _formStockKg = 1;

    [ObservableProperty]
    private int _formStockGramm;

    [ObservableProperty]
    private int _stockInKg;

    [ObservableProperty]
    private int _stockInGramm;

    public bool IsGrammSelected => FormUnitType == UnitType.Gramm;
    public bool IsStockInGramm => SelectedProduct?.UnitType == UnitType.Gramm;

    [ObservableProperty]
    private decimal _formMinThreshold;

    // Gramm uchun min threshold kg + gramm split
    [ObservableProperty]
    private int _formMinThresholdKg;

    [ObservableProperty]
    private int _formMinThresholdGramm;

    [ObservableProperty]
    private string _formBarcode = string.Empty;

    [ObservableProperty]
    private decimal _stockInQuantity;

    [ObservableProperty]
    private string? _formError;

    [ObservableProperty]
    private bool _isSaving;

    [ObservableProperty]
    private string? _successMessage;

    partial void OnFormUnitTypeChanged(UnitType value)
    {
        OnPropertyChanged(nameof(IsGrammSelected));
        if (value == UnitType.Gramm)
        {
            FormStockKg = 1;
            FormStockGramm = 0;
        }
    }

    partial void OnSelectedProductChanged(ProductDto? value)
    {
        OnPropertyChanged(nameof(IsStockInGramm));
        if (value?.UnitType == UnitType.Gramm)
        {
            StockInKg = 0;
            StockInGramm = 0;
        }
    }

    // CanExecute methods
    private bool CanGoToPreviousPage() => CurrentPage > 1 && !IsLoading;
    private bool CanGoToNextPage() => CurrentPage < TotalPages && !IsLoading;
    private bool HasSelectedProduct() => SelectedProduct is not null;

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        if (e.PropertyName == nameof(IsLoading))
        {
            PreviousPageCommand.NotifyCanExecuteChanged();
            NextPageCommand.NotifyCanExecuteChanged();
        }
    }

    // Load products
    [RelayCommand]
    private async Task LoadProductsAsync()
    {
        IsLoading = true;
        ClearError();
        SuccessMessage = null;

        try
        {
            var url = $"api/products?page={CurrentPage}&pageSize={PageSize}";
            var result = await _apiService.GetAsync<PagedResult<ProductDto>>(url);

            if (result?.Succeeded == true && result.Result is not null)
            {
                Products = new ObservableCollection<ProductDto>(result.Result.Items);
                TotalPages = result.Result.TotalPages;
                TotalCount = result.Result.TotalCount;
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

    // Load categories
    [RelayCommand]
    private async Task LoadCategoriesAsync()
    {
        try
        {
            var result = await _apiService.GetAsync<List<CategoryDto>>("api/category/list");
            if (result?.Succeeded == true && result.Result is not null)
            {
                Categories = new ObservableCollection<CategoryDto>(result.Result.Where(c => c.IsActive));
            }
        }
        catch
        {
            // Silently fail for categories
        }
    }

    // Pagination
    [RelayCommand(CanExecute = nameof(CanGoToPreviousPage))]
    private async Task PreviousPageAsync()
    {
        CurrentPage--;
        await LoadProductsAsync();
    }

    [RelayCommand(CanExecute = nameof(CanGoToNextPage))]
    private async Task NextPageAsync()
    {
        CurrentPage++;
        await LoadProductsAsync();
    }

    // Load suggestions for autocomplete
    private async Task LoadSuggestionsAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query) || query.Length < 1)
        {
            Suggestions.Clear();
            IsSuggestionsOpen = false;
            return;
        }

        try
        {
            var url = $"api/products/search?query={Uri.EscapeDataString(query)}";
            var result = await _apiService.GetAsync<List<ProductSuggestDto>>(url);

            if (result?.Succeeded == true && result.Result is not null)
            {
                Suggestions = new ObservableCollection<ProductSuggestDto>(result.Result);
                IsSuggestionsOpen = Suggestions.Count > 0;
            }
            else
            {
                Suggestions.Clear();
                IsSuggestionsOpen = false;
            }
        }
        catch
        {
            Suggestions.Clear();
            IsSuggestionsOpen = false;
        }
    }

    // Select suggestion
    [RelayCommand]
    private async Task SelectSuggestionAsync(ProductSuggestDto suggestion)
    {
        if (suggestion is null) return;

        SearchText = suggestion.Name;
        IsSuggestionsOpen = false;
        await SearchProductsAsync();
    }

    // Close suggestions
    [RelayCommand]
    private void CloseSuggestions()
    {
        IsSuggestionsOpen = false;
    }

    // Search
    [RelayCommand]
    private async Task SearchProductsAsync()
    {
        IsSuggestionsOpen = false;

        if (string.IsNullOrWhiteSpace(SearchText))
        {
            CurrentPage = 1;
            await LoadProductsAsync();
            return;
        }

        IsLoading = true;
        ClearError();

        try
        {
            var url = $"api/products/search/full?query={Uri.EscapeDataString(SearchText)}&page={CurrentPage}&pageSize={PageSize}";
            var result = await _apiService.GetAsync<PagedResult<ProductDto>>(url);
            if (result?.Succeeded == true && result.Result is not null)
            {
                Products = new ObservableCollection<ProductDto>(result.Result.Items);
                TotalPages = result.Result.TotalPages;
                TotalCount = result.Result.TotalCount;
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

    // Show Add Panel
    [RelayCommand]
    private async Task ShowAddPanelAsync()
    {
        await LoadCategoriesAsync();
        ClearForm();
        CloseAllPanels();
        IsAddPanelOpen = true;
    }

    // Edit Product
    [RelayCommand(CanExecute = nameof(HasSelectedProduct))]
    private async Task EditProductAsync()
    {
        if (SelectedProduct is null) return;

        await LoadCategoriesAsync();
        CloseAllPanels();

        FormName = SelectedProduct.Name;
        FormCategoryId = SelectedProduct.CategoryId;
        FormSellPrice = SelectedProduct.SellPrice;
        FormUnitType = SelectedProduct.UnitType;
        FormError = null;

        if (SelectedProduct.UnitType == UnitType.Gramm)
        {
            var grams = (int)SelectedProduct.MinStockThreshold;
            FormMinThresholdKg = grams / 1000;
            FormMinThresholdGramm = grams % 1000;
        }
        else
        {
            FormMinThreshold = SelectedProduct.MinStockThreshold;
        }

        IsEditPanelOpen = true;
    }

    // Show Stock In Panel
    [RelayCommand(CanExecute = nameof(HasSelectedProduct))]
    private async Task ShowStockInPanelAsync()
    {
        if (SelectedProduct is null) return;

        CloseAllPanels();
        StockInQuantity = 0;
        StockInBuyPrice = 0;
        StockInKg = 0;
        StockInGramm = 0;
        FormError = null;
        IsStockInPanelOpen = true;
        await Task.CompletedTask;
    }

    // Save Product (Create)
    [RelayCommand]
    private async Task SaveProductAsync()
    {
        if (!ValidateForm()) return;

        IsSaving = true;
        FormError = null;

        try
        {
            var stockQty = FormUnitType == UnitType.Gramm
                ? FormStockKg * 1000m + FormStockGramm
                : FormStockQuantity;

            var minThreshold = FormUnitType == UnitType.Gramm
                ? FormMinThresholdKg * 1000m + FormMinThresholdGramm
                : FormMinThreshold;

            var dto = new CreateProductDto
            {
                Name = FormName.Trim(),
                CategoryId = FormCategoryId!.Value,
                SellPrice = FormSellPrice,
                UnitType = FormUnitType,
                StockQuantity = stockQty,
                BuyPrice = stockQty > 0 ? FormBuyPrice : null,
                Barcode = string.IsNullOrWhiteSpace(FormBarcode) ? null : FormBarcode.Trim(),
                MinStockThreshold = minThreshold
            };

            var result = await _apiService.PostAsync<int>("api/products", dto);

            if (result?.Succeeded == true)
            {
                var productId = result.Result;
                var hasCustomBarcode = !string.IsNullOrWhiteSpace(FormBarcode);
                var productName = FormName.Trim();
                ClearForm();
                SuccessMessage = "Mahsulot muvaffaqiyatli qo'shildi";
                await LoadProductsAsync();
                if (!hasCustomBarcode)
                    await ShowBarcodeImageAsync(productId, productName);
            }
            else
            {
                FormError = result?.Errors.FirstOrDefault() ?? "Mahsulotni saqlashda xatolik";
            }
        }
        catch (Exception ex)
        {
            FormError = ex.Message;
        }
        finally
        {
            IsSaving = false;
        }
    }

    // Update Product
    [RelayCommand]
    private async Task UpdateProductAsync()
    {
        if (SelectedProduct is null || !ValidateForm(isUpdate: true)) return;

        IsSaving = true;
        FormError = null;

        try
        {
            var minThreshold = FormUnitType == UnitType.Gramm
                ? FormMinThresholdKg * 1000m + FormMinThresholdGramm
                : FormMinThreshold;

            var dto = new UpdateProductDto
            {
                Name = FormName.Trim(),
                CategoryId = FormCategoryId!.Value,
                SellPrice = FormSellPrice,
                UnitType = FormUnitType,
                MinStockThreshold = minThreshold
            };

            var result = await _apiService.PutAsync<ProductDto>($"api/products/{SelectedProduct.Id}", dto);

            if (result?.Succeeded == true)
            {
                CloseAllPanels();
                SuccessMessage = "Mahsulot muvaffaqiyatli yangilandi";
                await LoadProductsAsync();
            }
            else
            {
                FormError = result?.Errors.FirstOrDefault() ?? "Mahsulotni yangilashda xatolik";
            }
        }
        catch (Exception ex)
        {
            FormError = ex.Message;
        }
        finally
        {
            IsSaving = false;
        }
    }

    // Add Stock
    [RelayCommand]
    private async Task AddStockAsync()
    {
        if (SelectedProduct is null) return;

        var stockQty = SelectedProduct.UnitType == UnitType.Gramm
            ? StockInKg * 1000m + StockInGramm
            : StockInQuantity;

        if (stockQty <= 0)
        {
            FormError = "Miqdor 0 dan katta bo'lishi kerak";
            return;
        }

        if (StockInBuyPrice <= 0)
        {
            FormError = "Kelish narxini kiriting";
            return;
        }

        IsSaving = true;
        FormError = null;

        try
        {
            var dto = new AddStockDto
            {
                Quantity = stockQty,
                BuyPrice = StockInBuyPrice,
                UserId = _authService.UserId ?? 1
            };

            var result = await _apiService.PostAsync<ProductDto>($"api/products/{SelectedProduct.Id}/stock", dto);

            if (result?.Succeeded == true)
            {
                CloseAllPanels();
                SuccessMessage = $"{stockQty:N2} {SelectedProduct.UnitType} zaxiraga qo'shildi";
                await LoadProductsAsync();
            }
            else
            {
                FormError = result?.Errors.FirstOrDefault() ?? "Zaxirani qo'shishda xatolik";
            }
        }
        catch (Exception ex)
        {
            FormError = ex.Message;
        }
        finally
        {
            IsSaving = false;
        }
    }

    // Delete Product
    [RelayCommand(CanExecute = nameof(HasSelectedProduct))]
    private async Task DeleteProductAsync()
    {
        if (SelectedProduct is null) return;

        IsSaving = true;
        ClearError();

        try
        {
            var result = await _apiService.DeleteAsync<string>($"api/products/{SelectedProduct.Id}");

            if (result?.Succeeded == true)
            {
                SuccessMessage = "Mahsulot muvaffaqiyatli o'chirildi";
                SelectedProduct = null;
                await LoadProductsAsync();
            }
            else
            {
                ErrorMessage = result?.Errors.FirstOrDefault() ?? "Mahsulotni o'chirishda xatolik";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsSaving = false;
        }
    }

    // Show barcode image for selected product
    [RelayCommand(CanExecute = nameof(HasSelectedProduct))]
    private async Task ShowBarcodeAsync()
    {
        if (SelectedProduct is null) return;
        await ShowBarcodeImageAsync(SelectedProduct.Id, SelectedProduct.Name);
    }

    // Show batch details dialog (SuperAdmin only)
    [RelayCommand(CanExecute = nameof(HasSelectedProduct))]
    private async Task ShowBatchesAsync()
    {
        if (SelectedProduct is null) return;

        IsBatchLoading = true;
        BatchDialogProductName = SelectedProduct.Name;
        Batches.Clear();
        IsBatchDialogOpen = true;

        try
        {
            var result = await _apiService.GetAsync<List<ProductBatchDto>>(
                $"api/products/{SelectedProduct.Id}/batches");

            if (result?.Succeeded == true && result.Result is not null)
            {
                Batches = new ObservableCollection<ProductBatchDto>(result.Result);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsBatchLoading = false;
        }
    }

    // Close batch dialog
    [RelayCommand]
    private void CloseBatchDialog()
    {
        IsBatchDialogOpen = false;
        Batches.Clear();
    }

    // Close barcode dialog
    [RelayCommand]
    private void CloseBarcodeDialog()
    {
        IsBarcodeDialogOpen = false;
        BarcodeImage = null;
        BarcodeDialogProductName = null;
        _barcodeImageBytes = null;
    }

    // Print barcode image
    [RelayCommand]
    private void PrintBarcodeImage()
    {
        if (_barcodeImageBytes is null) return;

        var printDialog = new PrintDialog();
        if (printDialog.ShowDialog() == true)
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.StreamSource = new MemoryStream(_barcodeImageBytes);
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();
            bitmap.Freeze();

            var visual = new DrawingVisual();
            using (var dc = visual.RenderOpen())
            {
                var pageWidth = printDialog.PrintableAreaWidth;
                var pageHeight = printDialog.PrintableAreaHeight;

                var imgWidth = bitmap.PixelWidth;
                var imgHeight = bitmap.PixelHeight;

                // Center the barcode on the page
                var x = (pageWidth - imgWidth) / 2;
                var y = (pageHeight - imgHeight) / 2;

                dc.DrawImage(bitmap, new Rect(x, y, imgWidth, imgHeight));
            }

            printDialog.PrintVisual(visual, $"Barcode - {BarcodeDialogProductName}");
        }
    }

    private async Task ShowBarcodeImageAsync(int productId, string productName)
    {
        var imageBytes = await _apiService.GetBytesAsync($"api/products/{productId}/barcode-image");
        if (imageBytes is not null)
        {
            _barcodeImageBytes = imageBytes;
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.StreamSource = new MemoryStream(imageBytes);
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();
            bitmap.Freeze();

            BarcodeImage = bitmap;
            BarcodeDialogProductName = productName;
            IsBarcodeDialogOpen = true;
        }
    }

    // Cancel / Close Panel
    [RelayCommand]
    private void CancelPanel()
    {
        CloseAllPanels();
        ClearForm();
    }

    // Navigation
    [RelayCommand]
    private void GoBack()
    {
        _navigationService.GoBack();
    }

    // Helper methods
    private void CloseAllPanels()
    {
        IsAddPanelOpen = false;
        IsEditPanelOpen = false;
        IsStockInPanelOpen = false;
    }

    private void ClearForm()
    {
        FormName = string.Empty;
        FormCategoryId = null;
        FormSellPrice = 0;
        FormBuyPrice = 0;
        FormUnitType = UnitType.Dona;
        FormStockQuantity = 0;
        FormStockKg = 1;
        FormStockGramm = 0;
        FormBarcode = string.Empty;
        FormMinThreshold = 0;
        FormMinThresholdKg = 0;
        FormMinThresholdGramm = 0;
        StockInQuantity = 0;
        StockInBuyPrice = 0;
        StockInKg = 0;
        StockInGramm = 0;
        FormError = null;
    }

    private bool ValidateForm(bool isUpdate = false)
    {
        if (string.IsNullOrWhiteSpace(FormName))
        {
            FormError = "Mahsulot nomini kiriting";
            return false;
        }

        if (FormName.Trim().Length < 2)
        {
            FormError = "Mahsulot nomi kamida 2 ta belgidan iborat bo'lishi kerak";
            return false;
        }

        if (FormCategoryId is null || FormCategoryId <= 0)
        {
            FormError = "Kategoriyani tanlang";
            return false;
        }

        if (FormSellPrice <= 0)
        {
            FormError = "Sotish narxi 0 dan katta bo'lishi kerak";
            return false;
        }

        if (!isUpdate)
        {
            if (FormUnitType == UnitType.Gramm)
            {
                if (FormStockKg < 0 || FormStockGramm < 0 || FormStockGramm >= 1000)
                {
                    FormError = "Gramm qiymati 0 dan 999 gacha bo'lishi kerak";
                    return false;
                }
                var grammQty = FormStockKg * 1000m + FormStockGramm;
                if (grammQty > 0 && FormBuyPrice <= 0)
                {
                    FormError = "Kelish narxini kiriting";
                    return false;
                }
            }
            else
            {
                if (FormStockQuantity < 0)
                {
                    FormError = "Zaxira miqdori manfiy bo'lishi mumkin emas";
                    return false;
                }
                if (FormStockQuantity > 0 && FormBuyPrice <= 0)
                {
                    FormError = "Kelish narxini kiriting";
                    return false;
                }
            }
        }

        FormError = null;
        return true;
    }
}
