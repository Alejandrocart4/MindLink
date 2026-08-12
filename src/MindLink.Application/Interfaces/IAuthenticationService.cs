using MindLink.Domain.Entities;

namespace MindLink.Application.Interfaces;

public interface IAuthenticationService
{
    Task<LocalUser?> SignInAsync(string email, string password, CancellationToken cancellationToken = default);
}
