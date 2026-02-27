using System.Security.Claims;
using Application.Authorization;
using Application.DTOs.CategoryDTOs;
using Application.DTOs.Common;
using Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoryController : ControllerBase
{
    private readonly ICategoriesService _categoriesService;

    public CategoryController(ICategoriesService categoriesService)
    {
        _categoriesService = categoriesService;
    }

    [HttpPost("create")]
    [HasPermission("Categories", "Create")]
    public async Task<IActionResult> Create([FromBody] CreateCategoryDto dto)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        int.TryParse(userIdClaim?.Value, out var userId);

        var result = await _categoriesService.CreateCategoryAsync(dto, userId);
        if (!result.Succeeded)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPut("{id:int}")]
    [HasPermission("Categories", "Update")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCategoryDto dto)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        int.TryParse(userIdClaim?.Value, out var userId);

        var result = await _categoriesService.UpdateCategoryAsync(id, dto, userId);
        if (!result.Succeeded)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpGet]
    [HasPermission("Categories", "Read")]
    public async Task<IActionResult> GetAll([FromQuery] PaginationParams pagination)
    {
        var result = await _categoriesService.GetAllCategoriesAsync(pagination);
        return Ok(result);
    }

    [HttpGet("list")]
    [HasPermission("Categories", "Read")]
    public async Task<IActionResult> GetAllList()
    {
        var result = await _categoriesService.GetAllCategoriesListAsync();
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [HasPermission("Categories", "Read")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _categoriesService.GetCategoryByIdAsync(id);
        if (!result.Succeeded)
            return NotFound(result);

        return Ok(result);
    }

    [HttpGet("suggest")]
    [HasPermission("Categories", "Read")]
    public async Task<IActionResult> Suggest([FromQuery] string query)
    {
        var result = await _categoriesService.SuggestCategoriesAsync(query);
        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    [HasPermission("Categories", "Delete")]
    public async Task<IActionResult> Delete(int id)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        int.TryParse(userIdClaim?.Value, out var userId);

        var result = await _categoriesService.DeleteCategoryAsync(id, userId);
        if (!result.Succeeded)
            return BadRequest(result);

        return Ok(result);
    }
}
