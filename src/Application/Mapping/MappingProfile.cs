using Application.DTOs;
using AutoMapper;
using Domain.Entities;

namespace Application.Mapping;

public sealed class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Item, ItemResponse>();
        CreateMap<Product, ProductResponse>();
    }
}
