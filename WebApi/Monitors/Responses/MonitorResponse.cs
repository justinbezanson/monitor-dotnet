namespace WebApi.Monitors.Responses;

public record MonitorResponse(
    Guid Id,
    string Name,
    string Url,
    int IntervalSeconds,
    bool IsEnabled,
    DateTime? LastCheckedAt,
    string CurrentStatus
);
