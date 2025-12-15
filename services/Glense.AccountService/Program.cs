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

// Configure Database: prefer real Postgres when available, but allow an in-memory DB for local/dev testing
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var useInMemory = (Environment.GetEnvironmentVariable("ACCOUNT_USE_INMEMORY") ?? "false").ToLowerInvariant() == "true";

if (useInMemory || string.IsNullOrEmpty(connectionString) || connectionString.Contains("postgres_account"))
{
    // Use an in-memory database for quick local testing
    builder.Services.AddDbContext<AccountDbContext>(options =>
        options.UseInMemoryDatabase("GlenseAccount_InMemory"));
}
else
{
    builder.Services.AddDbContext<AccountDbContext>(options =>
        options.UseNpgsql(connectionString));
}

// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");

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
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
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

// Register services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<INotificationService, NotificationService>();

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
