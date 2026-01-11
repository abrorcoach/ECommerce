using ECommerce.Application.DTOs;

namespace ECommerce.Application.Interfaces;

public interface IProductService
{
    Task<IEnumerable<ProductDto>> GetAllProducts();
    Task<ProductDto?> GetProductById(int id);
    Task<IEnumerable<ProductDto>> GetProductsByCategory(int categoryId);
    Task<ProductDto> CreateProduct(CreateProductRequest request);
    Task<ProductDto?> UpdateProduct(int id, UpdateProductRequest request);
    Task<bool> DeleteProduct(int id);
}
