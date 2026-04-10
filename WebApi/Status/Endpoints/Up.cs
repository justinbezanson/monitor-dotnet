using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using WebApi.Common;

namespace WebApi.Status.Endpoints;

public class Up : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) => app
        .MapGet("/up", Handle)
        .WithSummary("required by once.com");
    
    public record Request();
    public record Response(string Message);
    
    private static async Task<Results<Ok<Response>, UnauthorizedHttpResult>> Handle(ApplicationDbContext database, CancellationToken cancellationToken)
    {
        var response = new Response("pong");
        return TypedResults.Ok(response);
    }
}