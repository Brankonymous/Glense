using Microsoft.EntityFrameworkCore;
using DonationService.Data;

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

// Health check endpoint for container orchestration
builder.Services.AddHealthChecks();

// CORS policy - Allow frontend origins (both HTTP and HTTPS)
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.SetIsOriginAllowed(_ => true) // Allow any origin in development
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

var app = builder.Build();

// CORS must be first middleware after exception handling
app.UseCors();

// Swagger UI available at root path for easy API exploration
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Donation Microservice API v1");
    c.RoutePrefix = string.Empty;
});

app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

// Auto-create database schema in development
// In production, use proper migrations
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
