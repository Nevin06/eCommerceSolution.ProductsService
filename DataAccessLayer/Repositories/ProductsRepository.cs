using DataAccessLayer.Context;
using DataAccessLayer.Entities;
using DataAccessLayer.RepositoryContracts;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace DataAccessLayer.Repositories;

// 37
public class ProductsRepository : IProductsRepository
{
    private readonly ApplicationDbContext _context;

    public ProductsRepository(ApplicationDbContext context)
    {
        _context = context; 
    }
    public async Task<Product?> AddProductAsync(Product product)
    {
        _context.Products.Add(product);
        await _context.SaveChangesAsync();
        return product;
    }

    public async Task<bool> DeleteProductAsync(Guid productID)
    {
        Product? existingProduct = await _context.Products.FirstOrDefaultAsync<Product>(product => product.ProductID == productID);
        if (existingProduct == null)
        {
            return false;
        }

        _context.Products.Remove(existingProduct);
        await _context.SaveChangesAsync();
        return true;

    }

    public async Task<IEnumerable<Product>> GetAllProductsAsync()
    {
        return await _context.Products.ToListAsync();
    }

    public async Task<Product?> GetProductByConditionAsync(Expression<Func<Product, bool>> expression)
    {
        return await _context.Products.FirstOrDefaultAsync(expression);
    }

    public async Task<IEnumerable<Product>> GetProductsByConditionAsync(Expression<Func<Product, bool>> expression)
    {
        return await _context.Products.Where(expression).ToListAsync();
    }

    public async Task<Product?> UpdateProductAsync(Product product)
    {
        Product? existingProduct = await _context.Products.FirstOrDefaultAsync<Product>(x => x.ProductID == product.ProductID);
        if (existingProduct == null)
        {
            return null;
        }
        else
        {
            existingProduct.ProductName = product.ProductName;
            existingProduct.Category = product.Category;
            existingProduct.UnitPrice = product.UnitPrice;
            existingProduct.QuantityInStock = product.QuantityInStock;
            await _context.SaveChangesAsync();
            return existingProduct;
        }
    }
}
