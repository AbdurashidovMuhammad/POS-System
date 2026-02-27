namespace Application.DTOs.PermissionDTOs;

public class UserPermissionsDto
{
    public int UserId { get; set; }
    public List<int> PermissionIds { get; set; } = new();
}
