using Application.DTOs;
using Application.DTOs.UserDTOs;
using Application.Helpers;
using Application.Options;
using Core.Enums;
using DataAccess.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Application.Services.Impl;

internal class AuthService : IAuthService
{
    private readonly DatabaseContext _context;
    private readonly JwtOption _jwtOptions;

    public AuthService(DatabaseContext context, IConfiguration configuration)
    {
        _context = context;
        _jwtOptions = configuration.GetSection("JwtOption").Get<JwtOption>()
            ?? throw new InvalidOperationException("JwtOption not found in configuration.");
    }

    public async Task<ApiResult<LoginResponseDto>> LoginAsync(LoginDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Username) || string.IsNullOrWhiteSpace(dto.Password))
            return ApiResult<LoginResponseDto>.Failure(new[] { "Username and Password are required." });

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == dto.Username);

        if (user is null || !PasswordHelper.VerifyPassword(dto.Password, user.PasswordHash))
            return ApiResult<LoginResponseDto>.Failure(new[] { "Invalid username or password." });

        if (!user.IsActive)
            return ApiResult<LoginResponseDto>.Failure(new[] { "Account is not active." });

        var permissions = await LoadPermissionsAsync(user.Id, user.Role);

        var (accessToken, _) = TokenHelper.GenerateToken(
            user.Id, user.Username, user.Role.ToString(), _jwtOptions, permissions);

        user.RefreshToken = TokenHelper.GenerateRefreshToken();
        user.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenExpirationDays);
        await _context.SaveChangesAsync();

        return ApiResult<LoginResponseDto>.Success(MapToResponse(user.RefreshToken!, accessToken));
    }

    public async Task<ApiResult<LoginResponseDto>> RefreshAsync(string refreshToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            return ApiResult<LoginResponseDto>.Failure(new[] { "Refresh token is required." });

        var user = await _context.Users.FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);

        if (user is null)
            return ApiResult<LoginResponseDto>.Failure(new[] { "Invalid refresh token." });

        if (user.RefreshTokenExpiresAt < DateTime.UtcNow)
            return ApiResult<LoginResponseDto>.Failure(new[] { "Refresh token has expired." });

        var permissions = await LoadPermissionsAsync(user.Id, user.Role);

        var (accessToken, _) = TokenHelper.GenerateToken(
            user.Id, user.Username, user.Role.ToString(), _jwtOptions, permissions);

        user.RefreshToken = TokenHelper.GenerateRefreshToken();
        user.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenExpirationDays);
        await _context.SaveChangesAsync();

        return ApiResult<LoginResponseDto>.Success(MapToResponse(user.RefreshToken!, accessToken));
    }

    private async Task<IEnumerable<string>> LoadPermissionsAsync(int userId, Role role)
    {
        // SuperAdmin has all permissions implicitly via role check — no claims needed
        if (role == Role.SuperAdmin)
            return [];

        return await _context.UserPermissions
            .Where(up => up.UserId == userId)
            .Select(up => $"{up.Permission.Section}.{up.Permission.Action}")
            .ToListAsync();
    }

    private static LoginResponseDto MapToResponse(string refreshToken, string accessToken) => new()
    {
        AccessToken = accessToken,
        RefreshToken = refreshToken
    };
}
