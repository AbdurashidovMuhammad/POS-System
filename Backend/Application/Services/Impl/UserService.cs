using Application.DTOs;
using Application.DTOs.UserDTOs;
using Application.Helpers;
using Application.Services;
using Core.Entities;
using Core.Enums;
using DataAccess.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Application.Services.Impl;

internal class UserService : IUserService
{
    private readonly DatabaseContext _context;

    public UserService(DatabaseContext context)
    {
        _context = context;
    }

    public async Task<ApiResult<UserDto>> CreateAdminAsync(CreateUserDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Username))
            return ApiResult<UserDto>.Failure(new[] { "Username cannot be empty." });

        if (string.IsNullOrWhiteSpace(dto.Password) || dto.Password.Length < 6)
            return ApiResult<UserDto>.Failure(new[] { "Password must be at least 6 characters." });

        if (!await IsUsernameUniqueAsync(dto.Username))
            return ApiResult<UserDto>.Failure(new[] { $"Username '{dto.Username}' is already taken." });

        var user = new User
        {
            Username = dto.Username,
            PasswordHash = PasswordHelper.HashPassword(dto.Password),
            Role = Role.Admin,
            IsActive = true
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return ApiResult<UserDto>.Success(MapToDto(user));
    }

    public async Task<ApiResult<List<UserDto>>> GetAllAdminsAsync()
    {
        var users = await _context.Users
            .Where(u => u.Role == Role.Admin)
            .ToListAsync();

        return ApiResult<List<UserDto>>.Success(users.Select(MapToDto).ToList());
    }

    public async Task<ApiResult<UserDto>> GetAdminByIdAsync(int id)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);

        if (user is null)
            return ApiResult<UserDto>.Failure(new[] { $"User with Id = {id} was not found." });

        return ApiResult<UserDto>.Success(MapToDto(user));
    }

    public async Task<ApiResult<UserDto>> UpdateAdminAsync(int id, UpdateUserDto dto)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);

        if (user is null)
            return ApiResult<UserDto>.Failure(new[] { $"User with Id = {id} was not found." });

        if (dto.Username is not null)
        {
            if (string.IsNullOrWhiteSpace(dto.Username))
                return ApiResult<UserDto>.Failure(new[] { "Username cannot be empty." });

            if (!await IsUsernameUniqueAsync(dto.Username, id))
                return ApiResult<UserDto>.Failure(new[] { $"Username '{dto.Username}' is already taken." });

            user.Username = dto.Username;
        }

        if (dto.Password is not null)
        {
            if (dto.Password.Length < 6)
                return ApiResult<UserDto>.Failure(new[] { "Password must be at least 6 characters." });

            user.PasswordHash = PasswordHelper.HashPassword(dto.Password);
        }

        if (dto.IsActive is not null)
            user.IsActive = dto.IsActive.Value;

        await _context.SaveChangesAsync();

        return ApiResult<UserDto>.Success(MapToDto(user));
    }

    public async Task<ApiResult<bool>> DeactivateAdminAsync(int id)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);

        if (user is null)
            return ApiResult<bool>.Failure(new[] { $"User with Id = {id} was not found." });

        user.IsActive = false;
        await _context.SaveChangesAsync();

        return ApiResult<bool>.Success(true);
    }

    public async Task<bool> IsUsernameUniqueAsync(string username, int? excludeUserId = null)
    {
        var query = _context.Users.Where(u => u.Username == username);

        if (excludeUserId is not null)
            query = query.Where(u => u.Id != excludeUserId.Value);

        return !await query.AnyAsync();
    }

    private static UserDto MapToDto(User user) => new()
    {
        Id = user.Id,
        Username = user.Username,
        Role = user.Role.ToString(),
        IsActive = user.IsActive,
        CreatedAt = user.CreatedAt
    };
}
