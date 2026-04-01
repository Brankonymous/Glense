using System.Text;
using DotNetEnv;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Glense.AccountService.Consumers;
using Glense.AccountService.Data;
using Glense.AccountService.GrpcServices;
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

// Configure Kestrel with two ports: HTTP/1.1 for REST API, HTTP/2 for gRPC
var restPort = int.Parse(Environment.GetEnvironmentVariable("ACCOUNT_REST_PORT") ?? "5000");
var grpcPort = int.Parse(Environment.GetEnvironmentVariable("ACCOUNT_GRPC_PORT") ?? "5001");

builder.WebHost.ConfigureKestrel(options =>
{
    // REST API endpoint (HTTP/1.1)
    options.ListenAnyIP(restPort, listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http1;
    });
    // gRPC endpoint (HTTP/2 cleartext)
    options.ListenAnyIP(grpcPort, listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http2;
    });
});

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSingleton<InternalApiKeyInterceptor>();
builder.Services.AddGrpc(options =>
{
    options.Interceptors.Add<InternalApiKeyInterceptor>();
});

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

// Configure MassTransit with RabbitMQ
var rabbitHost = builder.Configuration["RabbitMQ:Host"] ?? "localhost";
var rabbitUser = builder.Configuration["RabbitMQ:Username"] ?? "guest";
var rabbitPass = builder.Configuration["RabbitMQ:Password"] ?? "guest";

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<DonationMadeEventConsumer>();
    x.AddConsumer<UserSubscribedEventConsumer>();

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

app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapGrpcService<AccountGrpcService>();

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "account", timestamp = DateTime.UtcNow }));

app.Run();

// Expose Program class for integration tests
public partial class Program { }
