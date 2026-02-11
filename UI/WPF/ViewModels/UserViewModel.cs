using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WPF.Models;
using WPF.Services;

namespace WPF.ViewModels;

public partial class UserViewModel : ViewModelBase
{
    private readonly IApiService _apiService;

    public UserViewModel(IApiService apiService)
    {
        _apiService = apiService;
    }

    // Collections
    [ObservableProperty]
    private ObservableCollection<UserDto> _users = [];

    // Selected item
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(EditUserCommand))]
    [NotifyCanExecuteChangedFor(nameof(ToggleActiveCommand))]
    private UserDto? _selectedUser;

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
    private string _formUsername = string.Empty;

    [ObservableProperty]
    private string _formPassword = string.Empty;

    [ObservableProperty]
    private bool _formIsActive = true;

    [ObservableProperty]
    private string? _formError;

    [ObservableProperty]
    private bool _isSaving;

    // CanExecute
    private bool HasSelectedUser() => SelectedUser is not null;

    // Load users
    [RelayCommand]
    private async Task LoadUsersAsync()
    {
        IsLoading = true;
        ClearMessages();

        try
        {
            var result = await _apiService.GetAsync<List<UserDto>>("api/user");

            if (result?.Succeeded == true && result.Result is not null)
            {
                Users = new ObservableCollection<UserDto>(result.Result);
            }
            else
            {
                ErrorMessage = result?.Errors.FirstOrDefault() ?? "Foydalanuvchilarni yuklashda xatolik";
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

    // Edit User
    [RelayCommand(CanExecute = nameof(HasSelectedUser))]
    private void EditUser()
    {
        if (SelectedUser is null) return;

        CloseAllPanels();

        FormUsername = SelectedUser.Username;
        FormPassword = string.Empty;
        FormIsActive = SelectedUser.IsActive;
        FormError = null;

        IsEditPanelOpen = true;
    }

    // Save User (Create)
    [RelayCommand]
    private async Task SaveUserAsync()
    {
        if (!ValidateForm(isCreate: true)) return;

        IsSaving = true;
        FormError = null;

        try
        {
            var dto = new CreateUserDto
            {
                Username = FormUsername.Trim(),
                Password = FormPassword
            };

            var result = await _apiService.PostAsync<UserDto>("api/user/create", dto);

            if (result?.Succeeded == true)
            {
                CloseAllPanels();
                SuccessMessage = "Foydalanuvchi muvaffaqiyatli qo'shildi";
                await LoadUsersAsync();
            }
            else
            {
                FormError = result?.Errors.FirstOrDefault() ?? "Foydalanuvchini saqlashda xatolik";
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

    // Update User
    [RelayCommand]
    private async Task UpdateUserAsync()
    {
        if (SelectedUser is null || !ValidateForm(isCreate: false)) return;

        IsSaving = true;
        FormError = null;

        try
        {
            var dto = new UpdateUserDto
            {
                Username = FormUsername.Trim(),
                IsActive = FormIsActive
            };

            if (!string.IsNullOrWhiteSpace(FormPassword))
                dto.Password = FormPassword;

            var result = await _apiService.PutAsync<UserDto>($"api/user/{SelectedUser.Id}", dto);

            if (result?.Succeeded == true)
            {
                CloseAllPanels();
                SuccessMessage = "Foydalanuvchi muvaffaqiyatli yangilandi";
                await LoadUsersAsync();
            }
            else
            {
                FormError = result?.Errors.FirstOrDefault() ?? "Foydalanuvchini yangilashda xatolik";
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
    [RelayCommand(CanExecute = nameof(HasSelectedUser))]
    private async Task ToggleActiveAsync()
    {
        if (SelectedUser is null) return;

        IsSaving = true;
        ClearError();

        try
        {
            if (SelectedUser.IsActive)
            {
                // Deactivate
                var result = await _apiService.DeleteAsync<bool>($"api/user/{SelectedUser.Id}");

                if (result?.Succeeded == true)
                {
                    SuccessMessage = "Foydalanuvchi nofaollashtirildi";
                    await LoadUsersAsync();
                }
                else
                {
                    ErrorMessage = result?.Errors.FirstOrDefault() ?? "Holatni o'zgartirishda xatolik";
                }
            }
            else
            {
                // Activate
                var dto = new UpdateUserDto { IsActive = true };
                var result = await _apiService.PutAsync<UserDto>($"api/user/{SelectedUser.Id}", dto);

                if (result?.Succeeded == true)
                {
                    SuccessMessage = "Foydalanuvchi faollashtirildi";
                    await LoadUsersAsync();
                }
                else
                {
                    ErrorMessage = result?.Errors.FirstOrDefault() ?? "Holatni o'zgartirishda xatolik";
                }
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

    // Helpers
    private void CloseAllPanels()
    {
        IsAddPanelOpen = false;
        IsEditPanelOpen = false;
    }

    private void ClearForm()
    {
        FormUsername = string.Empty;
        FormPassword = string.Empty;
        FormIsActive = true;
        FormError = null;
    }

    private bool ValidateForm(bool isCreate)
    {
        if (string.IsNullOrWhiteSpace(FormUsername))
        {
            FormError = "Foydalanuvchi nomini kiriting";
            return false;
        }

        if (FormUsername.Trim().Length < 3)
        {
            FormError = "Foydalanuvchi nomi kamida 3 ta belgidan iborat bo'lishi kerak";
            return false;
        }

        if (isCreate && string.IsNullOrWhiteSpace(FormPassword))
        {
            FormError = "Parolni kiriting";
            return false;
        }

        if (!string.IsNullOrWhiteSpace(FormPassword) && FormPassword.Length < 6)
        {
            FormError = "Parol kamida 6 ta belgidan iborat bo'lishi kerak";
            return false;
        }

        FormError = null;
        return true;
    }
}
