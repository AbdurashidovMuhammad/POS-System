using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WPF.Models;
using WPF.Services;

namespace WPF.ViewModels;

public partial class CategoryViewModel : ViewModelBase
{
    private readonly IApiService _apiService;
    private readonly INavigationService _navigationService;

    public CategoryViewModel(IApiService apiService, INavigationService navigationService)
    {
        _apiService = apiService;
        _navigationService = navigationService;
    }

    // Collections
    [ObservableProperty]
    private ObservableCollection<CategoryDto> _categories = [];

    // Selected item
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(EditCategoryCommand))]
    [NotifyCanExecuteChangedFor(nameof(DeleteCategoryCommand))]
    [NotifyCanExecuteChangedFor(nameof(ToggleActiveCommand))]
    private CategoryDto? _selectedCategory;

    // Category products
    [ObservableProperty]
    private ObservableCollection<ProductDto> _categoryProducts = [];

    [ObservableProperty]
    private bool _isLoadingProducts;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsPanelOpen))]
    private bool _isProductsPanelOpen;

    // Search
    [ObservableProperty]
    private string _searchText = string.Empty;

    // Suggestions
    [ObservableProperty]
    private ObservableCollection<CategoryDto> _suggestions = [];

    [ObservableProperty]
    private bool _isSuggestionsOpen;

    partial void OnSelectedCategoryChanged(CategoryDto? value)
    {
        if (value is not null && !IsAddPanelOpen && !IsEditPanelOpen)
        {
            _ = LoadCategoryProductsAsync(value.Id);
        }
        else
        {
            IsProductsPanelOpen = false;
            CategoryProducts.Clear();
        }
    }

    partial void OnSearchTextChanged(string value)
    {
        _ = LoadSuggestionsAsync(value);
    }

    // Panel visibility
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsPanelOpen))]
    private bool _isAddPanelOpen;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsPanelOpen))]
    private bool _isEditPanelOpen;

    public bool IsPanelOpen => IsAddPanelOpen || IsEditPanelOpen || IsProductsPanelOpen;

    // Form fields
    [ObservableProperty]
    private string _formName = string.Empty;

    [ObservableProperty]
    private bool _formIsActive = true;

    [ObservableProperty]
    private string? _formError;

    [ObservableProperty]
    private bool _isSaving;

    [ObservableProperty]
    private string? _successMessage;

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

    // CanExecute methods
    private bool HasSelectedCategory() => SelectedCategory is not null;
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

    // Load categories
    [RelayCommand]
    private async Task LoadCategoriesAsync()
    {
        IsLoading = true;
        ClearError();
        SuccessMessage = null;

        try
        {
            var url = $"api/category?page={CurrentPage}&pageSize={PageSize}";
            var result = await _apiService.GetAsync<PagedResult<CategoryDto>>(url);

            if (result?.Succeeded == true && result.Result is not null)
            {
                Categories = new ObservableCollection<CategoryDto>(result.Result.Items);
                TotalPages = result.Result.TotalPages == 0 ? 1 : result.Result.TotalPages;
                TotalCount = result.Result.TotalCount;
            }
            else
            {
                ErrorMessage = result?.Errors.FirstOrDefault() ?? "Kategoriyalarni yuklashda xatolik";
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

    // Load suggestions for autocomplete
    private async Task LoadSuggestionsAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            Suggestions.Clear();
            IsSuggestionsOpen = false;
            return;
        }

        try
        {
            var url = $"api/category/suggest?query={Uri.EscapeDataString(query)}";
            var result = await _apiService.GetAsync<List<CategoryDto>>(url);

            if (result?.Succeeded == true && result.Result is not null)
            {
                Suggestions = new ObservableCollection<CategoryDto>(result.Result);
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
    private async Task SelectSuggestionAsync(CategoryDto suggestion)
    {
        if (suggestion is null) return;

        SearchText = suggestion.Name;
        IsSuggestionsOpen = false;
        await SearchCategoriesAsync();
    }

    // Close suggestions
    [RelayCommand]
    private void CloseSuggestions()
    {
        IsSuggestionsOpen = false;
    }

    // Search
    [RelayCommand]
    private async Task SearchCategoriesAsync()
    {
        IsSuggestionsOpen = false;
        CurrentPage = 1;

        if (string.IsNullOrWhiteSpace(SearchText))
        {
            await LoadCategoriesAsync();
            return;
        }

        IsLoading = true;
        ClearError();

        try
        {
            var url = $"api/category/suggest?query={Uri.EscapeDataString(SearchText)}";
            var result = await _apiService.GetAsync<List<CategoryDto>>(url);

            if (result?.Succeeded == true && result.Result is not null)
            {
                Categories = new ObservableCollection<CategoryDto>(result.Result);
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
    private void ShowAddPanel()
    {
        ClearForm();
        CloseAllPanels();
        FormIsActive = true;
        IsAddPanelOpen = true;
    }

    // Edit Category
    [RelayCommand(CanExecute = nameof(HasSelectedCategory))]
    private void EditCategory()
    {
        if (SelectedCategory is null) return;

        CloseAllPanels();

        FormName = SelectedCategory.Name;
        FormIsActive = SelectedCategory.IsActive;
        FormError = null;

        IsEditPanelOpen = true;
    }

    // Save Category (Create)
    [RelayCommand]
    private async Task SaveCategoryAsync()
    {
        if (!ValidateForm()) return;

        IsSaving = true;
        FormError = null;

        try
        {
            var dto = new CreateCategoryDto
            {
                Name = FormName.Trim()
            };

            var result = await _apiService.PostAsync<string>("api/category/create", dto);

            if (result?.Succeeded == true)
            {
                CloseAllPanels();
                SuccessMessage = "Kategoriya muvaffaqiyatli qo'shildi";
                await LoadCategoriesAsync();
            }
            else
            {
                FormError = result?.Errors.FirstOrDefault() ?? "Kategoriyani saqlashda xatolik";
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

    // Update Category
    [RelayCommand]
    private async Task UpdateCategoryAsync()
    {
        if (SelectedCategory is null || !ValidateForm()) return;

        IsSaving = true;
        FormError = null;

        try
        {
            var dto = new UpdateCategoryDto
            {
                Name = FormName.Trim(),
                IsActive = FormIsActive
            };

            var result = await _apiService.PutAsync<CategoryDto>($"api/category/{SelectedCategory.Id}", dto);

            if (result?.Succeeded == true)
            {
                CloseAllPanels();
                SuccessMessage = "Kategoriya muvaffaqiyatli yangilandi";
                await LoadCategoriesAsync();
            }
            else
            {
                FormError = result?.Errors.FirstOrDefault() ?? "Kategoriyani yangilashda xatolik";
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

    // Toggle Active Status
    [RelayCommand(CanExecute = nameof(HasSelectedCategory))]
    private async Task ToggleActiveAsync()
    {
        if (SelectedCategory is null) return;

        IsSaving = true;
        ClearError();

        try
        {
            var dto = new UpdateCategoryDto
            {
                IsActive = !SelectedCategory.IsActive
            };

            var result = await _apiService.PutAsync<CategoryDto>($"api/category/{SelectedCategory.Id}", dto);

            if (result?.Succeeded == true)
            {
                var status = !SelectedCategory.IsActive ? "faollashtirildi" : "nofaollashtirildi";
                SuccessMessage = $"Kategoriya {status}";
                await LoadCategoriesAsync();
            }
            else
            {
                ErrorMessage = result?.Errors.FirstOrDefault() ?? "Kategoriya holatini o'zgartirishda xatolik";
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

    // Delete Category
    [RelayCommand(CanExecute = nameof(HasSelectedCategory))]
    private async Task DeleteCategoryAsync()
    {
        if (SelectedCategory is null) return;

        IsSaving = true;
        ClearError();

        try
        {
            var result = await _apiService.DeleteAsync<bool>($"api/category/{SelectedCategory.Id}");

            if (result?.Succeeded == true)
            {
                SuccessMessage = "Kategoriya muvaffaqiyatli o'chirildi";
                SelectedCategory = null;
                await LoadCategoriesAsync();
            }
            else
            {
                ErrorMessage = result?.Errors.FirstOrDefault() ?? "Kategoriyani o'chirishda xatolik";
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

    // Pagination
    [RelayCommand(CanExecute = nameof(CanGoToPreviousPage))]
    private async Task PreviousPageAsync()
    {
        CurrentPage--;
        await LoadCategoriesAsync();
    }

    [RelayCommand(CanExecute = nameof(CanGoToNextPage))]
    private async Task NextPageAsync()
    {
        CurrentPage++;
        await LoadCategoriesAsync();
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

    // Close products panel
    [RelayCommand]
    private void CloseProductsPanel()
    {
        IsProductsPanelOpen = false;
        CategoryProducts.Clear();
        SelectedCategory = null;
    }

    // Load products for selected category
    private async Task LoadCategoryProductsAsync(int categoryId)
    {
        IsLoadingProducts = true;
        IsProductsPanelOpen = true;

        try
        {
            var result = await _apiService.GetAsync<List<ProductDto>>($"api/products/by-category/{categoryId}");

            if (result?.Succeeded == true && result.Result is not null)
            {
                CategoryProducts = new ObservableCollection<ProductDto>(result.Result);
            }
            else
            {
                CategoryProducts.Clear();
            }
        }
        catch
        {
            CategoryProducts.Clear();
        }
        finally
        {
            IsLoadingProducts = false;
        }
    }

    // Helper methods
    private void CloseAllPanels()
    {
        IsAddPanelOpen = false;
        IsEditPanelOpen = false;
        IsProductsPanelOpen = false;
        CategoryProducts.Clear();
    }

    private void ClearForm()
    {
        FormName = string.Empty;
        FormIsActive = true;
        FormError = null;
    }

    private bool ValidateForm()
    {
        if (string.IsNullOrWhiteSpace(FormName))
        {
            FormError = "Kategoriya nomini kiriting";
            return false;
        }

        if (FormName.Trim().Length < 2)
        {
            FormError = "Kategoriya nomi kamida 2 ta belgidan iborat bo'lishi kerak";
            return false;
        }

        FormError = null;
        return true;
    }
}
