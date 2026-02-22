using BusinessLogicLayer.DTOs;
using BusinessLogicLayer.ServiceContracts;
using BusinessLogicLayer.Validators;
using DataAccessLayer.Entities;
using FluentValidation;
using System.Linq.Expressions;

namespace ProductsMicroService.API.APiEndpoints;

// 43
public static class ProductAPIEndpoints
{
    public static IEndpointRouteBuilder MapProductAPIEndpoints(this IEndpointRouteBuilder app)
    {
        // GET /api/products
        app.MapGet("/api/products", async (IProductsService productsService) =>
        {
            List<ProductResponse?> products = await productsService.GetProductsAsync();
            return Results.Ok(products);
        });

        // GET /api/products/search/productid/{ProductID}
        app.MapGet("/api/products/search/productid/{ProductID}", async (IProductsService productsService, Guid ProductID) =>
        {
            ProductResponse? product = await productsService.GetProductByConditionAsync(product => product.ProductID == ProductID);
            if (product == null) //105
            { 
                return Results.NotFound();
            }
            return Results.Ok(product);
        });

        // GET /api/products/search/{SearchString}
        app.MapGet("/api/products/search/{SearchString}", async (string searchString, IProductsService productsService) =>
        {
            if (string.IsNullOrWhiteSpace(searchString))
            {
                return Results.BadRequest("Search string cannot be empty");
            }

            // Create expression to search in ProductName or Category
            string lowerSearchString = searchString.ToLower();
            Expression<Func<Product, bool>> searchExpression = (p => (p.ProductName != null &&
            p.ProductName.ToLower().Contains(lowerSearchString)) ||
                (p.Category != null && p.Category.ToString().ToLower().Contains(lowerSearchString)));

            var products = await productsService.GetProductsByConditionAsync(searchExpression);
            return Results.Ok(products);
        })
        .WithName("SearchProducts")
        //.WithTags("Products")
        .Produces<List<ProductResponse>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest);

        // POST /api/products
        app.MapPost("/api/products", async (ProductAddRequest productAddRequest, IProductsService productsService, IValidator<ProductAddRequest> productAddRequestValidator) =>
        {
            // Validation
            var validationResult = await productAddRequestValidator.ValidateAsync(productAddRequest);
            if (!validationResult.IsValid)
            {
                Dictionary<string, string[]> errors = validationResult.Errors.GroupBy(temp => temp.PropertyName).ToDictionary(grp => grp.Key, grp => grp.Select(err =>  err.ErrorMessage).ToArray());
                //string errors = string.Join(",", validationResult.Errors.Select(e => e.ErrorMessage));
                return Results.ValidationProblem(errors);
            }

            if (productAddRequest == null)
            {
                return Results.BadRequest("Product data cannot be null");
            }

            try
            {
                var createdProductResponse = await productsService.AddProductAsync(productAddRequest);
                return Results.Created($"/api/products/search/productid/{createdProductResponse.ProductID}", createdProductResponse);
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message);
            }
        })
        .WithName("CreateProduct")
        //.WithTags("Products")
        .Produces<ProductResponse>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status500InternalServerError);

        // PUT /api/products
        app.MapPut("/api/products/{productId}", async (Guid productId, ProductUpdateRequest productUpdateRequest, 
            IProductsService productsService, IValidator<ProductUpdateRequest> productUpdateRequestValidator) =>
        {
            // Validation
            var validationResult = await productUpdateRequestValidator.ValidateAsync(productUpdateRequest);
            if (!validationResult.IsValid)
            {
                Dictionary<string, string[]> errors = validationResult.Errors.GroupBy(temp => temp.PropertyName).ToDictionary(grp => grp.Key, grp => grp.Select(err => err.ErrorMessage).ToArray());
                //string errors = string.Join(",", validationResult.Errors.Select(e => e.ErrorMessage));
                return Results.ValidationProblem(errors);
            }

            if (productUpdateRequest == null)
            {
                return Results.BadRequest("Product data cannot be null");
            }

            if (productUpdateRequest.ProductID != productId)
            {
                return Results.BadRequest("Product ID in URL does not match Product ID in request body");
            }

            try
            {
                var updatedProductResponse = await productsService.UpdateProductAsync(productUpdateRequest);
                return Results.Ok(updatedProductResponse);
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(ex.Message);
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound($"Product with ID {productId} not found");
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message);
            }
        })
        .WithName("UpdateProduct")
        //.WithTags("Products")
        .Accepts<ProductUpdateRequest>("application/json")
        .Produces<ProductResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status500InternalServerError);

        // DELETE /api/products/{ProductID}
        app.MapDelete("/api/products/{productId}", async (Guid productId, IProductsService productsService) =>
        {
            try
            {
                bool deleted = await productsService.DeleteProductAsync(productId);

                if (deleted)
                {
                    //return Results.NoContent();
                    return Results.Ok(true);
                }
                else
                {
                    return Results.NotFound($"Product with ID {productId} not found");
                }
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message);
            }
        })
        .WithName("DeleteProduct")
        //.WithTags("Products")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status500InternalServerError);

        return app;

    }
}
