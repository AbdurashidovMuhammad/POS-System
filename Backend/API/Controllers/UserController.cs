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
        var result = await _userService.CreateAdminAsync(dto);
        if (!result.Succeeded)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllAdmins()
    {
        var result = await _userService.GetAllAdminsAsync();
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
        var result = await _userService.UpdateAdminAsync(id, dto);
        if (!result.Succeeded)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeactivateAdmin(int id)
    {
        var result = await _userService.DeactivateAdminAsync(id);
        if (!result.Succeeded)
            return NotFound(result);

        return Ok(result);
    }
}
