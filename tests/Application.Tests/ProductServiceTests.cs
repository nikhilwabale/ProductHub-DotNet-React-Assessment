using Application.DTOs;
using Application.Interfaces;
using Application.Mapping;
using Application.Services;
using AutoMapper;
using Domain.Entities;
using Domain.Exceptions;
using Moq;
using Xunit;

namespace Application.Tests;

public sealed class ProductServiceTests
{
    private static readonly IMapper Mapper =
        new MapperConfiguration(c => c.AddProfile<MappingProfile>()).CreateMapper();

    private static (ProductService Sut, Mock<IProductRepository> Products, Mock<IItemRepository> Items, Mock<IUnitOfWork> Uow)
        CreateSut()
    {
        var products = new Mock<IProductRepository>();
        var items = new Mock<IItemRepository>();
        var uow = new Mock<IUnitOfWork>();
        uow.SetupGet(x => x.Products).Returns(products.Object);
        uow.SetupGet(x => x.Items).Returns(items.Object);

        var sut = new ProductService(uow.Object, Mapper);
        return (sut, products, items, uow);
    }

    [Fact]
    public async Task GetPagedAsync_Returns_MappedResultsAndTotal()
    {
        var (sut, products, _, _) = CreateSut();
        var entities = new List<Product>
        {
            new() { Id = 1, ProductName = "Test", CreatedBy = "u", CreatedOn = DateTime.UtcNow }
        };
        products
            .Setup(x => x.GetPagedAsync(1, 10, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync((entities, 1));

        var result = await sut.GetPagedAsync(1, 10, null, default);

        Assert.Equal(1, result.TotalCount);
        Assert.Single(result.Items);
        Assert.Equal("Test", result.Items.Single().ProductName);
    }

    [Fact]
    public async Task GetPagedAsync_ClampsPageSizeAndPageNumber()
    {
        var (sut, products, _, _) = CreateSut();
        products
            .Setup(x => x.GetPagedAsync(1, 100, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<Product>(), 0));

        await sut.GetPagedAsync(pageNumber: 0, pageSize: 1000, search: null, ct: default);

        products.Verify(x => x.GetPagedAsync(1, 100, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAsync_ThrowsNotFound_WhenProductMissing()
    {
        var (sut, products, _, _) = CreateSut();
        products
            .Setup(x => x.GetByIdAsync(99, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => sut.GetAsync(99, default));
    }

    [Fact]
    public async Task CreateAsync_PersistsProductWithAuditFields()
    {
        var (sut, products, _, uow) = CreateSut();
        Product? captured = null;
        products
            .Setup(x => x.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .Callback<Product, CancellationToken>((p, _) => captured = p)
            .Returns(Task.CompletedTask);

        var request = new CreateProductRequest("Monitor", "4K monitor", [new CreateItemRequest(5)]);
        var result = await sut.CreateAsync(request, actor: "tester", default);

        Assert.NotNull(captured);
        Assert.Equal("Monitor", captured!.ProductName);
        Assert.Equal("tester", captured.CreatedBy);
        Assert.Single(captured.Items);
        Assert.Equal("Monitor", result.ProductName);
        uow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ThrowsNotFound_WhenProductMissing()
    {
        var (sut, products, _, _) = CreateSut();
        products
            .Setup(x => x.GetByIdAsync(5, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => sut.DeleteAsync(5, default));
    }

    [Fact]
    public async Task DeleteAsync_RemovesProduct_WhenFound()
    {
        var (sut, products, _, uow) = CreateSut();
        var existing = new Product { Id = 5, ProductName = "Old", CreatedBy = "u", CreatedOn = DateTime.UtcNow };
        products
            .Setup(x => x.GetByIdAsync(5, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        await sut.DeleteAsync(5, default);

        products.Verify(x => x.Remove(existing), Times.Once);
        uow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AddItemAsync_ThrowsNotFound_WhenProductMissing()
    {
        var (sut, products, _, _) = CreateSut();
        products
            .Setup(x => x.GetByIdAsync(7, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => sut.AddItemAsync(7, new CreateItemRequest(3), default));
    }
}
