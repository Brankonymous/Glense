using Microsoft.EntityFrameworkCore;
using DonationService.Entities;

namespace DonationService.Data;

public class DonationDbContext : DbContext
{
    public DonationDbContext(DbContextOptions<DonationDbContext> options) : base(options)
    {
    }

    public DbSet<Wallet> Wallets { get; set; }
    public DbSet<Donation> Donations { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Wallet configuration
        modelBuilder.Entity<Wallet>(entity =>
        {
            entity.ToTable("wallets");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.Balance).HasColumnName("balance").HasPrecision(12, 2);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasIndex(e => e.UserId).IsUnique();
        });

        // Donation configuration
        modelBuilder.Entity<Donation>(entity =>
        {
            entity.ToTable("donations");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.DonorUserId).HasColumnName("donor_user_id");
            entity.Property(e => e.RecipientUserId).HasColumnName("recipient_user_id");
            entity.Property(e => e.Amount).HasColumnName("amount").HasPrecision(12, 2);
            entity.Property(e => e.Message).HasColumnName("message");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
        });
    }
}

