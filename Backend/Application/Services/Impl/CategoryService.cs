using Application.DTOs;
using Application.DTOs.CategoryDTOs;
using Core.Entities;
using Core.Enums;
using DataAccess.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Application.Services.Impl;

internal class CategoryService : ICategoriesService
{
    private readonly DatabaseContext _context;
    private readonly IAuditLogService _auditLogService;

    public CategoryService(DatabaseContext context, IAuditLogService auditLogService)
    {
        _context = context;
        _auditLogService = auditLogService;
    }

    public async Task<ApiResult<string>> CreateCategoryAsync(CreateCategoryDto dto, int userId)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return ApiResult<string>.Failure(new[] { "Category name cannot be empty." });

        if (!await IsNameUniqueAsync(dto.Name))
            return ApiResult<string>.Failure(new[] { $"Category '{dto.Name}' already exists." });

        var category = new Category { Name = dto.Name };

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        try { await _auditLogService.LogAsync(userId, Action_Type.CategoryCreate, "Category", category.Id, $"Kategoriya yaratdi: {dto.Name}"); }
        catch { }

        return ApiResult<string>.Success("Category muvaffaqiyatli yaratildi.");
    }

    public async Task<ApiResult<CategoryDto>> UpdateCategoryAsync(int id, UpdateCategoryDto dto, int userId)
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

        try { await _auditLogService.LogAsync(userId, Action_Type.CategoryUpdate, "Category", id, $"Kategoriyani yangiladi: {category.Name}"); }
        catch { }

        return ApiResult<CategoryDto>.Success(MapToDto(category));
    }

    public async Task<ApiResult<List<CategoryDto>>> GetAllCategoriesAsync()
    {
        var categories = await _context.Categories
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

    public async Task<ApiResult<bool>> DeleteCategoryAsync(int id, int userId)
    {
        var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);

        if (category is null)
            return ApiResult<bool>.Failure(new[] { $"Category with Id = {id} was not found." });

        var hasProducts = await _context.Products.AnyAsync(p => p.CategoryId == id);

        if (hasProducts)
            return ApiResult<bool>.Failure(new[] { "This category has products and cannot be deleted." });

        var categoryName = category.Name;
        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();

        try { await _auditLogService.LogAsync(userId, Action_Type.CategoryDelete, "Category", id, $"Kategoriyani o'chirdi: {categoryName}"); }
        catch { }

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
