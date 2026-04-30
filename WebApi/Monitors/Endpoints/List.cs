using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebApi.Common;
using WebApi.Monitors.Responses;

namespace WebApi.Monitors.Endpoints;

public class List : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) => app
        .MapGet("/api/monitors", Handle)
        .WithTags("Monitors")
        .RequireAuthorization();

    private static async Task<Ok<PaginatedResult<MonitorResponse>>> Handle(
        [AsParameters] PaginationParams pagination,
        ClaimsPrincipal user,
        ApplicationDbContext database,
        CancellationToken ct)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);

        var query = database.Monitors
            .Where(m => m.UserId == userId)
            .Select(m => new
            {
                m.Id,
                m.Name,
                m.Url,
                m.Port,
                m.IntervalSeconds,
                m.IsEnabled,
                m.LastCheckedAt,
                m.CurrentStatus,
                LastResponseTimeMs = m.Checks
                    .OrderByDescending(c => c.Timestamp)
                    .Select(c => (int?)c.ResponseTimeMs)
                    .FirstOrDefault(),
                TotalChecks30d = m.Checks.Count(c => c.Timestamp >= thirtyDaysAgo),
                SuccessChecks30d = m.Checks.Count(c => c.Timestamp >= thirtyDaysAgo && c.IsSuccess),
                AvgResponseTime30d = m.Checks
                    .Where(c => c.Timestamp >= thirtyDaysAgo)
                    .Average(c => (double?)c.ResponseTimeMs) ?? 0,
            });

        var totalCount = await query.CountAsync(ct);

        var monitors = await query
            .OrderBy(m => m.Name)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToListAsync(ct);

        var results = monitors.Select(m => new MonitorResponse(
            m.Id,
            m.Name,
            m.Url,
            m.Port,
            m.IntervalSeconds,
            m.IsEnabled,
            m.LastCheckedAt,
            m.CurrentStatus,
            m.LastResponseTimeMs,
            m.TotalChecks30d > 0 ? Math.Round((m.SuccessChecks30d / (double)m.TotalChecks30d) * 100, 1) : 100,
            Math.Round(m.AvgResponseTime30d)
        ));

        return TypedResults.Ok(new PaginatedResult<MonitorResponse>(
            results,
            pagination.Page,
            pagination.PageSize,
            totalCount
        ));
    }

    public record PaginationParams(
        int Page = 1,
        int PageSize = 20
    );
}
