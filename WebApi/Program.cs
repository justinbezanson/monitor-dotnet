using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi; // v2.0.0 puts models here
using Scalar.AspNetCore;
using WebApi;
using WebApi.Responses;

var builder = WebApplication.CreateBuilder(args);

// --- Services Configuration ---

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlite(builder.Configuration.GetConnectionString("MonitorDotNetDb"));
});

builder.Services.AddIdentityCore<IdentityUser>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddApiEndpoints();

// Note: If you use .AddBearerToken, the scheme is "Identity.Bearer"
// If you use JWT, it's "Bearer". I've matched the docs below to "Bearer".
builder.Services.AddAuthentication().AddBearerToken(IdentityConstants.BearerScheme);
builder.Services.AddAuthorization();
builder.Services.AddControllers();

// Single AddOpenApi call with the Document Transformer
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        var scheme = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT"
        };

        // 1. Ensure Components is not null
        document.Components ??= new OpenApiComponents();

        // 2. Manual check to avoid the CS0019 Operator error
        if (document.Components.SecuritySchemes == null)
        {
            document.Components.SecuritySchemes = new Dictionary<string, IOpenApiSecurityScheme>();
        }
    
        // Add the scheme to the dictionary
        document.Components.SecuritySchemes.Add("Bearer", scheme);

        // 3. Create the Reference object
        var schemeReference = new OpenApiSecuritySchemeReference("Bearer", document);

        // 4. Ensure the global Security list is initialized
        document.Security ??= new List<OpenApiSecurityRequirement>();
    
        document.Security.Add(new OpenApiSecurityRequirement
        {
            [schemeReference] = new List<string>()
        });

        return Task.CompletedTask;
    });
});

var app = builder.Build();

// --- Middleware Pipeline ---

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    // Scalar is a great choice for .NET 10 / OpenAPI 3.1
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

// Order matters: Authentication must come before Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapIdentityApi<IdentityUser>();

/*app.MapGet("/up", () =>
    {
        var up = new GetUpResponse();
        return up;
    })
    .WithName("GetUp");*/

app.MapEndpoints();
app.Run();