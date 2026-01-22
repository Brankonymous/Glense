using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Glense.ChatService.Models;

namespace Glense.ChatService.Data.Configurations;

public class MessageConfig : IEntityTypeConfiguration<Message>
{
    public void Configure(EntityTypeBuilder<Message> builder)
    {
        builder.ToTable("messages");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Content).IsRequired();
        builder.Property(x => x.CreatedAtUtc).IsRequired();
        builder.Property(x => x.Sender).HasConversion<short>().IsRequired();
        builder.HasOne(x => x.Chat).WithMany(c => c.Messages).HasForeignKey(x => x.ChatId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(x => new { x.ChatId, x.CreatedAtUtc }).HasDatabaseName("IX_messages_chat_id_created_at");
    }
}
