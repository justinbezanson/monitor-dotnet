using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebApi.Common;
using WebApi.Monitors.Responses;

namespace WebApi.Monitors.Endpoints;

public class Get : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) => app
        .MapGet("/api/monitors/{id:guid}", Handle)
        .WithTags("Monitors")
        .RequireAuthorization();

    private static async Task<Results<Ok<MonitorDetailResponse>, NotFound>> Handle(
        Guid id,
        [AsParameters] ChecksQuery query,
        ClaimsPrincipal user,
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

        var fromDate = DateTime.UtcNow.AddDays(-30);
        var checksQuery = database.MonitorChecks
            .Where(c => c.MonitorId == id && c.Timestamp >= fromDate);

        var totalCount = await checksQuery.CountAsync(ct);

        var successCount = await checksQuery.CountAsync(c => c.IsSuccess, ct);
        var avgResponseTime = await checksQuery.AverageAsync(c => (double?)c.ResponseTimeMs, ct) ?? 0;

        var checks = await checksQuery
            .OrderByDescending(c => c.Timestamp)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(c => new MonitorCheckResponse(
                c.Id,
                c.Timestamp,
                c.IsSuccess,
                c.StatusCode,
                c.ResponseTimeMs,
                c.ErrorMessage
            ))
            .ToListAsync(ct);

        var uptimePercentage = totalCount > 0 ? Math.Round((successCount / (double)totalCount) * 100, 1) : 100;

        var response = new MonitorDetailResponse(
            monitor.Id,
            monitor.Name,
            monitor.Url,
            monitor.Port,
            monitor.IntervalSeconds,
            monitor.IsEnabled,
            monitor.LastCheckedAt,
            monitor.CurrentStatus,
            uptimePercentage,
            Math.Round(avgResponseTime),
            new PaginatedResult<MonitorCheckResponse>(
                checks,
                query.Page,
                query.PageSize,
                totalCount
            )
        );

        return TypedResults.Ok(response);
    }

    public record ChecksQuery(
        int Page = 1,
        int PageSize = 20
    );
}
