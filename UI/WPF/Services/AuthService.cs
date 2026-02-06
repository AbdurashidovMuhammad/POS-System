using WPF.Models;

namespace WPF.Services;

public class AuthService : IAuthService
{
    private readonly IApiService _apiService;

    public AuthService(IApiService apiService)
    {
        _apiService = apiService;
    }

    public string? AccessToken { get; private set; }
    public string? RefreshToken { get; private set; }
    public bool IsAuthenticated => !string.IsNullOrEmpty(AccessToken);

    public async Task<(bool Success, string? ErrorMessage)> LoginAsync(string username, string password)
    {
        var request = new LoginRequest
        {
            Username = username,
            Password = password
        };

        var result = await _apiService.PostAsync<LoginResponse>("api/auth/login", request);

        if (result is null)
        {
            return (false, "Serverga ulanib bo'lmadi");
        }

        if (!result.Succeeded || result.Result is null)
        {
            var errorMessage = result.Errors.FirstOrDefault() ?? "Login muvaffaqiyatsiz";
            return (false, errorMessage);
        }

        AccessToken = result.Result.AccessToken;
        RefreshToken = result.Result.RefreshToken;
        _apiService.SetAuthToken(AccessToken);

        return (true, null);
    }

    public async Task<bool> RefreshTokenAsync()
    {
        if (string.IsNullOrEmpty(RefreshToken))
        {
            return false;
        }

        var request = new { RefreshToken };
        var result = await _apiService.PostAsync<LoginResponse>("api/auth/refresh", request);

        if (result is null || !result.Succeeded || result.Result is null)
        {
            Logout();
            return false;
        }

        AccessToken = result.Result.AccessToken;
        RefreshToken = result.Result.RefreshToken;
        _apiService.SetAuthToken(AccessToken);

        return true;
    }

    public void Logout()
    {
        AccessToken = null;
        RefreshToken = null;
        _apiService.ClearAuthToken();
    }
}
