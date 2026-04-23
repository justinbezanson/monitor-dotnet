namespace WebApi.Monitors.Requests;

public class CreateMonitorRequest
{
    public string Name { get; set; } = null!;
    public string Url { get; set; } = null!;
    public int? Port { get; set; }
    public int IntervalSeconds { get; set; } = 60;
}
