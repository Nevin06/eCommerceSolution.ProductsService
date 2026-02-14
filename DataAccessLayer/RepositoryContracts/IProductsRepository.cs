using DataAccessLayer.Entities;
using System.Linq.Expressions;

namespace DataAccessLayer.RepositoryContracts;
// 37

/// <summary>
/// Repository for managing 'products' table
/// </summary>
public interface IProductsRepository
{
    Task<IEnumerable<Product>> GetAllProductsAsync();
    Task<IEnumerable<Product>> GetProductsByConditionAsync(Expression<Func<Product, bool>> expression);
    Task<Product?> GetProductByConditionAsync(Expression<Func<Product, bool>> expression);
    Task<Product?> AddProductAsync(Product product);
    Task<Product?> UpdateProductAsync(Product product);
    Task<bool> DeleteProductAsync(Guid productID);
}
