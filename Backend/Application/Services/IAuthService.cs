using Application.DTOs;
using Application.DTOs.UserDTOs;

namespace Application.Services;

public interface IAuthService
{
    Task<ApiResult<LoginResponseDto>> LoginAsync(LoginDto dto);
    Task<ApiResult<LoginResponseDto>> RefreshAsync(string refreshToken);
}
