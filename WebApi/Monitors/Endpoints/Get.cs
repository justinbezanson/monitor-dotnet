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
        ClaimsPrincipal user,
        ApplicationDbContext database,
        CancellationToken ct)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var monitor = await database.Monitors
            .Include(m => m.Checks.OrderByDescending(c => c.Timestamp).Take(20))
            .FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId, ct);

        if (monitor is null)
        {
            return TypedResults.NotFound();
        }

        var response = new MonitorDetailResponse(
            monitor.Id,
            monitor.Name,
            monitor.Url,
            monitor.IntervalSeconds,
            monitor.IsEnabled,
            monitor.LastCheckedAt,
            monitor.CurrentStatus,
            monitor.Checks.Select(c => new MonitorCheckResponse(
                c.Id,
                c.Timestamp,
                c.IsSuccess,
                c.StatusCode,
                c.ResponseTimeMs,
                c.ErrorMessage
            ))
        );

        return TypedResults.Ok(response);
    }
}
