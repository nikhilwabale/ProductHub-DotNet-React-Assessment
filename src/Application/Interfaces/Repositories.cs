using Domain.Entities;

namespace Application.Interfaces;

public interface IProductRepository
{
    Task<(IReadOnlyCollection<Product> Items, int Total)> GetPagedAsync(int page, int size, string? search, CancellationToken ct);
    Task<Product?> GetByIdAsync(int id, bool tracking, CancellationToken ct);
    Task AddAsync(Product entity, CancellationToken ct);
    void Remove(Product entity);
}

public interface IItemRepository
{
    Task<Item?> GetByIdAsync(int productId, int id, CancellationToken ct);
    Task AddAsync(Item entity, CancellationToken ct);
    void Remove(Item entity);
}

public interface IUserRepository
{
    Task<AppUser?> FindByNameAsync(string userName, CancellationToken ct);
    Task<AppUser?> FindByRefreshTokenAsync(string token, CancellationToken ct);
}

public interface IUnitOfWork
{
    IProductRepository Products { get; }
    IItemRepository Items { get; }
    IUserRepository Users { get; }

    Task<int> SaveChangesAsync(CancellationToken ct);
}
