using Glense.ChatService.Services;
using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Glense.ChatService.Data;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;

// Load environment variables from a .env file if present in this directory or any parent directory
try {
    // Search upward from current dir for a .env file and load the first one found
    var dir = Directory.GetCurrentDirectory();
    while (!string.IsNullOrEmpty(dir))
    {
        var candidate = Path.Combine(dir, ".env");
        if (File.Exists(candidate))
        {
            Env.Load(candidate);
            break;
        }
        var parent = Directory.GetParent(dir);
        dir = parent?.FullName;
    }
}
catch
{
    // ignore errors reading .env
}

var builder = WebApplication.CreateBuilder(args);

// Allow overriding URLs from environment: use CHAT_URLS first, then ASPNETCORE_URLS
var chatUrls = Environment.GetEnvironmentVariable("CHAT_URLS") ?? Environment.GetEnvironmentVariable("ASPNETCORE_URLS");
if (!string.IsNullOrEmpty(chatUrls)) builder.WebHost.UseUrls(chatUrls);

// Configuration
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Glense.ChatService", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// DbContext: allow in-memory fallback for local testing
var chatConn = builder.Configuration.GetConnectionString("DefaultConnection")
               ?? builder.Configuration["ConnectionStrings:DefaultConnection"];
var chatUseInMemory = (Environment.GetEnvironmentVariable("CHAT_USE_INMEMORY") ?? "false").ToLowerInvariant() == "true";
if (chatUseInMemory || string.IsNullOrEmpty(chatConn))
{
    builder.Services.AddDbContext<ChatDbContext>(opt => opt.UseInMemoryDatabase("GlenseChat_InMemory"));
}
else
{
    builder.Services.AddDbContext<ChatDbContext>(opt => opt.UseNpgsql(chatConn));
}

// JWT settings
var jwtIssuer = builder.Configuration["JwtSettings:Issuer"] ?? "GlenseAccountService";
var jwtAudience = builder.Configuration["JwtSettings:Audience"] ?? "GlenseApp";
var jwtSecret = builder.Configuration["JwtSettings:SecretKey"] ?? "ChangeMeToA32CharSecret";

// Authentication (JWT)
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });

builder.Services.AddAuthorization();

// SignalR
builder.Services.AddSignalR();

// DI for services
builder.Services.AddScoped<IChatService, ChatService>();

var app = builder.Build();

// Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Exception handler to return RFC7807 ProblemDetails
app.UseExceptionHandler(a => a.Run(async context =>
{
    var feature = context.Features.Get<IExceptionHandlerFeature>();
    var ex = feature?.Error;
    var pd = new ProblemDetails
    {
        Title = "An unexpected error occurred.",
        Detail = app.Environment.IsDevelopment() ? ex?.ToString() : "An internal server error occurred.",
        Status = StatusCodes.Status500InternalServerError,
    };
    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
    context.Response.ContentType = "application/problem+json";
    await context.Response.WriteAsJsonAsync(pd);
}));

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// Serve static files (for the simple SignalR test page)
app.UseStaticFiles();

// Convert non-success status codes (if no body) into ProblemDetails
app.Use(async (ctx, next) =>
{
    await next();
    if (ctx.Response.HasStarted) return;
    if (ctx.Response.StatusCode >= 400)
    {
        // If the response already has a content-type, don't overwrite
        if (string.IsNullOrEmpty(ctx.Response.ContentType))
        {
            var pd = new ProblemDetails
            {
                Status = ctx.Response.StatusCode,
                Title = ReasonPhrases.GetReasonPhrase(ctx.Response.StatusCode)
            };
            ctx.Response.ContentType = "application/problem+json";
            await ctx.Response.WriteAsJsonAsync(pd);
        }
    }
});

app.MapGet("/health", () => Results.Json(new { status = "Healthy", service = "chat", timestamp = DateTime.UtcNow }));

app.MapControllers();

// Map SignalR hubs
app.MapHub<Glense.ChatService.Hubs.ChatHub>("/hubs/chat");
// Ensure relational DB schema exists when using Postgres (not in-memory)
try
{
    var startupChatUseInMemory = (Environment.GetEnvironmentVariable("CHAT_USE_INMEMORY") ?? "false").ToLowerInvariant() == "true";
    var startupChatConn = app.Configuration.GetConnectionString("DefaultConnection")
                        ?? app.Configuration["ConnectionStrings:DefaultConnection"];

    if (!startupChatUseInMemory && !string.IsNullOrEmpty(startupChatConn))
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetService<ChatDbContext>();
        if (db != null)
        {
            // EnsureCreated is safe for simple local testing; for production use migrations.
            db.Database.EnsureCreated();
        }
    }
}
catch
{
    // no-op: don't crash the app if DB creation/check fails here
}

app.Run();
