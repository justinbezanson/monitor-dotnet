using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebApi.Common;
using WebApi.Monitors.Requests;
using WebApi.Monitors.Responses;

namespace WebApi.Monitors.Endpoints;

public class Update : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) => app
        .MapPut("/api/monitors/{id:guid}", Handle)
        .WithTags("Monitors")
        .RequireAuthorization();

    private static async Task<Results<Ok<MonitorResponse>, NotFound>> Handle(
        Guid id,
        ClaimsPrincipal user,
        UpdateMonitorRequest request,
        ApplicationDbContext database,
        CancellationToken ct)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var monitor = await database.Monitors
            .FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId, ct);

        if (monitor is null)
        {
            return TypedResults.NotFound();
        }

        monitor.Name = request.Name;
        monitor.Url = request.Url;
        monitor.Port = request.Port;
        monitor.IntervalSeconds = request.IntervalSeconds;
        monitor.IsEnabled = request.IsEnabled;

        await database.SaveChangesAsync(ct);

        var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);

        var lastResponseTime = await database.MonitorChecks
            .Where(c => c.MonitorId == monitor.Id)
            .OrderByDescending(c => c.Timestamp)
            .Select(c => (int?)c.ResponseTimeMs)
            .FirstOrDefaultAsync(ct);

        var totalChecks30d = await database.MonitorChecks
            .CountAsync(c => c.MonitorId == monitor.Id && c.Timestamp >= thirtyDaysAgo, ct);

        var successChecks30d = await database.MonitorChecks
            .CountAsync(c => c.MonitorId == monitor.Id && c.Timestamp >= thirtyDaysAgo && c.IsSuccess, ct);

        var avgResponseTime30d = await database.MonitorChecks
            .Where(c => c.MonitorId == monitor.Id && c.Timestamp >= thirtyDaysAgo)
            .AverageAsync(c => (double?)c.ResponseTimeMs, ct) ?? 0;

        var response = new MonitorResponse(
            monitor.Id,
            monitor.Name,
            monitor.Url,
            monitor.Port,
            monitor.IntervalSeconds,
            monitor.IsEnabled,
            monitor.LastCheckedAt,
            monitor.CurrentStatus,
            lastResponseTime,
            totalChecks30d > 0 ? Math.Round((successChecks30d / (double)totalChecks30d) * 100, 1) : 100,
            Math.Round(avgResponseTime30d)
        );

        return TypedResults.Ok(response);
    }
}
