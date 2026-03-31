using Glense.VideoCatalogue.Data;
using Glense.VideoCatalogue.GrpcClients;
using Glense.VideoCatalogue.Protos;
using Grpc.Core.Interceptors;
using Glense.VideoCatalogue.Services;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Linq;
using System.Text;
using Glense.VideoCatalogue.Models;

var builder = WebApplication.CreateBuilder(args);

// Allow large video uploads (500 MB)
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 500 * 1024 * 1024;
});

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure CORS — restrict to known frontend origins
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? ["http://localhost:5173", "http://localhost:50653", "http://localhost:50654", "http://localhost:3000"];

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy => policy
        .WithOrigins(allowedOrigins)
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials());
});

// Configure DbContext (Postgres)
var conn = builder.Configuration.GetConnectionString("VideoCatalogue")
           ?? builder.Configuration.GetValue<string>("ConnectionStrings:VideoCatalogue");
if (!string.IsNullOrEmpty(conn))
{
    builder.Services.AddDbContext<VideoCatalogueDbContext>(options =>
        options.UseNpgsql(conn));
}
else
{
    // Register an in-memory database for development when no connection string provided
    builder.Services.AddDbContext<VideoCatalogueDbContext>(options =>
        options.UseInMemoryDatabase("VideoCatalogue"));
}

// gRPC client for Account Service (replaces HTTP-based username resolution)
var accountGrpcUrl = Environment.GetEnvironmentVariable("ACCOUNT_GRPC_URL")
    ?? builder.Configuration["AccountService:GrpcUrl"]
    ?? "http://localhost:5001";

builder.Services.AddSingleton<InternalApiKeyClientInterceptor>();
builder.Services.AddGrpcClient<AccountGrpc.AccountGrpcClient>(options =>
{
    options.Address = new Uri(accountGrpcUrl);
})
.AddInterceptor<InternalApiKeyClientInterceptor>();
builder.Services.AddScoped<IAccountGrpcClient, AccountGrpcClient>();

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

// Configure MassTransit with RabbitMQ
var rabbitHost = builder.Configuration["RabbitMQ:Host"] ?? "localhost";
var rabbitUser = builder.Configuration["RabbitMQ:Username"] ?? "guest";
var rabbitPass = builder.Configuration["RabbitMQ:Password"] ?? "guest";

builder.Services.AddMassTransit(x =>
{
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

builder.Services.AddMemoryCache();

// Health checks
builder.Services.AddHealthChecks();

// Register storage implementation
builder.Services.AddSingleton<IVideoStorage, LocalFileVideoStorage>();
// Register Upload service
builder.Services.AddScoped<Upload>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

// Ensure database schema exists
using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<VideoCatalogueDbContext>();
        db.Database.EnsureCreated();
    }
    catch
    {
        // ignore DB errors on startup
    }
}

app.Run();
