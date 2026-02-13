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
        _ = LoadSuggestionsAsync(value);
    }

    // Pagination
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(PreviousPageCommand))]
    [NotifyCanExecuteChangedFor(nameof(NextPageCommand))]
    private int _currentPage = 1;

    [ObservableProperty]
    private int _pageSize = 20;

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
    private decimal _formUnitPrice;

    [ObservableProperty]
    private UnitType _formUnitType = UnitType.Dona;

    [ObservableProperty]
    private decimal _formStockQuantity;

    [ObservableProperty]
    private decimal _stockInQuantity;

    [ObservableProperty]
    private string? _formError;

    [ObservableProperty]
    private bool _isSaving;

    [ObservableProperty]
    private string? _successMessage;

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
            var result = await _apiService.GetAsync<List<CategoryDto>>("api/category");
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
        FormUnitPrice = SelectedProduct.UnitPrice;
        FormUnitType = SelectedProduct.UnitType;
        FormError = null;

        IsEditPanelOpen = true;
    }

    // Show Stock In Panel
    [RelayCommand(CanExecute = nameof(HasSelectedProduct))]
    private async Task ShowStockInPanelAsync()
    {
        if (SelectedProduct is null) return;

        CloseAllPanels();
        StockInQuantity = 0;
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
            var dto = new CreateProductDto
            {
                Name = FormName.Trim(),
                CategoryId = FormCategoryId!.Value,
                UnitPrice = FormUnitPrice,
                UnitType = FormUnitType,
                StockQuantity = FormStockQuantity
            };

            var result = await _apiService.PostAsync<int>("api/products", dto);

            if (result?.Succeeded == true)
            {
                var productId = result.Result;
                CloseAllPanels();
                SuccessMessage = "Mahsulot muvaffaqiyatli qo'shildi";
                await LoadProductsAsync();
                await ShowBarcodeImageAsync(productId, FormName.Trim());
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
            var dto = new UpdateProductDto
            {
                Name = FormName.Trim(),
                CategoryId = FormCategoryId!.Value,
                UnitPrice = FormUnitPrice,
                UnitType = FormUnitType
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

        if (StockInQuantity <= 0)
        {
            FormError = "Miqdor 0 dan katta bo'lishi kerak";
            return;
        }

        IsSaving = true;
        FormError = null;

        try
        {
            var dto = new AddStockDto
            {
                Quantity = StockInQuantity,
                UserId = _authService.UserId ?? 1
            };

            var result = await _apiService.PostAsync<ProductDto>($"api/products/{SelectedProduct.Id}/stock", dto);

            if (result?.Succeeded == true)
            {
                var productId = SelectedProduct.Id;
                var productName = SelectedProduct.Name;
                CloseAllPanels();
                SuccessMessage = $"{StockInQuantity:N2} {SelectedProduct.UnitType} zaxiraga qo'shildi";
                await LoadProductsAsync();
                await ShowBarcodeImageAsync(productId, productName);
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
        FormUnitPrice = 0;
        FormUnitType = UnitType.Dona;
        FormStockQuantity = 0;
        StockInQuantity = 0;
        FormError = null;
    }

    private bool ValidateForm(bool isUpdate = false)
    {
        if (string.IsNullOrWhiteSpace(FormName))
        {
            FormError = "Mahsulot nomini kiriting";
            return false;
        }

        if (FormCategoryId is null || FormCategoryId <= 0)
        {
            FormError = "Kategoriyani tanlang";
            return false;
        }

        if (FormUnitPrice <= 0)
        {
            FormError = "Narx 0 dan katta bo'lishi kerak";
            return false;
        }

        if (!isUpdate && FormStockQuantity < 0)
        {
            FormError = "Zaxira miqdori manfiy bo'lishi mumkin emas";
            return false;
        }

        FormError = null;
        return true;
    }
}
