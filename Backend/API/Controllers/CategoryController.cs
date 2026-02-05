using Application.DTOs.CategoryDTOs;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CategoryController : ControllerBase
{
    private readonly ICategoriesService _categoriesService;

    public CategoryController(ICategoriesService categoriesService)
    {
        _categoriesService = categoriesService;
    }

    [HttpPost("create")]
    public async Task<IActionResult> Create([FromBody] CreateCategoryDto dto)
    {
        var result = await _categoriesService.CreateCategoryAsync(dto);
        if (!result.Succeeded)
            return BadRequest(new { errors = result.Errors });

        return Ok(new { message = result.Result });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCategoryDto dto)
    {
        var result = await _categoriesService.UpdateCategoryAsync(id, dto);
        if (!result.Succeeded)
            return BadRequest(new { errors = result.Errors });

        return Ok(result.Result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _categoriesService.GetAllCategoriesAsync();
        return Ok(result.Result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _categoriesService.GetCategoryByIdAsync(id);
        if (!result.Succeeded)
            return NotFound(new { errors = result.Errors });

        return Ok(result.Result);
    }

    [HttpGet("suggest")]
    public async Task<IActionResult> Suggest([FromQuery] string query)
    {
        var result = await _categoriesService.SuggestCategoriesAsync(query);
        return Ok(result.Result);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _categoriesService.DeleteCategoryAsync(id);
        if (!result.Succeeded)
            return BadRequest(new { errors = result.Errors });

        return Ok(new { message = "Category muvaffaqiyatli o'chirildi." });
    }
}
