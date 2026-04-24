using Microsoft.AspNetCore.Http.HttpResults;
using System.Security.Claims;
using WebApi.Common;
using WebApi.Monitors.Requests;
using WebApi.Monitors.Responses;

namespace WebApi.Monitors.Endpoints;

public class Create : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) => app
        .MapPost("/api/monitors", Handle)
        .WithTags("Monitors")
        .RequireAuthorization();

    private static async Task<Results<Created<MonitorResponse>, BadRequest<string>>> Handle(
        ClaimsPrincipal user,
        CreateMonitorRequest request,
        ApplicationDbContext database,
        CancellationToken ct)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var monitor = new Data.Entities.Monitor
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = request.Name,
            Url = request.Url,
            Port = request.Port,
            IntervalSeconds = request.IntervalSeconds,
            IsEnabled = true,
            CurrentStatus = "Pending"
        };

        database.Monitors.Add(monitor);
        await database.SaveChangesAsync(ct);

        var response = new MonitorResponse(
            monitor.Id,
            monitor.Name,
            monitor.Url,
            monitor.Port,
            monitor.IntervalSeconds,
            monitor.IsEnabled,
            monitor.LastCheckedAt,
            monitor.CurrentStatus,
            null
        );

        return TypedResults.Created($"/api/monitors/{monitor.Id}", response);
    }
}
