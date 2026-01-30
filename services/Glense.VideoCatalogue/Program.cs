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
var conn = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
           ?? builder.Configuration.GetConnectionString("DefaultConnection")
           ?? builder.Configuration.GetConnectionString("VideoCatalogue");
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

// Seed demo videos when using in-memory provider to ensure frontend shows content
using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<VideoCatalogueDbContext>();
        db.Database.EnsureCreated();
        if (!db.Videos.Any())
        {
            db.Videos.AddRange(
                new Videos
                {
                    Id = Guid.NewGuid(),
                    Title = "An Honest Review of Apple Intelligence... So Far",
                    Description = "Demo video seeded for development",
                    UploadDate = DateTime.UtcNow,
                    UploaderId = 1,
                    ThumbnailUrl = "https://i.ibb.co/G2L2Gwp/API-Course.png",
                    VideoUrl = "https://www.youtube.com/watch?v=haDjmBT9tu4",
                    ViewCount = 12345,
                    LikeCount = 123,
                    DislikeCount = 4
                },
                new Videos
                {
                    Id = Guid.NewGuid(),
                    Title = "Build and Deploy 5 JavaScript & React API Projects",
                    Description = "Demo playlist video",
                    UploadDate = DateTime.UtcNow,
                    UploaderId = 2,
                    ThumbnailUrl = "https://i.ibb.co/G2L2Gwp/API-Course.png",
                    VideoUrl = "https://www.youtube.com/watch?v=GDa8kZLNhJ4",
                    ViewCount = 54321,
                    LikeCount = 456,
                    DislikeCount = 10
                }
            );
            db.SaveChanges();
        }
    }
    catch
    {
        // ignore seeding errors in environments where DB isn't available
    }
}

app.Run();
