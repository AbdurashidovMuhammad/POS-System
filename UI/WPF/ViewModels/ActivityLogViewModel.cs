using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WPF.Models;
using WPF.Services;

namespace WPF.ViewModels;

public partial class ActivityLogViewModel : ViewModelBase
{
    private readonly IApiService _apiService;

    public ActivityLogViewModel(IApiService apiService)
    {
        _apiService = apiService;
        _startDate = DateTime.Today.AddDays(-7);
        _endDate = DateTime.Today;
    }

    [ObservableProperty]
    private ObservableCollection<AuditLogDto> _logs = new();

    [ObservableProperty]
    private DateTime _startDate;

    [ObservableProperty]
    private DateTime _endDate;

    [ObservableProperty]
    private UserDto? _selectedUser;

    [ObservableProperty]
    private ActionTypeOption? _selectedActionType;

    [ObservableProperty]
    private ObservableCollection<UserDto> _users = new();

    public ObservableCollection<ActionTypeOption> ActionTypes { get; } = new()
    {
        new("Barchasi", null),
        new("Sotish", 1),
        new("Zaxiraga qo'shish", 2),
        new("Mahsulot yaratish", 10),
        new("Mahsulot yangilash", 11),
        new("Mahsulot o'chirish", 12),
        new("Kategoriya yaratish", 20),
        new("Kategoriya yangilash", 21),
        new("Kategoriya o'chirish", 22),
        new("Foydalanuvchi yaratish", 30),
        new("Foydalanuvchi yangilash", 31),
        new("Foydalanuvchi o'chirish", 32),
    };

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(PreviousPageCommand))]
    [NotifyCanExecuteChangedFor(nameof(NextPageCommand))]
    private int _currentPage = 1;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(PreviousPageCommand))]
    [NotifyCanExecuteChangedFor(nameof(NextPageCommand))]
    private int _totalPages = 1;

    [ObservableProperty]
    private int _totalCount;

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

    [RelayCommand]
    private async Task LoadLogsAsync()
    {
        IsLoading = true;
        ClearMessages();

        try
        {
            var from = StartDate.ToString("yyyy-MM-dd");
            var to = EndDate.ToString("yyyy-MM-dd");

            var url = $"api/audit-logs?page={CurrentPage}&pageSize=20&from={from}&to={to}";

            if (SelectedUser is not null)
                url += $"&userId={SelectedUser.Id}";

            if (SelectedActionType?.Value is not null)
                url += $"&actionType={SelectedActionType.Value}";

            var result = await _apiService.GetAsync<PagedResult<AuditLogDto>>(url);

            if (result?.Succeeded == true && result.Result is not null)
            {
                Logs = new ObservableCollection<AuditLogDto>(result.Result.Items);
                TotalPages = result.Result.TotalPages == 0 ? 1 : result.Result.TotalPages;
                TotalCount = result.Result.TotalCount;
            }
            else
            {
                ErrorMessage = result?.Errors?.FirstOrDefault() ?? "Jurnalni yuklashda xatolik";
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
    private async Task LoadUsersAsync()
    {
        try
        {
            var result = await _apiService.GetAsync<List<UserDto>>("api/user");
            if (result?.Succeeded == true && result.Result is not null)
            {
                Users = new ObservableCollection<UserDto>(result.Result);
            }
        }
        catch { }
    }

    [RelayCommand(CanExecute = nameof(CanGoToPreviousPage))]
    private async Task PreviousPageAsync()
    {
        CurrentPage--;
        await LoadLogsAsync();
    }

    [RelayCommand(CanExecute = nameof(CanGoToNextPage))]
    private async Task NextPageAsync()
    {
        CurrentPage++;
        await LoadLogsAsync();
    }

    [RelayCommand]
    private async Task SearchAsync()
    {
        CurrentPage = 1;
        await LoadLogsAsync();
    }
}

public class ActionTypeOption
{
    public ActionTypeOption(string name, int? value)
    {
        Name = name;
        Value = value;
    }

    public string Name { get; }
    public int? Value { get; }

    public override string ToString() => Name;
}
