namespace ProfileGenie.Data.Entities;

/// <summary>
/// Represents a history log entry for job processing.
/// </summary>
public class HistoryLog
{
    public Guid Id { get; set; }
    public Guid JobId { get; set; }
    public Job Job { get; set; } = null!;
    public string Action { get; set; } = string.Empty;
    public string? Details { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
