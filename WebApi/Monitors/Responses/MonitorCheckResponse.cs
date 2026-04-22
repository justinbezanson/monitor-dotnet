namespace WebApi.Monitors.Responses;

public record MonitorCheckResponse(
    Guid Id,
    DateTime Timestamp,
    bool IsSuccess,
    int? StatusCode,
    long ResponseTimeMs,
    string? ErrorMessage
);
