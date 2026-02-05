using Application.DTOs;
using Application.DTOs.CategoryDTOs;

namespace Application.Services;

public interface ICategoriesService
{
    Task<ApiResult<string>> CreateCategoryAsync(CreateCategoryDto dto);
    Task<ApiResult<CategoryDto>> UpdateCategoryAsync(int id, UpdateCategoryDto dto);
    Task<ApiResult<List<CategoryDto>>> GetAllCategoriesAsync();
    Task<ApiResult<CategoryDto>> GetCategoryByIdAsync(int id);
    Task<ApiResult<List<CategorySuggestDto>>> SuggestCategoriesAsync(string query);
    Task<ApiResult<bool>> DeleteCategoryAsync(int id);
}
