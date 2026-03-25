using System.Text;
using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Glense.AccountService.Data;
using Glense.AccountService.Services;

// Load environment variables from a .env file if present in this directory or any parent directory
try {
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
catch { }

var builder = WebApplication.CreateBuilder(args);

// Allow overriding URLs from environment: use ACCOUNT_URLS first, then ASPNETCORE_URLS
var accountUrls = Environment.GetEnvironmentVariable("ACCOUNT_URLS") ?? Environment.GetEnvironmentVariable("ASPNETCORE_URLS");
if (!string.IsNullOrEmpty(accountUrls)) builder.WebHost.UseUrls(accountUrls);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configure Swagger with JWT support
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Glense Account Service API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token.",
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

// Configure PostgreSQL Database
// Priority: Environment variable > appsettings.json
var connectionString = Environment.GetEnvironmentVariable("ACCOUNT_DB_CONNECTION_STRING")
    ?? builder.Configuration.GetConnectionString("DefaultConnection");

if (!string.IsNullOrEmpty(connectionString))
{
    builder.Services.AddDbContext<AccountDbContext>(options =>
        options.UseNpgsql(connectionString));
}
else
{
    // Development fallback: In-memory database (no setup required)
    builder.Services.AddDbContext<AccountDbContext>(options =>
        options.UseInMemoryDatabase("GlenseAccount_InMemory"));
    Console.WriteLine("[WARNING] No connection string found, using in-memory database");
}

// Configure JWT Authentication
// Priority: Environment variables > appsettings.json
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY")
    ?? jwtSettings["SecretKey"]
    ?? throw new InvalidOperationException("JWT SecretKey not configured. Set JWT_SECRET_KEY env var or configure in appsettings.json");
var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? jwtSettings["Issuer"] ?? "GlenseAccountService";
var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? jwtSettings["Audience"] ?? "GlenseApp";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
    };
});

builder.Services.AddAuthorization();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// HttpClient for Donation Service
builder.Services.AddHttpClient("DonationService", client =>
{
    var serviceUrl = Environment.GetEnvironmentVariable("DONATION_SERVICE_URL")
        ?? "http://localhost:5100";
    client.BaseAddress = new Uri(serviceUrl);
    client.Timeout = TimeSpan.FromSeconds(10);
});

// Register services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IWalletServiceClient, WalletServiceClient>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Glense Account Service API v1");
    });
}

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "account", timestamp = DateTime.UtcNow }));

app.Run();
