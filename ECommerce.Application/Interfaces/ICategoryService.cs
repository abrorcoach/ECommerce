using ECommerce.Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ECommerce.Application.Interfaces;

public interface ICategoryService
{
    Task<IEnumerable<CategoryDto>> GetAllCategories();
    Task<CategoryDto?> GetCategoryById(int id);
    Task<CategoryDto> CreateCategory(CreateCategoryRequest request);
    Task<CategoryDto?> UpdateCategory(int id, UpdateCategoryRequest request);
    Task<bool> DeleteCategory(int id);
}
