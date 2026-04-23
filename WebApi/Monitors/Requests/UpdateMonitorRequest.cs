namespace WebApi.Monitors.Requests;

public class UpdateMonitorRequest
{
    public string Name { get; set; } = null!;
    public string Url { get; set; } = null!;
    public int? Port { get; set; }
    public int IntervalSeconds { get; set; }
    public bool IsEnabled { get; set; }
}
