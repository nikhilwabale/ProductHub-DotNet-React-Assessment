using Application.DTOs;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using Domain.Exceptions;

namespace Application.Services;

public sealed class ProductService(IUnitOfWork uow, IMapper mapper) : IProductService
{
    public async Task<PagedResponse<ProductResponse>> GetPagedAsync(int page, int size, string? search, CancellationToken ct)
    {
        page = Math.Max(1, page);
        size = Math.Clamp(size, 1, 100);

        var (items, total) = await uow.Products.GetPagedAsync(page, size, search, ct);
        var totalPages = (int)Math.Ceiling(total / (double)size);

        return new PagedResponse<ProductResponse>(
            items.Select(mapper.Map<ProductResponse>).ToArray(),
            page,
            size,
            total,
            totalPages);
    }

    public async Task<ProductResponse> GetAsync(int id, CancellationToken ct)
    {
        var product = await uow.Products.GetByIdAsync(id, tracking: false, ct)
            ?? throw new NotFoundException($"Product {id} was not found.");

        return mapper.Map<ProductResponse>(product);
    }

    public async Task<ProductResponse> CreateAsync(CreateProductRequest r, string actor, CancellationToken ct)
    {
        var product = new Product
        {
            ProductName = r.ProductName.Trim(),
            Description = string.IsNullOrWhiteSpace(r.Description) ? null : r.Description.Trim(),
            CreatedBy = actor,
            CreatedOn = DateTime.UtcNow,
            Items = r.Items?.Select(i => new Item { Quantity = i.Quantity }).ToList() ?? new List<Item>()
        };

        await uow.Products.AddAsync(product, ct);
        await uow.SaveChangesAsync(ct);

        return mapper.Map<ProductResponse>(product);
    }

    public async Task<ProductResponse> UpdateAsync(int id, UpdateProductRequest r, string actor, CancellationToken ct)
    {
        var product = await uow.Products.GetByIdAsync(id, tracking: true, ct)
            ?? throw new NotFoundException($"Product {id} was not found.");

        product.ProductName = r.ProductName.Trim();
        product.Description = string.IsNullOrWhiteSpace(r.Description) ? null : r.Description.Trim();
        product.ModifiedBy = actor;
        product.ModifiedOn = DateTime.UtcNow;

        await uow.SaveChangesAsync(ct);

        return mapper.Map<ProductResponse>(product);
    }

    public async Task DeleteAsync(int id, CancellationToken ct)
    {
        var product = await uow.Products.GetByIdAsync(id, tracking: true, ct)
            ?? throw new NotFoundException($"Product {id} was not found.");

        uow.Products.Remove(product);
        await uow.SaveChangesAsync(ct);
    }

    public async Task<ItemResponse> AddItemAsync(int productId, CreateItemRequest r, CancellationToken ct)
    {
        _ = await uow.Products.GetByIdAsync(productId, tracking: false, ct)
            ?? throw new NotFoundException($"Product {productId} was not found.");

        var item = new Item { ProductId = productId, Quantity = r.Quantity };

        await uow.Items.AddAsync(item, ct);
        await uow.SaveChangesAsync(ct);

        return mapper.Map<ItemResponse>(item);
    }

    public async Task<ItemResponse> UpdateItemAsync(int productId, int id, UpdateItemRequest r, CancellationToken ct)
    {
        var item = await uow.Items.GetByIdAsync(productId, id, ct)
            ?? throw new NotFoundException($"Item {id} was not found.");

        item.Quantity = r.Quantity;
        await uow.SaveChangesAsync(ct);

        return mapper.Map<ItemResponse>(item);
    }

    public async Task DeleteItemAsync(int productId, int id, CancellationToken ct)
    {
        var item = await uow.Items.GetByIdAsync(productId, id, ct)
            ?? throw new NotFoundException($"Item {id} was not found.");

        uow.Items.Remove(item);
        await uow.SaveChangesAsync(ct);
    }
}
