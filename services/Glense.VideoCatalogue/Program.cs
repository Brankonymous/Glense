using Glense.VideoCatalogue.Data;
using Glense.VideoCatalogue.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using Glense.VideoCatalogue.Models;

var builder = WebApplication.CreateBuilder(args);

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

// Health checks
builder.Services.AddHealthChecks();

// Register storage implementation
builder.Services.AddSingleton<IVideoStorage, LocalFileVideoStorage>();
// Register Upload service
builder.Services.AddScoped<Upload>();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseCors("AllowAll");

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
