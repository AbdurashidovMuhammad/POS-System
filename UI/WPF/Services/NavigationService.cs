using Microsoft.Extensions.DependencyInjection;

namespace WPF.Services;

public class NavigationService : INavigationService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Stack<object> _navigationStack = new();
    private readonly Dictionary<Type, object> _viewModelCache = new();

    public NavigationService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public object? CurrentViewModel { get; private set; }
    public bool CanGoBack => _navigationStack.Count > 0;

    public event Action<object>? CurrentViewModelChanged;

    public void NavigateTo<TViewModel>() where TViewModel : class
    {
        NavigateTo(typeof(TViewModel));
    }

    public void NavigateTo(Type viewModelType)
    {
        if (CurrentViewModel is not null)
        {
            _navigationStack.Push(CurrentViewModel);
        }

        if (!_viewModelCache.TryGetValue(viewModelType, out var viewModel))
        {
            viewModel = _serviceProvider.GetRequiredService(viewModelType);
            _viewModelCache[viewModelType] = viewModel;
        }

        CurrentViewModel = viewModel;
        CurrentViewModelChanged?.Invoke(viewModel);
    }

    public void GoBack()
    {
        if (!CanGoBack) return;

        CurrentViewModel = _navigationStack.Pop();
        CurrentViewModelChanged?.Invoke(CurrentViewModel);
    }

    public void ClearCache()
    {
        _viewModelCache.Clear();
        _navigationStack.Clear();
        CurrentViewModel = null;
    }
}
