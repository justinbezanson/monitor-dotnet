using Microsoft.AspNetCore.Identity;

namespace WebApi.Data.Entities;

public class Monitor
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Url { get; set; } = null!;
    public int IntervalSeconds { get; set; } = 60;
    public bool IsEnabled { get; set; } = true;
    public DateTime? LastCheckedAt { get; set; }
    public string CurrentStatus { get; set; } = "Pending";

    // Navigation properties
    public IdentityUser User { get; set; } = null!;
    public ICollection<MonitorCheck> Checks { get; set; } = new List<MonitorCheck>();
}
