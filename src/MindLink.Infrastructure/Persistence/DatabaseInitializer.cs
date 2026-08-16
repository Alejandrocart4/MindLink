using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using MindLink.Domain.Entities;

namespace MindLink.Infrastructure.Persistence;

public sealed class DatabaseInitializer(MindLinkDbContext database)
{
    public async Task InitializeAsync()
    {
        await database.Database.MigrateAsync();

        // Remove the only data created by prior sample versions.
        var sampleEmails = new[] { "jonny@mindlink.local", "valeria@mindlink.local" };
        var sampleUsers = await database.Users.Where(user => sampleEmails.Contains(user.Email)).ToListAsync();
        database.Users.RemoveRange(sampleUsers);
        database.ActivityLogs.RemoveRange(await database.ActivityLogs.ToListAsync());

        var now = DateTime.Now;
        await EnsureAccountAsync("Cuenta Free", "free@mindlink.local", "MindLinkFree2026!", "Free", now);
        await EnsureAccountAsync("Jonny", "pro@mindlink.local", "MindLinkPro2026!", "Pro", now);
        await database.SaveChangesAsync();
    }

    private async Task EnsureAccountAsync(string fullName, string email, string password, string plan, DateTime now)
    {
        var user = await database.Users.SingleOrDefaultAsync(item => item.Email == email);
        if (user is null)
        {
            database.Users.Add(new LocalUser
            {
                FullName = fullName,
                Email = email,
                PasswordHash = HashPassword(password),
                Plan = plan,
                CreatedAt = now,
                UpdatedAt = now
            });
            return;
        }

        user.FullName = fullName;
        user.PasswordHash = HashPassword(password);
        user.Plan = plan;
        user.UpdatedAt = now;
    }

    private static string HashPassword(string password) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(password)));
}
