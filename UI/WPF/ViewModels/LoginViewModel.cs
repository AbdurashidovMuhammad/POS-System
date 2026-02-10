using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WPF.Services;

namespace WPF.ViewModels;

public partial class LoginViewModel : ViewModelBase
{
    private readonly IAuthService _authService;
    private readonly INavigationService _navigationService;

    public LoginViewModel(IAuthService authService, INavigationService navigationService)
    {
        _authService = authService;
        _navigationService = navigationService;
    }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
    private string _username = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
    private string _password = string.Empty;

    [ObservableProperty]
    private bool _isPasswordVisible;

    private bool CanLogin => !string.IsNullOrWhiteSpace(Username)
                           && !string.IsNullOrWhiteSpace(Password)
                           && !IsLoading;

    [RelayCommand]
    private void TogglePasswordVisibility()
    {
        IsPasswordVisible = !IsPasswordVisible;
    }

    [RelayCommand(CanExecute = nameof(CanLogin))]
    private async Task LoginAsync()
    {
        ClearError();
        IsLoading = true;

        try
        {
            var (success, errorMessage) = await _authService.LoginAsync(Username, Password);

            if (success)
            {
                _navigationService.NavigateTo<ShellViewModel>();
            }
            else
            {
                ErrorMessage = errorMessage ?? "Login muvaffaqiyatsiz";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Xatolik yuz berdi: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
}
