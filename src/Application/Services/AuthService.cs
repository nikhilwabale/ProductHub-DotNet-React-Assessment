using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Application.Services;

public sealed class AuthService(
    IUnitOfWork uow,
    IPasswordHasher<AppUser> hasher,
    ITokenService tokens) : IAuthService
{
    public async Task<TokenResponse> LoginAsync(LoginRequest r, CancellationToken ct)
    {
        var user = await uow.Users.FindByNameAsync(r.UserName, ct)
            ?? throw new UnauthorizedAccessException("Invalid credentials.");

        if (hasher.VerifyHashedPassword(user, user.PasswordHash, r.Password) == PasswordVerificationResult.Failed)
        {
            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        var result = tokens.CreateToken(user);
        user.RefreshTokens.Add(new RefreshToken
        {
            Token = result.RefreshToken,
            CreatedOn = DateTime.UtcNow,
            ExpiresOn = DateTime.UtcNow.AddDays(7)
        });

        await uow.SaveChangesAsync(ct);
        return result;
    }

    public async Task<TokenResponse> RefreshAsync(RefreshRequest r, CancellationToken ct)
    {
        var user = await uow.Users.FindByRefreshTokenAsync(r.RefreshToken, ct)
            ?? throw new UnauthorizedAccessException("Invalid refresh token.");

        var oldToken = user.RefreshTokens.Single(x => x.Token == r.RefreshToken);

        if (!oldToken.IsActive)
        {
            throw new UnauthorizedAccessException("Refresh token is expired or revoked.");
        }

        var result = tokens.CreateToken(user);
        oldToken.RevokedOn = DateTime.UtcNow;
        oldToken.ReplacedByToken = result.RefreshToken;

        user.RefreshTokens.Add(new RefreshToken
        {
            Token = result.RefreshToken,
            CreatedOn = DateTime.UtcNow,
            ExpiresOn = DateTime.UtcNow.AddDays(7)
        });

        await uow.SaveChangesAsync(ct);
        return result;
    }

    public async Task RevokeAsync(RefreshRequest r, CancellationToken ct)
    {
        var user = await uow.Users.FindByRefreshTokenAsync(r.RefreshToken, ct)
            ?? throw new UnauthorizedAccessException("Invalid refresh token.");

        var token = user.RefreshTokens.Single(x => x.Token == r.RefreshToken);
        token.RevokedOn = DateTime.UtcNow;

        await uow.SaveChangesAsync(ct);
    }
}
