using BusinessLogicLayer.DTOs;
using DataAccessLayer.Entities;
using System.Linq.Expressions;

namespace BusinessLogicLayer.ServiceContracts;

public interface IProductsService
{
    Task<List<ProductResponse?>> GetProductsAsync();
    Task<List<ProductResponse?>> GetProductsByConditionAsync(Expression<Func<Product, bool>> expression);
    Task<ProductResponse?> GetProductByConditionAsync(Expression<Func<Product, bool>> expression);
    Task<ProductResponse> AddProductAsync(ProductAddRequest productAddRequest);
    Task<ProductResponse> UpdateProductAsync(ProductUpdateRequest productUpdateRequest);
    Task<bool> DeleteProductAsync(Guid ProductID);
    Task<ProductResponse?> GetProductAsync(string productId);
}
