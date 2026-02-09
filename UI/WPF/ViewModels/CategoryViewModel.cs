using System.Collections.ObjectModel;
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

    // Search
    [ObservableProperty]
    private string _searchText = string.Empty;

    // Suggestions
    [ObservableProperty]
    private ObservableCollection<CategoryDto> _suggestions = [];

    [ObservableProperty]
    private bool _isSuggestionsOpen;

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

    public bool IsPanelOpen => IsAddPanelOpen || IsEditPanelOpen;

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

    // CanExecute methods
    private bool HasSelectedCategory() => SelectedCategory is not null;

    // Load categories
    [RelayCommand]
    private async Task LoadCategoriesAsync()
    {
        IsLoading = true;
        ClearError();
        SuccessMessage = null;

        try
        {
            var result = await _apiService.GetAsync<List<CategoryDto>>("api/category");

            if (result?.Succeeded == true && result.Result is not null)
            {
                Categories = new ObservableCollection<CategoryDto>(result.Result);
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
