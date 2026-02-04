namespace Application.DTOs.UserDTOs;

public class UpdateUserDto
{
    public string? Username { get; set; }
    public string? Password { get; set; }
    public bool? IsActive { get; set; }
}
