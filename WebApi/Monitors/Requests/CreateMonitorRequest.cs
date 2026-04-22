namespace WebApi.Monitors.Requests;

public record CreateMonitorRequest(
    string Name,
    string Url,
    int IntervalSeconds = 60
);
