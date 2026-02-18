namespace ProfileGenie.Shared.Models;

/// <summary>
/// Represents a job to be processed in the pipeline.
/// </summary>
public class Job
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ProfileUrl { get; set; } = string.Empty;
    public JobStatus Status { get; set; } = JobStatus.Pending;
    public int Score { get; set; }
    public string? RawData { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessedAt { get; set; }
}
