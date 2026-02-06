namespace WPF.Services;

public interface INavigationService
{
    void NavigateTo<TViewModel>() where TViewModel : class;
    void NavigateTo(Type viewModelType);
    void GoBack();
    bool CanGoBack { get; }
    event Action<object>? CurrentViewModelChanged;
    object? CurrentViewModel { get; }
}
