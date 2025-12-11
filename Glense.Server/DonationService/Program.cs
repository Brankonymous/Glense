using Microsoft.EntityFrameworkCore;
using DonationService.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Donation Microservice API", Version = "v1" });
});

// Configure database (Neon PostgreSQL or in-memory for local dev)
var connectionString = builder.Configuration.GetConnectionString("DonationDb")
    ?? Environment.GetEnvironmentVariable("DONATION_DB_CONNECTION_STRING");

if (!string.IsNullOrEmpty(connectionString))
{
    builder.Services.AddDbContext<DonationDbContext>(options =>
        options.UseNpgsql(connectionString));
}
else
{
    // Fallback to in-memory database for local development
    builder.Services.AddDbContext<DonationDbContext>(options =>
        options.UseInMemoryDatabase("DonationDb"));
    Console.WriteLine("⚠️  No connection string found, using in-memory database");
}

// Add health checks
builder.Services.AddHealthChecks();

// Configure CORS for inter-service communication
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowMicroservices", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Donation Microservice API v1");
    c.RoutePrefix = string.Empty; // Swagger at root
});

app.UseCors("AllowMicroservices");

app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

// Apply migrations on startup (optional - can be disabled in production)
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<DonationDbContext>();
    dbContext.Database.EnsureCreated();
}

var port = Environment.GetEnvironmentVariable("PORT") ?? "5100";
app.Urls.Add($"http://0.0.0.0:{port}");

Console.WriteLine($"🚀 Donation Microservice running on port {port}");
Console.WriteLine($"📖 Swagger UI: http://localhost:{port}/");
Console.WriteLine($"❤️  Health check: http://localhost:{port}/health");

app.Run();

// Make Program accessible for integration tests
public partial class Program { }

