using BusinessLogicLayer.Mappers;
using BusinessLogicLayer.ServiceContracts;
using BusinessLogicLayer.Services;
using BusinessLogicLayer.Validators;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace BusinessLogicLayer;

public static class DependencyInjection
{
    public static IServiceCollection AddBusinessLogicLayer(this IServiceCollection services)
    {
        // TODO: Register your business logic services here into the IoC container
        services.AddAutoMapper(cfg => { },typeof(ProductAddRequestToProductMappingProfile).Assembly);
        services.AddAutoMapper(cfg => { }, typeof(ProductUpdateRequestToProductMappingProfile).Assembly); // no need to repeat
        services.AddAutoMapper(cfg => { }, typeof(ProductToProductResponseMappingProfile).Assembly);
        
        // Register FluentValidation validators
        services.AddValidatorsFromAssemblyContaining<ProductAddRequestValidator>();
        services.AddValidatorsFromAssemblyContaining<ProductUpdateRequestValidator>(); // no need to repeat // 45

        services.AddScoped<IProductsService, ProductsService>();
        return services;
    }
}
