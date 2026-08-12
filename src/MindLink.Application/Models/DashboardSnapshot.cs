namespace MindLink.Application.Models;

public sealed record DashboardSnapshot(
    string UserName,
    int ProjectProgress,
    int EvidenceCoverage,
    int ConnectedIdeas,
    int PendingTasks,
    IReadOnlyList<DashboardActivity> RecentActivity,
    IReadOnlyList<DashboardAlert> Alerts);

public sealed record DashboardActivity(string Title, string Detail, string Category, string TimeLabel);
public sealed record DashboardAlert(string Title, string Detail, string Severity);
