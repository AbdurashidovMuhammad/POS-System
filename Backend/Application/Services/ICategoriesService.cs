using Application.DTOs;
using Application.DTOs.CategoryDTOs;
using Application.DTOs.Common;

namespace Application.Services;

public interface ICategoriesService
{
    Task<ApiResult<string>> CreateCategoryAsync(CreateCategoryDto dto, int userId);
    Task<ApiResult<CategoryDto>> UpdateCategoryAsync(int id, UpdateCategoryDto dto, int userId);
    Task<ApiResult<PagedResult<CategoryDto>>> GetAllCategoriesAsync(PaginationParams pagination);
    Task<ApiResult<List<CategoryDto>>> GetAllCategoriesListAsync();
    Task<ApiResult<CategoryDto>> GetCategoryByIdAsync(int id);
    Task<ApiResult<List<CategorySuggestDto>>> SuggestCategoriesAsync(string query);
    Task<ApiResult<bool>> DeleteCategoryAsync(int id, int userId);
}
