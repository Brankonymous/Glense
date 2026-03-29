using Glense.VideoCatalogue.Data;
using Glense.VideoCatalogue.Services;
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

// CORS for frontend dev
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy => policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
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

// HttpClient for Account Service (resolve uploader usernames)
builder.Services.AddHttpClient("AccountService", client =>
{
    var serviceUrl = Environment.GetEnvironmentVariable("ACCOUNT_SERVICE_URL")
        ?? "http://localhost:5001";
    client.BaseAddress = new Uri(serviceUrl);
    client.Timeout = TimeSpan.FromSeconds(10);
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

app.UseHttpsRedirection();

app.UseCors("AllowAll");

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
