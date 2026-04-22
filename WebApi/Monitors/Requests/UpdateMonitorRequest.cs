namespace WebApi.Monitors.Requests;

public record UpdateMonitorRequest(
    string Name,
    string Url,
    int IntervalSeconds,
    bool IsEnabled
);
