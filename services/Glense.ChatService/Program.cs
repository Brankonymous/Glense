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

// Configure CORS — restrict to known frontend origins
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? ["http://localhost:5173", "http://localhost:50653", "http://localhost:50654", "http://localhost:3000"];

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy => policy
        .WithOrigins(allowedOrigins)
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials());
});

// DbContext: allow in-memory fallback for local testing
var chatConn = builder.Configuration.GetConnectionString("DefaultConnection")
               ?? builder.Configuration["ConnectionStrings:DefaultConnection"];
var chatUseInMemory = (Environment.GetEnvironmentVariable("CHAT_USE_INMEMORY") ?? "false").ToLowerInvariant() == "true";
// Debug output to help diagnose env/connection issues during local dev
try { Console.WriteLine($"CHAT_USE_INMEMORY={chatUseInMemory}, DefaultConnection={(chatConn ?? "(null)")}"); } catch {}

if (chatUseInMemory)
{
    builder.Services.AddDbContext<ChatDbContext>(opt => opt.UseInMemoryDatabase("GlenseChat_InMemory"));
}
else if (string.IsNullOrEmpty(chatConn))
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
var jwtSecret = builder.Configuration["JwtSettings:SecretKey"]
    ?? Environment.GetEnvironmentVariable("JWT_SECRET_KEY")
    ?? throw new InvalidOperationException("JWT SecretKey not configured. Set JwtSettings:SecretKey or JWT_SECRET_KEY env var");

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

// Simple request logging for local development
app.Use(async (ctx, next) =>
{
    try { Console.WriteLine($"[{DateTime.UtcNow:O}] Incoming {ctx.Request.Method} {ctx.Request.Path} from {ctx.Connection.RemoteIpAddress}"); } catch {}
    await next();
    try { Console.WriteLine($"[{DateTime.UtcNow:O}] Response {ctx.Response.StatusCode} for {ctx.Request.Method} {ctx.Request.Path}"); } catch {}
});

// Enable dev CORS policy before auth so preflight succeeds
app.UseCors("AllowFrontend");
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
// Ensure database schema exists and seed demo data (skip in test environment)
if (!app.Environment.IsEnvironment("Testing"))
try
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetService<ChatDbContext>();
    if (db != null)
    {
        db.Database.EnsureCreated();

        // Seed demo chats if empty
        if (!db.Chats.Any())
        {
            var generalChat = new Glense.ChatService.Models.Chat
            {
                Id = Guid.NewGuid(),
                Topic = "General",
                CreatedAtUtc = DateTime.UtcNow.AddDays(-7)
            };
            var techChat = new Glense.ChatService.Models.Chat
            {
                Id = Guid.NewGuid(),
                Topic = "Tech Talk",
                CreatedAtUtc = DateTime.UtcNow.AddDays(-3)
            };
            var gamingChat = new Glense.ChatService.Models.Chat
            {
                Id = Guid.NewGuid(),
                Topic = "Gaming",
                CreatedAtUtc = DateTime.UtcNow.AddDays(-1)
            };
            db.Chats.AddRange(generalChat, techChat, gamingChat);

            db.Messages.AddRange(
                new Glense.ChatService.Models.Message { Id = Guid.NewGuid(), ChatId = generalChat.Id, UserId = Guid.Empty, Sender = Glense.ChatService.Models.MessageSender.User, Content = "Welcome to Glense!", CreatedAtUtc = DateTime.UtcNow.AddDays(-7) },
                new Glense.ChatService.Models.Message { Id = Guid.NewGuid(), ChatId = generalChat.Id, UserId = Guid.Empty, Sender = Glense.ChatService.Models.MessageSender.User, Content = "Hey everyone, glad to be here", CreatedAtUtc = DateTime.UtcNow.AddDays(-6) },
                new Glense.ChatService.Models.Message { Id = Guid.NewGuid(), ChatId = techChat.Id, UserId = Guid.Empty, Sender = Glense.ChatService.Models.MessageSender.User, Content = "Anyone tried .NET 8 yet?", CreatedAtUtc = DateTime.UtcNow.AddDays(-3) },
                new Glense.ChatService.Models.Message { Id = Guid.NewGuid(), ChatId = techChat.Id, UserId = Guid.Empty, Sender = Glense.ChatService.Models.MessageSender.User, Content = "Yeah, the performance improvements are solid", CreatedAtUtc = DateTime.UtcNow.AddDays(-2) },
                new Glense.ChatService.Models.Message { Id = Guid.NewGuid(), ChatId = gamingChat.Id, UserId = Guid.Empty, Sender = Glense.ChatService.Models.MessageSender.User, Content = "What games are you all playing?", CreatedAtUtc = DateTime.UtcNow.AddDays(-1) }
            );
            db.SaveChanges();
        }
    }
}
catch
{
    // no-op: don't crash the app if DB creation/check fails here
}

app.Run();

// Expose Program class for integration tests
public partial class Program { }
