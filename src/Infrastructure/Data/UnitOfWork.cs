using Application.Interfaces;
using Infrastructure.Data.Repositories;

namespace Infrastructure.Data;

public sealed class UnitOfWork(ApplicationDbContext db) : IUnitOfWork
{
    public IProductRepository Products { get; } = new ProductRepository(db);
    public IItemRepository Items { get; } = new ItemRepository(db);
    public IUserRepository Users { get; } = new UserRepository(db);

    public Task<int> SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
}
