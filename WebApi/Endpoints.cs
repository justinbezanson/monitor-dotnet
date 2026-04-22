using WebApi.Common;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi;
using WebApi.Status.Endpoints;
using WebApi.Monitors.Endpoints;

namespace WebApi;

public static class Endpoints
{
    public static void MapEndpoints(this WebApplication app)
    {
        // Note: You might want to call MapStatusEndpoints here 
        // if it's not being called elsewhere.
        var endpoints = app.MapGroup("");
        endpoints.MapStatusEndpoints();
        endpoints.MapMonitorEndpoints();

        /*endpoints.MapAuthenticationEndpoints();
        endpoints.MapPostEndpoints();
        endpoints.MapCommentEndpoints();
        endpoints.MapUserEndpoints();*/
    }
    
    private static void MapStatusEndpoints(this IEndpointRouteBuilder app)
    {
        var endpoints = app.MapGroup("")
            .WithTags("Status");
            
        endpoints.MapPublicGroup()
            .MapEndpoint<Up>();
    }

    private static void MapMonitorEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapEndpoint<List>();
        app.MapEndpoint<Create>();
        app.MapEndpoint<Get>();
        app.MapEndpoint<Update>();
        app.MapEndpoint<Delete>();
    }
    
    private static RouteGroupBuilder MapPublicGroup(this IEndpointRouteBuilder app, string? prefix = null)
    {
        return app.MapGroup(prefix ?? string.Empty)
            .AllowAnonymous();
    }

    private static RouteGroupBuilder MapAuthorizedGroup(this IEndpointRouteBuilder app, string? prefix = null)
    {
        // CLEANUP: We removed the .WithOpenApi block.
        // The global Transformer in Program.cs handles the "Lock" icon in Scalar/Swagger.
        return app.MapGroup(prefix ?? string.Empty)
            .RequireAuthorization();
    }
    
    private static IEndpointRouteBuilder MapEndpoint<TEndpoint>(this IEndpointRouteBuilder app) where TEndpoint : IEndpoint
    {
        TEndpoint.Map(app);
        return app;
    }
}