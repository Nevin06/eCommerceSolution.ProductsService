using AutoMapper;
using BusinessLogicLayer.DTOs;
using DataAccessLayer.Entities;

namespace BusinessLogicLayer.Mappers;
// 40
public class ProductToProductResponseMappingProfile : Profile
{
    public ProductToProductResponseMappingProfile()
    {
        CreateMap<Product, ProductResponse>()
            .ForMember(dest => dest.ProductID, opt => opt.MapFrom(src => src.ProductID));
        CreateMap<Product, ProductResponse>()
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.ProductName));
        CreateMap<Product, ProductResponse>()
            .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Category));
        CreateMap<Product, ProductResponse>()
            .ForMember(dest => dest.UnitPrice, opt => opt.MapFrom(src => src.UnitPrice));
        CreateMap<Product, ProductResponse>()
            .ForMember(dest => dest.QuantityInStock, opt => opt.MapFrom(src => src.QuantityInStock));
        CreateMap<Product, ProductResponse>()
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.ProductName));
    }
}
