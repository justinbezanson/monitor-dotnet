namespace WebApi.Monitors.Responses;

public record MonitorResponse(
    Guid Id,
    string Name,
    string Url,
    int? Port,
    int IntervalSeconds,
    bool IsEnabled,
    DateTime? LastCheckedAt,
    string CurrentStatus,
    int? LastResponseTimeMs,
    double UptimePercentage30Days,
    double AvgResponseTime30Days
);
