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
            .Where(c => c.MonitorId == id && c.Timestamp >= fromDate)
            .OrderByDescending(c => c.Timestamp);

        var totalCount = await checksQuery.CountAsync(ct);

        var checks = await checksQuery
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

        var response = new MonitorDetailResponse(
            monitor.Id,
            monitor.Name,
            monitor.Url,
            monitor.Port,
            monitor.IntervalSeconds,
            monitor.IsEnabled,
            monitor.LastCheckedAt,
            monitor.CurrentStatus,
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
