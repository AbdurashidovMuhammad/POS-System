using System.Collections.ObjectModel;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using WPF.Messages;
using WPF.Services;

namespace WPF.ViewModels;

public partial class ShellViewModel : ViewModelBase, IRecipient<NavigateToViewMessage>
{
    private readonly INavigationService _navigationService;
    private readonly IAuthService _authService;
    private readonly IServiceProvider _serviceProvider;
    private readonly IApiService _apiService;
    private DispatcherTimer? _toastTimer;

    public ShellViewModel(
        INavigationService navigationService,
        IAuthService authService,
        IServiceProvider serviceProvider,
        IApiService apiService)
    {
        _navigationService = navigationService;
        _authService = authService;
        _serviceProvider = serviceProvider;
        _apiService = apiService;

        _apiService.OnForbidden = ShowToast;

        WeakReferenceMessenger.Default.Register(this);

        InitializeMenuItems();

        if (string.Equals(_authService.Role, "Admin", StringComparison.OrdinalIgnoreCase))
        {
            IsSidebarCollapsed = true;
            var salesItem = MenuItems.FirstOrDefault(m => m.ViewModelName == nameof(SalesViewModel));
            if (salesItem is not null)
                SelectedMenuItem = salesItem;
        }
        else
        {
            NavigateToDashboard();
        }
    }

    public void Receive(NavigateToViewMessage message)
    {
        var menuItem = MenuItems.FirstOrDefault(m => m.ViewModelName == message.Value);
        if (menuItem is not null)
        {
            SelectedMenuItem = menuItem;
        }
    }

    [ObservableProperty]
    private ObservableCollection<MenuItemViewModel> _menuItems = new();

    [ObservableProperty]
    private MenuItemViewModel? _selectedMenuItem;

    [ObservableProperty]
    private object? _currentView;

    [ObservableProperty]
    private string _pageTitle = "Bosh sahifa";

    [ObservableProperty]
    private bool _isSidebarCollapsed;

    [ObservableProperty]
    private string? _toastMessage;

    [ObservableProperty]
    private bool _isToastVisible;

    private readonly Dictionary<string, object> _viewModelCache = new();

    private void InitializeMenuItems()
    {
        var userRole = _authService.Role;
        var isSuperAdmin = string.Equals(userRole, "SuperAdmin", StringComparison.OrdinalIgnoreCase);
        var dashboardVmName = isSuperAdmin ? nameof(AdminDashboardViewModel) : nameof(DashboardViewModel);

        var allMenuItems = new List<MenuItemViewModel>
        {
            new("Bosh sahifa", "M10,20V14H14V20H19V12H22L12,3L2,12H5V20H10Z", dashboardVmName, true),
            new("Sotish", "M17,18C15.89,18 15,18.89 15,20A2,2 0 0,0 17,22A2,2 0 0,0 19,20C19,18.89 18.1,18 17,18M1,2V4H3L6.6,11.59L5.24,14.04C5.09,14.32 5,14.65 5,15A2,2 0 0,0 7,17H19V15H7.42A0.25,0.25 0 0,1 7.17,14.75C7.17,14.7 7.18,14.66 7.2,14.63L8.1,13H15.55C16.3,13 16.96,12.58 17.3,11.97L20.88,5.5C20.95,5.34 21,5.17 21,5A1,1 0 0,0 20,4H5.21L4.27,2M7,18C5.89,18 5,18.89 5,20A2,2 0 0,0 7,22A2,2 0 0,0 9,20C9,18.89 8.1,18 7,18Z", nameof(SalesViewModel)),
            new("Mahsulotlar", "M12,3L2,12H5V20H19V12H22L12,3M12,8.75A2.25,2.25 0 0,1 14.25,11A2.25,2.25 0 0,1 12,13.25A2.25,2.25 0 0,1 9.75,11A2.25,2.25 0 0,1 12,8.75M12,15C13.5,15 16.5,15.75 16.5,17.25V18H7.5V17.25C7.5,15.75 10.5,15 12,15Z", nameof(ProductViewModel)),
            new("Kategoriyalar", "M3,4H7V8H3V4M9,5V7H21V5H9M3,10H7V14H3V10M9,11V13H21V11H9M3,16H7V20H3V16M9,17V19H21V17H9", nameof(CategoryViewModel)),
            new("Hisobotlar", "M19,3H5C3.89,3 3,3.89 3,5V19A2,2 0 0,0 5,21H19A2,2 0 0,0 21,19V5C21,3.89 20.1,3 19,3M9,17H7V10H9V17M13,17H11V7H13V17M17,17H15V13H17V17Z", nameof(ReportViewModel), requiredRole: "SuperAdmin"),
            new("Foydalanuvchilar", "M16 17V19H2V17S2 13 9 13 16 17 16 17M12.5 7.5A3.5 3.5 0 1 0 9 11A3.5 3.5 0 0 0 12.5 7.5M15.94 13A5.32 5.32 0 0 1 18 17V19H22V17S22 13.37 15.94 13M15 4A3.39 3.39 0 0 0 13.07 4.59A5 5 0 0 1 13.07 10.41A3.39 3.39 0 0 0 15 11A3.5 3.5 0 0 0 15 4Z", nameof(UserViewModel), requiredRole: "SuperAdmin"),
            new("Faoliyat jurnali", "M13,2.05V4.05C17.39,4.59 20.5,8.58 19.96,12.97C19.5,16.61 16.64,19.5 13,19.93V21.93C18.5,21.38 22.5,16.5 21.95,11C21.5,6.25 17.73,2.5 13,2.05M5.67,19.74C7.18,21 9.04,21.79 11,22V20C9.58,19.82 8.23,19.25 7.1,18.37L5.67,19.74M2.06,13C2.26,14.96 3.03,16.81 4.27,18.33L5.68,16.92C4.82,15.79 4.24,14.45 4.06,13H2.06M4.26,5.67L5.67,7.1C6.81,6.22 8.15,5.64 9.57,5.44V3.42C7.62,3.64 5.78,4.39 4.26,5.67M2.06,11H4.06C4.24,9.58 4.8,8.23 5.67,7.1L4.26,5.68C3.03,7.19 2.26,9.04 2.06,11M7,12.5A5,5 0 0,0 12,17.5A5,5 0 0,0 17,12.5A5,5 0 0,0 12,7.5A5,5 0 0,0 7,12.5M12,9.5A3,3 0 0,1 15,12.5A3,3 0 0,1 12,15.5A3,3 0 0,1 9,12.5A3,3 0 0,1 12,9.5Z", nameof(ActivityLogViewModel), requiredRole: "SuperAdmin"),
        };

        // Filter out menu items the user doesn't have access to
        var filteredItems = allMenuItems
            .Where(m => m.RequiredRole is null ||
                        string.Equals(userRole, m.RequiredRole, StringComparison.OrdinalIgnoreCase))
            .ToList();

        MenuItems = new ObservableCollection<MenuItemViewModel>(filteredItems);

        SelectedMenuItem = MenuItems.FirstOrDefault(m => m.IsSelected);
    }

