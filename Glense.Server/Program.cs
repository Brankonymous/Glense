using DotNetEnv;

// Load environment variables from .env file (for local development)
Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Register MVC controllers and API documentation services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// HTTP client for communicating with the Donation microservice
// The microservice runs independently on port 5100
builder.Services.AddHttpClient("DonationService", client =>
{
    var serviceUrl = Environment.GetEnvironmentVariable("DONATION_SERVICE_URL")
        ?? "http://localhost:5100";
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

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// SPA fallback - serves index.html for client-side routing
app.MapFallbackToFile("/index.html");

app.Run();
