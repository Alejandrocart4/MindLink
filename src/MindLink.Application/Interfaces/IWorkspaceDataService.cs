using MindLink.Application.Models;

namespace MindLink.Application.Interfaces;

public interface IWorkspaceDataService
{
    Task<WorkspaceSnapshot> GetWorkspaceAsync(int userId, CancellationToken cancellationToken = default);
    Task SaveWorkspaceAsync(int userId, WorkspaceSnapshot workspace, CancellationToken cancellationToken = default);
}
