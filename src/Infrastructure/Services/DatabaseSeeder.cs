using Domain.Entities;
using Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(ApplicationDbContext db, IPasswordHasher<AppUser> hasher)
    {
        await db.Database.EnsureCreatedAsync();

        await db.Database.ExecuteSqlRawAsync("""
            IF COL_LENGTH('dbo.Product', 'Description') IS NULL
                ALTER TABLE [dbo].[Product] ADD [Description] NVARCHAR(1000) NULL;
            """);

        if (!await db.Users.AnyAsync())
        {
            var admin = new AppUser { UserName = "admin@crn.local", Role = "Admin" };
            admin.PasswordHash = hasher.HashPassword(admin, "Admin@123");

            var user = new AppUser { UserName = "user@crn.local", Role = "User" };
            user.PasswordHash = hasher.HashPassword(user, "User@123");

            db.Users.AddRange(admin, user);
        }

        if (!await db.Products.AnyAsync())
        {
            db.Products.AddRange(
                new Product
                {
                    ProductName = "Laptop",
                    Description = "High-performance business laptop",
                    CreatedBy = "seed",
                    CreatedOn = DateTime.UtcNow,
                    Items = [new Item { Quantity = 10 }]
                },
                new Product
                {
                    ProductName = "Keyboard",
                    Description = "Ergonomic USB keyboard",
                    CreatedBy = "seed",
                    CreatedOn = DateTime.UtcNow,
                    Items = [new Item { Quantity = 25 }]
                });
        }

        await db.SaveChangesAsync();
    }
}
