namespace MindLink.Domain.Entities;

public sealed class ActivityLog
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Detail { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public DateTime OccurredAt { get; set; }
}
