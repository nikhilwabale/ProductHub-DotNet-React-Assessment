using Application.DTOs;
using Domain.Entities;

namespace Application.Interfaces;

public interface IProductService
{
    Task<PagedResponse<ProductResponse>> GetPagedAsync(int page, int size, string? search, CancellationToken ct);
    Task<ProductResponse> GetAsync(int id, CancellationToken ct);
    Task<ProductResponse> CreateAsync(CreateProductRequest request, string actor, CancellationToken ct);
    Task<ProductResponse> UpdateAsync(int id, UpdateProductRequest request, string actor, CancellationToken ct);
    Task DeleteAsync(int id, CancellationToken ct);
    Task<ItemResponse> AddItemAsync(int productId, CreateItemRequest request, CancellationToken ct);
    Task<ItemResponse> UpdateItemAsync(int productId, int itemId, UpdateItemRequest request, CancellationToken ct);
    Task DeleteItemAsync(int productId, int itemId, CancellationToken ct);
}

public interface IAuthService
{
    Task<TokenResponse> LoginAsync(LoginRequest request, CancellationToken ct);
    Task<TokenResponse> RefreshAsync(RefreshRequest request, CancellationToken ct);
    Task RevokeAsync(RefreshRequest request, CancellationToken ct);
}

public interface ITokenService
{
    TokenResponse CreateToken(AppUser user);
}
