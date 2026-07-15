using Application.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data.Repositories;

public sealed class ProductRepository(ApplicationDbContext db) : IProductRepository
{
    public async Task<(IReadOnlyCollection<Product> Items, int Total)> GetPagedAsync(
        int page, int size, string? search, CancellationToken ct)
    {
        var query = db.Products.AsNoTracking().Include(x => x.Items).AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(x => x.ProductName.Contains(search));
        }

        var total = await query.CountAsync(ct);
        var list = await query
            .OrderByDescending(x => x.Id)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync(ct);

        return (list, total);
    }

    public Task<Product?> GetByIdAsync(int id, bool tracking, CancellationToken ct)
    {
        var query = db.Products.Include(x => x.Items).AsQueryable();

        if (!tracking)
        {
            query = query.AsNoTracking();
        }

        return query.SingleOrDefaultAsync(x => x.Id == id, ct);
    }

    public Task AddAsync(Product entity, CancellationToken ct) => db.Products.AddAsync(entity, ct).AsTask();

    public void Remove(Product entity) => db.Products.Remove(entity);
}

public sealed class ItemRepository(ApplicationDbContext db) : IItemRepository
{
    public Task<Item?> GetByIdAsync(int productId, int id, CancellationToken ct)
        => db.Items.SingleOrDefaultAsync(x => x.ProductId == productId && x.Id == id, ct);

    public Task AddAsync(Item entity, CancellationToken ct) => db.Items.AddAsync(entity, ct).AsTask();

    public void Remove(Item entity) => db.Items.Remove(entity);
}

public sealed class UserRepository(ApplicationDbContext db) : IUserRepository
{
    public Task<AppUser?> FindByNameAsync(string userName, CancellationToken ct)
        => db.Users.Include(x => x.RefreshTokens).SingleOrDefaultAsync(x => x.UserName == userName, ct);

    public Task<AppUser?> FindByRefreshTokenAsync(string token, CancellationToken ct)
        => db.Users.Include(x => x.RefreshTokens)
            .SingleOrDefaultAsync(x => x.RefreshTokens.Any(y => y.Token == token), ct);
}
