using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WPF.Models;
using WPF.Services;

namespace WPF.ViewModels;

public partial class PermissionItemViewModel : ObservableObject
{
    public int Id { get; set; }
    public string DisplayName { get; set; } = string.Empty;

    [ObservableProperty]
    private bool _isChecked;
}

public partial class PermissionSectionViewModel : ObservableObject
{
    public string SectionDisplayName { get; set; } = string.Empty;
    public ObservableCollection<PermissionItemViewModel> Items { get; set; } = new();
}

public partial class UserPermissionViewModel : ObservableObject
{
    private readonly IApiService _apiService;
    private int _userId;

    public UserPermissionViewModel(IApiService apiService)
    {
        _apiService = apiService;
    }

    [ObservableProperty]
    private string _username = string.Empty;

    [ObservableProperty]
    private ObservableCollection<PermissionSectionViewModel> _sections = new();

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _isSaving;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private string? _successMessage;

    public event Action? CloseRequested;

    public async Task LoadAsync(int userId, string username)
    {
        _userId = userId;
        Username = username;
        IsLoading = true;
        ErrorMessage = null;

        try
        {
            // Load all permissions
            var allResult = await _apiService.GetAsync<List<PermissionGroupDto>>("api/permissions");
            // Load user's current permissions
            var userResult = await _apiService.GetAsync<UserPermissionsDto>($"api/permissions/user/{userId}");

            if (allResult?.Succeeded != true || allResult.Result is null)
            {
                ErrorMessage = "Ruxsatlarni yuklashda xatolik";
                return;
            }

            var userPermIds = userResult?.Result?.PermissionIds ?? new List<int>();

            Sections = new ObservableCollection<PermissionSectionViewModel>(
                allResult.Result.Select(g => new PermissionSectionViewModel
                {
                    SectionDisplayName = g.SectionDisplayName,
                    Items = new ObservableCollection<PermissionItemViewModel>(
                        g.Permissions.Select(p => new PermissionItemViewModel
                        {
                            Id = p.Id,
                            DisplayName = p.DisplayName,
                            IsChecked = userPermIds.Contains(p.Id)
                        }))
                }));
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
    private async Task SaveAsync()
    {
        IsSaving = true;
        ErrorMessage = null;
        SuccessMessage = null;

        try
        {
            var selectedIds = Sections
                .SelectMany(s => s.Items)
                .Where(i => i.IsChecked)
                .Select(i => i.Id)
                .ToList();

            var dto = new UpdateUserPermissionsDto { PermissionIds = selectedIds };
            var result = await _apiService.PutAsync<string>($"api/permissions/user/{_userId}", dto);

            if (result?.Succeeded == true)
            {
                SuccessMessage = "Ruxsatlar saqlandi";
                await Task.Delay(800);
                CloseRequested?.Invoke();
            }
            else
            {
                ErrorMessage = result?.Errors.FirstOrDefault() ?? "Saqlashda xatolik";
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

    [RelayCommand]
    private void Cancel()
    {
        CloseRequested?.Invoke();
    }
}
