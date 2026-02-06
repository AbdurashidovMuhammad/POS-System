using CommunityToolkit.Mvvm.ComponentModel;

namespace WPF.ViewModels;

public abstract class ViewModelBase : ObservableObject
{
    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    private string? _errorMessage;
    public string? ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    private string? _successMessage;
    public string? SuccessMessage
    {
        get => _successMessage;
        set => SetProperty(ref _successMessage, value);
    }

    protected void ClearError() => ErrorMessage = null;
    protected void ClearSuccess() => SuccessMessage = null;
    protected void ClearMessages()
    {
        ErrorMessage = null;
        SuccessMessage = null;
    }
}
