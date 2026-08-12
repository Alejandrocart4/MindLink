using Microsoft.EntityFrameworkCore;
using MindLink.Application.Interfaces;
using MindLink.Application.Models;
using MindLink.Infrastructure.Persistence;

namespace MindLink.Infrastructure.Services;

public sealed class DashboardService(MindLinkDbContext database) : IDashboardService
{
    public async Task<DashboardSnapshot> GetSnapshotAsync(int userId, CancellationToken cancellationToken = default)
    {
        var user = await database.Users.FindAsync([userId], cancellationToken)
            ?? throw new InvalidOperationException("No se encontró el perfil local.");
        var activity = await database.ActivityLogs.OrderByDescending(x => x.OccurredAt).Take(4).ToListAsync(cancellationToken);

        return new DashboardSnapshot(user.FullName, 68, 76, 24, 3,
            activity.Select(x => new DashboardActivity(x.Title, x.Detail, x.Category, ToTimeLabel(x.OccurredAt))).ToList(),
            [
                new DashboardAlert("3 notas por clasificar", "Dales contexto para que puedas encontrarlas al redactar.", "Warning"),
                new DashboardAlert("Una sección sin respaldo", "Marco teórico aún no tiene una fuente vinculada.", "Attention")
            ]);
    }

    private static string ToTimeLabel(DateTime occurredAt)
    {
        var elapsed = DateTime.Now - occurredAt;
        return elapsed.TotalMinutes < 60 ? $"Hace {Math.Max(1, (int)elapsed.TotalMinutes)} min" :
            elapsed.TotalHours < 24 ? $"Hace {(int)elapsed.TotalHours} h" : occurredAt.ToString("dd MMM");
    }
}
