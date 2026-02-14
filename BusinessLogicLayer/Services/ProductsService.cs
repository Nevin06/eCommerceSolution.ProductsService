using AutoMapper;
using BusinessLogicLayer.DTOs;
using BusinessLogicLayer.ServiceContracts;
using DataAccessLayer.Entities;
using DataAccessLayer.RepositoryContracts;
using FluentValidation;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;

namespace BusinessLogicLayer.Services;
// 41
public class ProductsService : IProductsService
{
    private readonly IProductsRepository _productsRepository;
    private readonly IMapper _mapper;
    private readonly IValidator<ProductAddRequest> _productAddRequestValidator;
    private readonly IValidator<ProductUpdateRequest> _productUpdateRequestValidator;
    public ProductsService(IProductsRepository productsRepository, IMapper mapper, IValidator<ProductAddRequest> productAddRequestValidator,
        IValidator<ProductUpdateRequest> productUpdateRequestValidator)
    {
        _productsRepository = productsRepository;
        _mapper = mapper;
        _productAddRequestValidator = productAddRequestValidator;
        _productUpdateRequestValidator = productUpdateRequestValidator;
    }
    public async Task<ProductResponse> AddProductAsync(ProductAddRequest productAddRequest)
    {
        if (productAddRequest == null)
        {
            throw new ArgumentNullException(nameof(productAddRequest));
        }

        // Validation
        var validationResult = await _productAddRequestValidator.ValidateAsync(productAddRequest);
        if (!validationResult.IsValid)
        {
            string errors = string.Join(",", validationResult.Errors.Select(e => e.ErrorMessage));
            throw new ArgumentException(errors);
        }
        Product product = _mapper.Map<Product>(productAddRequest);
        Product? addedProduct = await _productsRepository.AddProductAsync(product);
        return _mapper.Map<ProductResponse>(addedProduct);
    }

    public async Task<bool> DeleteProductAsync(Guid ProductID)
    {
        Product? existingProduct = await _productsRepository.GetProductByConditionAsync(product => product.ProductID == ProductID);
        if (existingProduct != null)
        {
            return await _productsRepository.DeleteProductAsync(ProductID);
        }
        else
        {
            return false;
        }
    }

    public async Task<ProductResponse?> GetProductAsync(string productId)
    {
        Product? product = await _productsRepository.GetProductByConditionAsync(product => product.ProductID.ToString() == productId);
        return _mapper.Map<ProductResponse>(product);
    }

    public async Task<ProductResponse?> GetProductByConditionAsync(Expression<Func<Product, bool>> expression)
    {
        Product? product = await _productsRepository.GetProductByConditionAsync(expression);
        if (product != null)
        {
            return _mapper.Map<ProductResponse>(product);
        }
        else
        {
            return null; 
        }
    }

    public async Task<List<ProductResponse?>> GetProductsAsync()
    {
        IEnumerable<Product> products = await _productsRepository.GetAllProductsAsync();
        List<Product> productList = products.ToList();
        return _mapper.Map<List<ProductResponse?>>(productList);
    }

    public async Task<List<ProductResponse?>> GetProductsByConditionAsync(Expression<Func<Product, bool>> expression)
    {
        IEnumerable<Product> products = await _productsRepository.GetProductsByConditionAsync(expression);
        List<Product> productList = products.ToList();
        return _mapper.Map<List<ProductResponse?>>(productList);
    }

    public async Task<ProductResponse> UpdateProductAsync(ProductUpdateRequest productUpdateRequest)
    {
        Product? existingProduct = await _productsRepository.GetProductByConditionAsync(product => product.ProductID ==
        productUpdateRequest.ProductID);

        if (productUpdateRequest == null)
        {
            throw new ArgumentNullException(nameof(productUpdateRequest));
        }
        else if (existingProduct != null)
        {

            // Validation
            var validationResult = await _productUpdateRequestValidator.ValidateAsync(productUpdateRequest);
            if (!validationResult.IsValid)
            {
                string errors = string.Join(",", validationResult.Errors.Select(e => e.ErrorMessage));
                throw new ArgumentException(errors);
            }

            Product product = _mapper.Map<Product>(productUpdateRequest);
            Product? updatedProduct = await _productsRepository.UpdateProductAsync(product);
            return _mapper.Map<ProductResponse>(updatedProduct);
        }
        else
        {
            throw new ArgumentException("Invalid Product ID");
        }
    }
}
