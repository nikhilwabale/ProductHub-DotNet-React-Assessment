namespace Domain.Entities;

public sealed class RefreshToken
{
    public int Id { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresOn { get; set; }
    public DateTime CreatedOn { get; set; }
    public DateTime? RevokedOn { get; set; }
    public string? ReplacedByToken { get; set; }
    public int AppUserId { get; set; }
    public AppUser AppUser { get; set; } = null!;

    public bool IsActive => RevokedOn is null && ExpiresOn > DateTime.UtcNow;
}
