using Application.DTOs;
using Application.DTOs.Common;
using Application.DTOs.UserDTOs;
using Application.Helpers;
using Core.Entities;
using Core.Enums;
using DataAccess.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Application.Services.Impl;

internal class UserService : IUserService
{
    private readonly DatabaseContext _context;
    private readonly IAuditLogService _auditLogService;

    public UserService(DatabaseContext context, IAuditLogService auditLogService)
    {
        _context = context;
        _auditLogService = auditLogService;
    }

    public async Task<ApiResult<UserDto>> CreateAdminAsync(CreateUserDto dto, int performedByUserId)
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
            Password = dto.Password,
            PasswordHash = PasswordHelper.HashPassword(dto.Password),
            Role = Role.Admin,
            IsActive = true
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        try { await _auditLogService.LogAsync(performedByUserId, Action_Type.UserCreate, "User", user.Id, $"Foydalanuvchi yaratdi: {dto.Username}"); }
        catch { }

        return ApiResult<UserDto>.Success(MapToDto(user));
    }

    public async Task<ApiResult<PagedResult<UserDto>>> GetAllAdminsAsync(PaginationParams pagination)
    {
        var query = _context.Users.Where(u => u.Role == Role.Admin);

        var totalCount = await query.CountAsync();

        var users = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToListAsync();

        var result = new PagedResult<UserDto>
        {
            Items = users.Select(MapToDto).ToList(),
            Page = pagination.Page,
            PageSize = pagination.PageSize,
            TotalCount = totalCount
        };

        return ApiResult<PagedResult<UserDto>>.Success(result);
    }

    public async Task<ApiResult<List<UserDto>>> GetAllAdminsListAsync()
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

    public async Task<ApiResult<UserDto>> UpdateAdminAsync(int id, UpdateUserDto dto, int performedByUserId)
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

            user.Password = dto.Password;
            user.PasswordHash = PasswordHelper.HashPassword(dto.Password);
        }

        if (dto.IsActive is not null)
            user.IsActive = dto.IsActive.Value;

        await _context.SaveChangesAsync();

        try { await _auditLogService.LogAsync(performedByUserId, Action_Type.UserUpdate, "User", id, $"Foydalanuvchini yangiladi: {user.Username}"); }
        catch { }

        return ApiResult<UserDto>.Success(MapToDto(user));
    }

    public async Task<ApiResult<bool>> DeactivateAdminAsync(int id, int performedByUserId)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);

        if (user is null)
            return ApiResult<bool>.Failure(new[] { $"User with Id = {id} was not found." });

        user.IsActive = false;
        await _context.SaveChangesAsync();

        try { await _auditLogService.LogAsync(performedByUserId, Action_Type.UserDeactivate, "User", id, $"Foydalanuvchini o'chirdi: {user.Username}"); }
        catch { }

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
        Password = user.Password,
        Role = user.Role.ToString(),
        IsActive = user.IsActive,
        CreatedAt = user.CreatedAt
    };
}
