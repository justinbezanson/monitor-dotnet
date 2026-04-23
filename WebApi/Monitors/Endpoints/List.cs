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

    private static async Task<Ok<IEnumerable<MonitorResponse>>> Handle(
        ClaimsPrincipal user,
        ApplicationDbContext database,
        CancellationToken ct)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var monitors = await database.Monitors
            .Where(m => m.UserId == userId)
            .Select(m => new MonitorResponse(
                m.Id,
                m.Name,
                m.Url,
                m.Port,
                m.IntervalSeconds,
                m.IsEnabled,
                m.LastCheckedAt,
                m.CurrentStatus
            ))
            .ToListAsync(ct);

        return TypedResults.Ok(monitors.AsEnumerable());
    }
}
