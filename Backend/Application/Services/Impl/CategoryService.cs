using Application.DTOs;
using Application.DTOs.CategoryDTOs;
using Core.Entities;
using DataAccess.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Application.Services.Impl;

internal class CategoryService : ICategoriesService
{
    private readonly DatabaseContext _context;

    public CategoryService(DatabaseContext context)
    {
        _context = context;
    }

    public async Task<ApiResult<string>> CreateCategoryAsync(CreateCategoryDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return ApiResult<string>.Failure(new[] { "Category name cannot be empty." });

        if (!await IsNameUniqueAsync(dto.Name))
            return ApiResult<string>.Failure(new[] { $"Category '{dto.Name}' already exists." });

        var category = new Category { Name = dto.Name };

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        return ApiResult<string>.Success("Category muvaffaqiyatli yaratildi.");
    }

    public async Task<ApiResult<CategoryDto>> UpdateCategoryAsync(int id, UpdateCategoryDto dto)
    {
        var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);

        if (category is null)
            return ApiResult<CategoryDto>.Failure(new[] { $"Category with Id = {id} was not found." });

        if (dto.Name is not null)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                return ApiResult<CategoryDto>.Failure(new[] { "Category name cannot be empty." });

            if (!await IsNameUniqueAsync(dto.Name, id))
                return ApiResult<CategoryDto>.Failure(new[] { $"Category '{dto.Name}' already exists." });

            category.Name = dto.Name;
        }

        if (dto.IsActive is not null)
            category.IsActive = dto.IsActive.Value;

        await _context.SaveChangesAsync();

        return ApiResult<CategoryDto>.Success(MapToDto(category));
    }

    public async Task<ApiResult<List<CategoryDto>>> GetAllCategoriesAsync()
    {
        var categories = await _context.Categories
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .ToListAsync();

        return ApiResult<List<CategoryDto>>.Success(categories.Select(MapToDto).ToList());
    }

    public async Task<ApiResult<CategoryDto>> GetCategoryByIdAsync(int id)
    {
        var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);

        if (category is null)
            return ApiResult<CategoryDto>.Failure(new[] { $"Category with Id = {id} was not found." });

        return ApiResult<CategoryDto>.Success(MapToDto(category));
    }

    public async Task<ApiResult<List<CategorySuggestDto>>> SuggestCategoriesAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return ApiResult<List<CategorySuggestDto>>.Success(new List<CategorySuggestDto>());

        var lowerQuery = query.Trim().ToLower();

        var categories = await _context.Categories
            .Where(c => c.IsActive && c.Name.ToLower().StartsWith(lowerQuery))
            .OrderBy(c => c.Name)
            .Take(10)
            .Select(c => new CategorySuggestDto { Id = c.Id, Name = c.Name })
            .ToListAsync();

        return ApiResult<List<CategorySuggestDto>>.Success(categories);
    }

    public async Task<ApiResult<bool>> DeleteCategoryAsync(int id)
    {
        var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);

        if (category is null)
            return ApiResult<bool>.Failure(new[] { $"Category with Id = {id} was not found." });

        var hasProducts = await _context.Products.AnyAsync(p => p.CategoryId == id);

        if (hasProducts)
            return ApiResult<bool>.Failure(new[] { "This category has products and cannot be deleted." });

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();

        return ApiResult<bool>.Success(true);
    }

    private async Task<bool> IsNameUniqueAsync(string name, int? excludeCategoryId = null)
    {
        var query = _context.Categories.Where(c => c.Name == name);

        if (excludeCategoryId is not null)
            query = query.Where(c => c.Id != excludeCategoryId.Value);

        return !await query.AnyAsync();
    }

    private static CategoryDto MapToDto(Category category) => new()
    {
        Id = category.Id,
        Name = category.Name,
        IsActive = category.IsActive
    };
}
