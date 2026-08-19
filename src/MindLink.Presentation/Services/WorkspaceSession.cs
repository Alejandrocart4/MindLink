using MindLink.Application.Interfaces;
using MindLink.Application.Models;
using MindLink.Presentation.ViewModels;

namespace MindLink.Presentation.Services;

/// <summary>
/// Mantiene un único estado de trabajo para todas las pantallas y lo guarda de forma local.
/// </summary>
public sealed class WorkspaceSession(IWorkspaceDataService dataService)
{
    public WorkspaceSnapshot? Snapshot { get; private set; }

    public event Action<WorkspaceSnapshot>? Changed;

    public void Initialize(WorkspaceSnapshot snapshot) => Snapshot = snapshot;

    public void Update(Func<WorkspaceSnapshot, WorkspaceSnapshot> change)
    {
        if (Snapshot is null) return;
        Snapshot = change(Snapshot);
        Changed?.Invoke(Snapshot);
        _ = dataService.SaveWorkspaceAsync(AppSession.UserId, Snapshot);
    }
}
