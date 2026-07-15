using Domain.Entities;
using Infrastructure.Data;
using Infrastructure.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Infrastructure.Tests;

public sealed class ProductRepositoryTests
{
    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task GetPagedAsync_ReturnsPagedResultsAndTotalCount()
    {
        await using var db = CreateContext();
        db.Products.AddRange(
            new Product { ProductName = "A", CreatedBy = "t", CreatedOn = DateTime.UtcNow },
            new Product { ProductName = "B", CreatedBy = "t", CreatedOn = DateTime.UtcNow });
        await db.SaveChangesAsync();

        var repository = new ProductRepository(db);
        var (items, total) = await repository.GetPagedAsync(1, 1, null, default);

        Assert.Single(items);
        Assert.Equal(2, total);
    }

    [Fact]
    public async Task GetPagedAsync_FiltersBySearchTerm()
    {
        await using var db = CreateContext();
        db.Products.AddRange(
            new Product { ProductName = "Laptop", CreatedBy = "t", CreatedOn = DateTime.UtcNow },
            new Product { ProductName = "Keyboard", CreatedBy = "t", CreatedOn = DateTime.UtcNow });
        await db.SaveChangesAsync();

        var repository = new ProductRepository(db);
        var (items, total) = await repository.GetPagedAsync(1, 10, "lap", default);

        Assert.Equal(1, total);
        Assert.Equal("Laptop", items.Single().ProductName);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenProductDoesNotExist()
    {
        await using var db = CreateContext();
        var repository = new ProductRepository(db);

        var result = await repository.GetByIdAsync(999, tracking: false, default);

        Assert.Null(result);
    }

    [Fact]
    public async Task AddAsync_ThenSaveChanges_PersistsProduct()
    {
        await using var db = CreateContext();
        var repository = new ProductRepository(db);
        var product = new Product { ProductName = "Monitor", CreatedBy = "t", CreatedOn = DateTime.UtcNow };

        await repository.AddAsync(product, default);
        await db.SaveChangesAsync();

        Assert.Equal(1, await db.Products.CountAsync());
    }
}
