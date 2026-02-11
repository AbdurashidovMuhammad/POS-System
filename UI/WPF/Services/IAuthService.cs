namespace WPF.Services;

public interface IAuthService
{
    Task<(bool Success, string? ErrorMessage)> LoginAsync(string username, string password);
    Task<bool> RefreshTokenAsync();
    void Logout();
    bool IsAuthenticated { get; }
    string? AccessToken { get; }
    string? RefreshToken { get; }
    int? UserId { get; }
    string? Username { get; }
    string? Role { get; }
}
