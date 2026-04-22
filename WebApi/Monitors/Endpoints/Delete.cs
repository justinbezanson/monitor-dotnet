using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebApi.Common;

namespace WebApi.Monitors.Endpoints;

public class Delete : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) => app
        .MapDelete("/api/monitors/{id:guid}", Handle)
        .WithTags("Monitors")
        .RequireAuthorization();

    private static async Task<Results<NoContent, NotFound>> Handle(
        Guid id,
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

        database.Monitors.Remove(monitor);
        await database.SaveChangesAsync(ct);

        return TypedResults.NoContent();
    }
}
