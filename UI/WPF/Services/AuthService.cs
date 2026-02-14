using System.Text.Json;
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
    public int? UserId { get; private set; }
    public string? Username { get; private set; }
    public string? Role { get; private set; }
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

        ExtractUserInfoFromToken(AccessToken);

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

        ExtractUserInfoFromToken(AccessToken);

        return true;
    }

    public void Logout()
    {
        AccessToken = null;
        RefreshToken = null;
        UserId = null;
        Username = null;
        Role = null;
        _apiService.ClearAuthToken();
    }

    private void ExtractUserInfoFromToken(string token)
    {
        try
        {
            var parts = token.Split('.');
            if (parts.Length != 3) return;

            var payload = parts[1];
            // Add padding if needed
            switch (payload.Length % 4)
            {
                case 2: payload += "=="; break;
                case 3: payload += "="; break;
            }

            var jsonBytes = Convert.FromBase64String(payload.Replace('-', '+').Replace('_', '/'));
            var json = System.Text.Encoding.UTF8.GetString(jsonBytes);

            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (root.TryGetProperty("nameid", out var nameIdElement) ||
                root.TryGetProperty("sub", out nameIdElement))
            {
                if (int.TryParse(nameIdElement.GetString(), out var userId))
                {
                    UserId = userId;
                }
            }

            if (root.TryGetProperty("unique_name", out var usernameElement) ||
                root.TryGetProperty("name", out usernameElement) ||
                root.TryGetProperty("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name", out usernameElement))
            {
                Username = usernameElement.GetString();
            }

            if (root.TryGetProperty("role", out var roleElement) ||
                root.TryGetProperty("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", out roleElement))
            {
                Role = roleElement.GetString();
            }
        }
        catch
        {
            // Token parsing failed, leave UserId/Username as null
        }
    }
}
