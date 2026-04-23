namespace WebApi.Monitors.Responses;

public record MonitorDetailResponse(
    Guid Id,
    string Name,
    string Url,
    int? Port,
    int IntervalSeconds,
    bool IsEnabled,
    DateTime? LastCheckedAt,
    string CurrentStatus,
    IEnumerable<MonitorCheckResponse> RecentChecks
);
