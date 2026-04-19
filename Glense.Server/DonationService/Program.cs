using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using DonationService.Consumers;
using DonationService.Data;
using DonationService.Services;

var builder = WebApplication.CreateBuilder(args);

// Register MVC controllers and API documentation services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Donation Microservice API", Version = "v1" });
});

// Database configuration
var connectionString = builder.Configuration.GetConnectionString("DonationDb")
    ?? Environment.GetEnvironmentVariable("DONATION_DB_CONNECTION_STRING");

if (!string.IsNullOrEmpty(connectionString))
{
    // Production: Use Neon PostgreSQL
    builder.Services.AddDbContext<DonationDbContext>(options =>
        options.UseNpgsql(connectionString));
}
else
{
    // Development fallback: In-memory database (no setup required)
    builder.Services.AddDbContext<DonationDbContext>(options =>
        options.UseInMemoryDatabase("DonationDb"));
    Console.WriteLine("[WARNING] No connection string found, using in-memory database");
}

// HttpClient for Account Service (kept for synchronous profile lookup/validation)
builder.Services.AddHttpClient("AccountService", client =>
{
    var serviceUrl = Environment.GetEnvironmentVariable("ACCOUNT_SERVICE_URL")
        ?? "http://localhost:5001";
    client.BaseAddress = new Uri(serviceUrl);
    client.Timeout = TimeSpan.FromSeconds(10);
});

builder.Services.AddScoped<IAccountServiceClient, AccountServiceClient>();

// Configure MassTransit with RabbitMQ
var rabbitHost = builder.Configuration["RabbitMQ:Host"] ?? "localhost";
var rabbitUser = builder.Configuration["RabbitMQ:Username"] ?? "guest";
var rabbitPass = builder.Configuration["RabbitMQ:Password"] ?? "guest";

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<UserRegisteredEventConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(rabbitHost, "/", h =>
        {
            h.Username(rabbitUser);
            h.Password(rabbitPass);
        });

        cfg.ConfigureEndpoints(context);
    });
});

// JWT Authentication
var jwtIssuer = builder.Configuration["JwtSettings:Issuer"] ?? "GlenseAccountService";
var jwtAudience = builder.Configuration["JwtSettings:Audience"] ?? "GlenseApp";
var jwtSecret = builder.Configuration["JwtSettings:SecretKey"]
    ?? Environment.GetEnvironmentVariable("JWT_SECRET_KEY")
    ?? throw new InvalidOperationException("JWT secret key is not configured");

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

// Health check endpoint for container orchestration
builder.Services.AddHealthChecks();

// Configure CORS — restrict to known frontend origins
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? ["http://localhost:5173", "http://localhost:50653", "http://localhost:50654", "http://localhost:3000"];

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

var app = builder.Build();

// CORS must be first middleware after exception handling
app.UseCors("AllowFrontend");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Donation Microservice API v1");
    });
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

// Auto-create database schema on startup
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<DonationDbContext>();
    dbContext.Database.EnsureCreated();
}

// Configure port from environment or default to 5100
var port = Environment.GetEnvironmentVariable("PORT") ?? "5100";
app.Urls.Add($"http://0.0.0.0:{port}");

Console.WriteLine($"Donation Microservice running on port {port}");
Console.WriteLine($"Swagger UI: http://localhost:{port}/");
Console.WriteLine($"Health check: http://localhost:{port}/health");

app.Run();

// Expose Program class for integration tests
public partial class Program { }
