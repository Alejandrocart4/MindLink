using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using MindLink.Application.Interfaces;
using MindLink.Domain.Entities;
using MindLink.Infrastructure.Persistence;

namespace MindLink.Infrastructure.Services;

public sealed class AuthenticationService(MindLinkDbContext database) : IAuthenticationService
{
    public Task<LocalUser?> SignInAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var passwordHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(password)));
        return database.Users.SingleOrDefaultAsync(user => user.Email == normalizedEmail && user.PasswordHash == passwordHash, cancellationToken);
    }
}
