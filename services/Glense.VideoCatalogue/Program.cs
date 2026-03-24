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

// Seed demo videos when using in-memory provider to ensure frontend shows content
using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<VideoCatalogueDbContext>();
        db.Database.EnsureCreated();
        if (!db.Videos.Any())
        {
            var seedVideos = new[]
            {
                ("An Honest Review of Apple Intelligence... So Far", "Reviewing every Apple Intelligence feature that's come out so far.", "https://i.ytimg.com/vi/haDjmBT9tu4/hqdefault.jpg", "https://www.youtube.com/watch?v=haDjmBT9tu4", 234175, 12300, 40),
                ("Build and Deploy 5 JavaScript & React API Projects", "Full course covering 5 real-world API projects.", "https://i.ibb.co/G2L2Gwp/API-Course.png", "https://www.youtube.com/watch?v=GDa8kZLNhJ4", 54321, 4560, 10),
                ("How I Built a $1M SaaS in 6 Months", "The story behind launching a profitable SaaS product.", "https://i.ytimg.com/vi/rIuMCxX8tJY/hqdefault.jpg", "https://www.youtube.com/watch?v=rIuMCxX8tJY", 187000, 9800, 120),
                ("Microservices Explained in 10 Minutes", "Quick overview of microservice architecture patterns.", "https://i.ytimg.com/vi/lTAcCNbJ7KE/hqdefault.jpg", "https://www.youtube.com/watch?v=lTAcCNbJ7KE", 320000, 15000, 200),
                ("Docker in 100 Seconds", "Everything you need to know about Docker, fast.", "https://i.ytimg.com/vi/Gjnup-PuquQ/hqdefault.jpg", "https://www.youtube.com/watch?v=Gjnup-PuquQ", 890000, 42000, 300),
                ("Why I Left Google", "My experience leaving a big tech job.", "https://i.ytimg.com/vi/sH4Tq0Zb4Is/hqdefault.jpg", "https://www.youtube.com/watch?v=sH4Tq0Zb4Is", 150000, 8700, 95),
                ("The Ultimate Guide to .NET 8", "Everything new in .NET 8 and how to use it.", "https://i.ytimg.com/vi/pFkBm_NnNqI/hqdefault.jpg", "https://www.youtube.com/watch?v=pFkBm_NnNqI", 98000, 5600, 30),
                ("React vs Angular vs Vue - Which One?", "Comparing the top 3 frontend frameworks in 2024.", "https://i.ytimg.com/vi/cuHDQhDhvPE/hqdefault.jpg", "https://www.youtube.com/watch?v=cuHDQhDhvPE", 445000, 21000, 1800),
            };

            var rng = new Random(42);
            foreach (var (title, desc, thumb, url, views, likes, dislikes) in seedVideos)
            {
                db.Videos.Add(new Videos
                {
                    Id = Guid.NewGuid(),
                    Title = title,
                    Description = desc,
                    UploadDate = DateTime.UtcNow.AddDays(-rng.Next(1, 60)),
                    UploaderId = Guid.Empty,
                    ThumbnailUrl = thumb,
                    VideoUrl = url,
                    ViewCount = views,
                    LikeCount = likes,
                    DislikeCount = dislikes
                });
            }
            db.SaveChanges();
        }
    }
    catch
    {
        // ignore seeding errors in environments where DB isn't available
    }
}

app.Run();
