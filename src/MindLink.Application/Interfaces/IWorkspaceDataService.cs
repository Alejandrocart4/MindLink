using MindLink.Application.Models;

namespace MindLink.Application.Interfaces;

public interface IWorkspaceDataService
{
    Task<WorkspaceSnapshot> GetWorkspaceAsync(CancellationToken cancellationToken = default);
}
