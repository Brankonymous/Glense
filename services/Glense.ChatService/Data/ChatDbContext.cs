using Microsoft.EntityFrameworkCore;
using Glense.ChatService.Models;

namespace Glense.ChatService.Data;

public class ChatDbContext : DbContext
{
    public ChatDbContext(DbContextOptions<ChatDbContext> options) : base(options) { }

    public DbSet<Chat> Chats => Set<Chat>();
    public DbSet<Message> Messages => Set<Message>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Map to lowercase table and column names to match PostgreSQL schema
        modelBuilder.Entity<Chat>(entity =>
        {
            entity.ToTable("chats");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Topic).HasColumnName("topic");
            entity.Property(e => e.CreatedAtUtc).HasColumnName("created_at_utc");
        });

        modelBuilder.Entity<Message>(entity =>
        {
            entity.ToTable("messages");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ChatId).HasColumnName("chat_id");
            entity.Property(e => e.Sender).HasColumnName("sender");
            entity.Property(e => e.Content).HasColumnName("content");
            entity.Property(e => e.CreatedAtUtc).HasColumnName("created_at_utc");
        });

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ChatDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