    partial void OnSelectedMenuItemChanged(MenuItemViewModel? value)
    {
        if (value is null) return;

        foreach (var item in MenuItems)
        {
            item.IsSelected = item == value;
        }

        NavigateToView(value.ViewModelName);
        PageTitle = value.Title;
    }

    private void NavigateToView(string viewModelName)
    {
        if (!_viewModelCache.TryGetValue(viewModelName, out var viewModel))
        {
            viewModel = viewModelName switch
            {
                nameof(DashboardViewModel) => _serviceProvider.GetService(typeof(DashboardViewModel)),
                nameof(AdminDashboardViewModel) => _serviceProvider.GetService(typeof(AdminDashboardViewModel)),
                nameof(SalesViewModel) => _serviceProvider.GetService(typeof(SalesViewModel)),
                nameof(ProductViewModel) => _serviceProvider.GetService(typeof(ProductViewModel)),
                nameof(CategoryViewModel) => _serviceProvider.GetService(typeof(CategoryViewModel)),
                nameof(ReportViewModel) => _serviceProvider.GetService(typeof(ReportViewModel)),
                nameof(UserViewModel) => _serviceProvider.GetService(typeof(UserViewModel)),
                nameof(ActivityLogViewModel) => _serviceProvider.GetService(typeof(ActivityLogViewModel)),
                _ => null
            };

            if (viewModel is not null)
            {
                _viewModelCache[viewModelName] = viewModel;
            }
        }

        CurrentView = viewModel;
    }

    [RelayCommand]
    private void NavigateToDashboard()
    {
        var dashboardItem = MenuItems.FirstOrDefault(m =>
            m.ViewModelName == nameof(DashboardViewModel) ||
            m.ViewModelName == nameof(AdminDashboardViewModel));
        if (dashboardItem is not null)
        {
            SelectedMenuItem = dashboardItem;
        }
    }

    [RelayCommand]
    private void SelectMenu(MenuItemViewModel? menuItem)
    {
        if (menuItem is not null)
        {
            SelectedMenuItem = menuItem;
        }
    }

    [RelayCommand]
    private void ToggleSidebar()
    {
        IsSidebarCollapsed = !IsSidebarCollapsed;
    }

    private void ShowToast(string message)
    {
        System.Windows.Application.Current.Dispatcher.Invoke(() =>
        {
            ToastMessage = message;
            IsToastVisible = true;

            _toastTimer?.Stop();
            _toastTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(3) };
            _toastTimer.Tick += (_, _) =>
            {
                IsToastVisible = false;
                ToastMessage = null;
                _toastTimer.Stop();
            };
            _toastTimer.Start();
        });
    }

    [RelayCommand]
    private void Logout()
    {
        _authService.Logout();
        _viewModelCache.Clear();
        _navigationService.ClearCache();
        _navigationService.NavigateTo<LoginViewModel>();
    }
}

public partial class MenuItemViewModel : ObservableObject
{
    public MenuItemViewModel(string title, string iconPath, string viewModelName, bool isSelected = false, string? requiredRole = null)
    {
        Title = title;
        IconPath = iconPath;
        ViewModelName = viewModelName;
        IsSelected = isSelected;
        RequiredRole = requiredRole;
    }

    public string Title { get; }
    public string IconPath { get; }
    public string ViewModelName { get; }
    public string? RequiredRole { get; }

    [ObservableProperty]
    private bool _isSelected;
}
