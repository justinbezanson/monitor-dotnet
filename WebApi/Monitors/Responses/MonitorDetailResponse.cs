namespace WebApi.Monitors.Responses;

public record MonitorDetailResponse(
    Guid Id,
    string Name,
    string Url,
    int IntervalSeconds,
    bool IsEnabled,
    DateTime? LastCheckedAt,
    string CurrentStatus,
    IEnumerable<MonitorCheckResponse> RecentChecks
);
