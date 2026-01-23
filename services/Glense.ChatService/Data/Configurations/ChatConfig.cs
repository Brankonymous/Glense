using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Glense.ChatService.Models;

namespace Glense.ChatService.Data.Configurations;

public class ChatConfig : IEntityTypeConfiguration<Chat>
{
    public void Configure(EntityTypeBuilder<Chat> builder)
    {
        builder.ToTable("chats");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Topic).HasMaxLength(200).IsRequired();
        builder.Property(x => x.CreatedAtUtc).IsRequired();
        builder.HasIndex(x => x.CreatedAtUtc).HasDatabaseName("IX_chats_created_at");
    }
}
