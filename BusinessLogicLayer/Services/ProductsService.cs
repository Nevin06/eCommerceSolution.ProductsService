using AutoMapper;
using BusinessLogicLayer.DTOs;
using BusinessLogicLayer.RabbitMQ;
using BusinessLogicLayer.ServiceContracts;
using DataAccessLayer.Entities;
using DataAccessLayer.RepositoryContracts;
using eCommerce.OrdersMicroService.BusinessLogicLayer.RabbitMQ;
using FluentValidation;
using Microsoft.Extensions.Logging;
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
    private readonly IRabbitMQPublisher _rabbitMQPublisher;
    private readonly ILogger<ProductsService> _logger;
    public ProductsService(IProductsRepository productsRepository, IMapper mapper, IValidator<ProductAddRequest> productAddRequestValidator,
        IValidator<ProductUpdateRequest> productUpdateRequestValidator, IRabbitMQPublisher rabbitMQPublisher, ILogger<ProductsService> logger)
    {
        _productsRepository = productsRepository;
        _mapper = mapper;
        _productAddRequestValidator = productAddRequestValidator;
        _productUpdateRequestValidator = productUpdateRequestValidator;
        _rabbitMQPublisher = rabbitMQPublisher;
        _logger = logger;
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
            //164
            //Publish message to RabbitMQ after deleting the product
            //return await _productsRepository.DeleteProductAsync(ProductID);
            bool isDeleted = await _productsRepository.DeleteProductAsync(ProductID);
            if (isDeleted) 
            { 
                _logger.LogInformation("Entered DeleteProductAsync RabbitMQ block");
                //string routingKey = "product.delete";
                var message = new ProductDeleteMessage(ProductID, existingProduct.ProductName);
                //await _rabbitMQPublisher.Publish(routingKey, message);

                //167
                var headers = new Dictionary<string, object>
                {
                    { "eventType", "product.delete" },
                    { "RowCount", 1 }
                    //{ "timestamp", DateTime.UtcNow }
                };
                await _rabbitMQPublisher.Publish(headers, message); //167
            }
            return isDeleted;
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

            //160
            bool isProductNameChanged = existingProduct.ProductName != product.ProductName;

            Product? updatedProduct = await _productsRepository.UpdateProductAsync(product);

            //160
            //Check if product name is updated, if yes then publish message to RabbitMQ
            if (existingProduct.ProductName == product.ProductName)
            {
                _logger.LogInformation("Entered UpdateProductAsync RabbitMQ block");
                //string message = $"Product with ID {product.ProductID} has been updated. Old Name: {existingProduct.ProductName}, New Name: {product.ProductName}";
                string routingKey = "product.update.name";
                var message = new ProductNameUpdateMessage(product.ProductID,
                    product.ProductName);
                //await _rabbitMQPublisher.Publish(routingKey,message);

                //167
                var headers = new Dictionary<string, object>
                {
                    { "eventType", "product.update" },
                    { "field", "name" },
                    {"RowCount", 1 }
                    //{ "timestamp", DateTime.UtcNow }
                };
                await _rabbitMQPublisher.Publish(headers, message); //167
            } //160

            return _mapper.Map<ProductResponse>(updatedProduct);
        }
        else
        {
            throw new ArgumentException("Invalid Product ID");
        }
    }
}
