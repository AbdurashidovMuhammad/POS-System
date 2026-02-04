using Application.DTOs;
using Application.DTOs.UserDTOs;

namespace Application.Services;

public interface IUserService
{
    Task<ApiResult<UserDto>> CreateAdminAsync(CreateUserDto dto);
    Task<ApiResult<List<UserDto>>> GetAllAdminsAsync();
    Task<ApiResult<UserDto>> GetAdminByIdAsync(int id);
    Task<ApiResult<UserDto>> UpdateAdminAsync(int id, UpdateUserDto dto);
    Task<ApiResult<bool>> DeactivateAdminAsync(int id);
    Task<bool> IsUsernameUniqueAsync(string username, int? excludeUserId = null);
}
