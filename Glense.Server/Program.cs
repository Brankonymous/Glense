using DotNetEnv;

// Load environment variables from .env file (for local development)
Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Register MVC controllers and API documentation services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure CORS for frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
                "http://localhost:5173",   // Vite dev server
                "http://localhost:50653",  // Vite dev server (alt port)
                "http://localhost:50654",  // Vite dev server (alt port)
                "http://localhost:3000"    // Production/Docker
            )
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

// HTTP clients for communicating with microservices
// Account Service
builder.Services.AddHttpClient("AccountService", client =>
{
    var serviceUrl = Environment.GetEnvironmentVariable("ACCOUNT_SERVICE_URL")
        ?? "http://localhost:5001";
    client.BaseAddress = new Uri(serviceUrl);
});

// Donation Service
builder.Services.AddHttpClient("DonationService", client =>
{
    var serviceUrl = Environment.GetEnvironmentVariable("DONATION_SERVICE_URL")
        ?? "http://localhost:5100";
    client.BaseAddress = new Uri(serviceUrl);
});

// Video Catalogue Service (for future use)
builder.Services.AddHttpClient("VideoService", client =>
{
    var serviceUrl = Environment.GetEnvironmentVariable("VIDEO_SERVICE_URL")
        ?? "http://localhost:5002";
    client.BaseAddress = new Uri(serviceUrl);
});

var app = builder.Build();

// Serve static files from wwwroot (for SPA frontend)
app.UseDefaultFiles();
app.UseStaticFiles();

// Enable Swagger in development for API testing
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowFrontend");

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "gateway", timestamp = DateTime.UtcNow }));

// SPA fallback - serves index.html for client-side routing
app.MapFallbackToFile("/index.html");

app.Run();
