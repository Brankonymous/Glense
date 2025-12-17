using Glense.VideoCatalogue.Data;
using Glense.VideoCatalogue.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure DbContext (Postgres)
var conn = builder.Configuration.GetConnectionString("VideoCatalogue")
           ?? builder.Configuration.GetValue<string>("ConnectionStrings:VideoCatalogue");
if (!string.IsNullOrEmpty(conn))
{
    builder.Services.AddDbContext<VideoCatalogueDbContext>(options =>
        options.UseNpgsql(conn));
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

app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
