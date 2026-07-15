namespace Application.DTOs;

public sealed record CreateProductRequest(string ProductName, string? Description, List<CreateItemRequest>? Items);
public sealed record UpdateProductRequest(string ProductName, string? Description);
public sealed record CreateItemRequest(int Quantity);
public sealed record UpdateItemRequest(int Quantity);
public sealed record ItemResponse(int Id, int ProductId, int Quantity);
public sealed record ProductResponse(int Id, string ProductName, string? Description, string CreatedBy, DateTime CreatedOn, string? ModifiedBy, DateTime? ModifiedOn, IReadOnlyCollection<ItemResponse> Items);
public sealed record PagedResponse<T>(IReadOnlyCollection<T> Items, int PageNumber, int PageSize, int TotalCount, int TotalPages);
public sealed record LoginRequest(string UserName, string Password);
public sealed record RefreshRequest(string RefreshToken);
public sealed record TokenResponse(string AccessToken, string RefreshToken, DateTime ExpiresAt, string Role, string UserName);
