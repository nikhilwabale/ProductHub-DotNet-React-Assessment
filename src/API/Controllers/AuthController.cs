using Application.DTOs;
using Application.Interfaces;
using Asp.Versioning;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[ApiVersion(1)]
[Route("api/v{version:apiVersion}/auth")]
public sealed class AuthController(IAuthService service) : ControllerBase
{
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<TokenResponse>> Login(
        LoginRequest r,
        [FromServices] IValidator<LoginRequest> v,
        CancellationToken ct)
    {
        await v.ValidateAndThrowAsync(r, ct);
        return Ok(await service.LoginAsync(r, ct));
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<ActionResult<TokenResponse>> Refresh(
        RefreshRequest r,
        [FromServices] IValidator<RefreshRequest> v,
        CancellationToken ct)
    {
        await v.ValidateAndThrowAsync(r, ct);
        return Ok(await service.RefreshAsync(r, ct));
    }

    [HttpPost("revoke")]
    [Authorize]
    public async Task<IActionResult> Revoke(
        RefreshRequest r,
        [FromServices] IValidator<RefreshRequest> v,
        CancellationToken ct)
    {
        await v.ValidateAndThrowAsync(r, ct);
        await service.RevokeAsync(r, ct);
        return NoContent();
    }
}
