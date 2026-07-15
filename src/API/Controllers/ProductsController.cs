using Application.DTOs;
using Application.Interfaces;
using Asp.Versioning;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[ApiVersion(1)]
[Route("api/v{version:apiVersion}/products")]
[Authorize]
public sealed class ProductsController(IProductService service) : ControllerBase
{
    [HttpGet]
    public Task<PagedResponse<ProductResponse>> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
        => service.GetPagedAsync(pageNumber, pageSize, search, ct);

    [HttpGet("{id:int}")]
    public Task<ProductResponse> Get(int id, CancellationToken ct)
        => service.GetAsync(id, ct);

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ProductResponse>> Create(
        CreateProductRequest r,
        [FromServices] IValidator<CreateProductRequest> v,
        CancellationToken ct)
    {
        await v.ValidateAndThrowAsync(r, ct);
        var p = await service.CreateAsync(r, User.Identity?.Name ?? "unknown", ct);
        return Created($"/api/v1/products/{p.Id}", p);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<ProductResponse> Update(
        int id,
        UpdateProductRequest r,
        [FromServices] IValidator<UpdateProductRequest> v,
        CancellationToken ct)
    {
        await v.ValidateAndThrowAsync(r, ct);
        return await service.UpdateAsync(id, r, User.Identity?.Name ?? "unknown", ct);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await service.DeleteAsync(id, ct);
        return NoContent();
    }

    [HttpPost("{productId:int}/items")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ItemResponse>> AddItem(
        int productId,
        CreateItemRequest r,
        [FromServices] IValidator<CreateItemRequest> v,
        CancellationToken ct)
    {
        await v.ValidateAndThrowAsync(r, ct);
        var i = await service.AddItemAsync(productId, r, ct);
        return Created($"/api/v1/products/{productId}/items/{i.Id}", i);
    }

    [HttpPut("{productId:int}/items/{itemId:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<ItemResponse> UpdateItem(
        int productId,
        int itemId,
        UpdateItemRequest r,
        [FromServices] IValidator<UpdateItemRequest> v,
        CancellationToken ct)
    {
        await v.ValidateAndThrowAsync(r, ct);
        return await service.UpdateItemAsync(productId, itemId, r, ct);
    }

    [HttpDelete("{productId:int}/items/{itemId:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteItem(int productId, int itemId, CancellationToken ct)
    {
        await service.DeleteItemAsync(productId, itemId, ct);
        return NoContent();
    }
}
