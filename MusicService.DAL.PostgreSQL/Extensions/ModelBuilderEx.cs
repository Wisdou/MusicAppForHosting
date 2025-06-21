using Microsoft.EntityFrameworkCore;
using MusicService.Domain.Entities;

namespace MusicService.DAL.PostgreSQL.Extensions;

public static class ModelBuilderEx
{
    private const string FirstUserPwd = "Admin_123#";

    private static readonly User FirstUser = new()
    {
        Username = "FirstUser",
        PasswordHash = BCrypt.Net.BCrypt.HashPassword(FirstUserPwd),
        Role = Role.Admin,
        Id = Guid.Parse("a87e7235-132a-4e5c-9688-f55cafea3a36"),
        Email = "first_user@musicservice.com",
    };
    
    public static void Seed(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().HasData(FirstUser);
    }
}