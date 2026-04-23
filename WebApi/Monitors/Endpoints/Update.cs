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

        var response = new MonitorResponse(
            monitor.Id,
            monitor.Name,
            monitor.Url,
            monitor.Port,
            monitor.IntervalSeconds,
            monitor.IsEnabled,
            monitor.LastCheckedAt,
            monitor.CurrentStatus
        );

        return TypedResults.Ok(response);
    }
}
