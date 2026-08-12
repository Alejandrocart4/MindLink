using MindLink.Application.Models;

namespace MindLink.Application.Interfaces;

public interface IDashboardService
{
    Task<DashboardSnapshot> GetSnapshotAsync(int userId, CancellationToken cancellationToken = default);
}
