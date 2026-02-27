using Application.DTOs.PermissionDTOs;

namespace Application.Services;

public interface IPermissionService
{
    Task<List<PermissionGroupDto>> GetAllPermissionsAsync();
    Task<UserPermissionsDto> GetUserPermissionsAsync(int userId);
    Task UpdateUserPermissionsAsync(int userId, List<int> permissionIds);
}
