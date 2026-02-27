namespace Application.DTOs.PermissionDTOs;

public class PermissionDto
{
    public int Id { get; set; }
    public string Section { get; set; } = null!;
    public string Action { get; set; } = null!;
    public string DisplayName { get; set; } = null!;
}
