namespace WebApi.Data.Entities;

public class MonitorCheck
{
    public Guid Id { get; set; }
    public Guid MonitorId { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public bool IsSuccess { get; set; }
    public int? StatusCode { get; set; }
    public long ResponseTimeMs { get; set; }
    public string? ErrorMessage { get; set; }

    // Navigation properties
    public Monitor Monitor { get; set; } = null!;
}
