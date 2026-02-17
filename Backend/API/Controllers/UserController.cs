using System.Security.Claims;
using Application.DTOs.Common;
using Application.DTOs.UserDTOs;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "SuperAdmin")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateAdmin([FromBody] CreateUserDto dto)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        int.TryParse(userIdClaim?.Value, out var userId);

        var result = await _userService.CreateAdminAsync(dto, userId);
        if (!result.Succeeded)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllAdmins([FromQuery] PaginationParams pagination)
    {
        var result = await _userService.GetAllAdminsAsync(pagination);
        return Ok(result);
    }

    [HttpGet("list")]
    public async Task<IActionResult> GetAllAdminsList()
    {
        var result = await _userService.GetAllAdminsListAsync();
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetAdminById(int id)
    {
        var result = await _userService.GetAdminByIdAsync(id);
        if (!result.Succeeded)
            return NotFound(result);

        return Ok(result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateAdmin(int id, [FromBody] UpdateUserDto dto)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        int.TryParse(userIdClaim?.Value, out var userId);

        var result = await _userService.UpdateAdminAsync(id, dto, userId);
        if (!result.Succeeded)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeactivateAdmin(int id)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        int.TryParse(userIdClaim?.Value, out var userId);

        var result = await _userService.DeactivateAdminAsync(id, userId);
        if (!result.Succeeded)
            return NotFound(result);

        return Ok(result);
    }
}
