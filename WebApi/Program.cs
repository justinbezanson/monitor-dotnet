using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi; // v2.0.0 puts models here
using Scalar.AspNetCore;
using WebApi;
using WebApi.Responses;

var builder = WebApplication.CreateBuilder(args);

// --- Services Configuration ---

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

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

// Enable CORS at the very beginning
app.UseCors(policy => policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

// Optional: In Development, avoid redirects that can mess up CORS preflights
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

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