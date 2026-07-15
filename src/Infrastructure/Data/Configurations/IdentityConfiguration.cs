using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace Infrastructure.Data.Configurations;

public sealed class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
{
    public void Configure(EntityTypeBuilder<AppUser> builder)
    {
        builder.HasIndex(x => x.UserName).IsUnique();
        builder.Property(x => x.UserName).HasMaxLength(100);
        builder.Property(x => x.Role).HasMaxLength(30);
    }
}

public sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.HasIndex(x => x.Token).IsUnique();
        builder.Property(x => x.Token).HasMaxLength(200);
        builder.HasOne(x => x.AppUser).WithMany(x => x.RefreshTokens).HasForeignKey(x => x.AppUserId).OnDelete(DeleteBehavior.Cascade);
    }
}
