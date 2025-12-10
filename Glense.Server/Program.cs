using Microsoft.EntityFrameworkCore;
using DotNetEnv;
using DonationService.Data;

// Load environment variables from .env file
Env.Load();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register Donation Service dependencies
var donationDbConnectionString = Environment.GetEnvironmentVariable("DONATION_DB_CONNECTION_STRING")
    ?? builder.Configuration.GetConnectionString("DonationDb");

if (!string.IsNullOrEmpty(donationDbConnectionString))
{
    builder.Services.AddDbContext<DonationDbContext>(options =>
        options.UseNpgsql(donationDbConnectionString));
}
else
{
    builder.Services.AddDbContext<DonationDbContext>(options =>
        options.UseInMemoryDatabase("DonationDb"));
}

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapFallbackToFile("/index.html");

app.Run();
