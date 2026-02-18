namespace ProfileGenie.Data.Entities;

/// <summary>
/// Represents a job entity in the database.
/// </summary>
public class Job
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ProfileUrl { get; set; } = string.Empty;
    public int Status { get; set; }
    public int Score { get; set; }
    public string? RawData { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
    
    public ICollection<HistoryLog> HistoryLogs { get; set; } = new List<HistoryLog>();
}
