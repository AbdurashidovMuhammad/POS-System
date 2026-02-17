using Application.DTOs;
using Application.DTOs.Common;
using Application.DTOs.UserDTOs;

namespace Application.Services;

public interface IUserService
{
    Task<ApiResult<UserDto>> CreateAdminAsync(CreateUserDto dto, int performedByUserId);
    Task<ApiResult<PagedResult<UserDto>>> GetAllAdminsAsync(PaginationParams pagination);
    Task<ApiResult<List<UserDto>>> GetAllAdminsListAsync();
    Task<ApiResult<UserDto>> GetAdminByIdAsync(int id);
    Task<ApiResult<UserDto>> UpdateAdminAsync(int id, UpdateUserDto dto, int performedByUserId);
    Task<ApiResult<bool>> DeactivateAdminAsync(int id, int performedByUserId);
    Task<bool> IsUsernameUniqueAsync(string username, int? excludeUserId = null);
}
