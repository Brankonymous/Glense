using Microsoft.AspNetCore.Connections;
using Microsoft.EntityFrameworkCore;

namespace Glense.Server
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<Category> Categories { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<CommentLikes> CommentLikes { get; set; }
        public DbSet<Conversation> Conversations { get; set; }
        public DbSet<Donation> Donations { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Video> Videos { get; set; }
        public DbSet<VideoLikes> VideoLikes { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Category>()
                .HasKey(c => new { c.categoryID });
            modelBuilder.Entity<Comment>()
                .HasKey(c => new { c.commentId });
            modelBuilder.Entity<CommentLikes>()
                .HasKey(cl => new { cl.commentId, cl.userId });
            modelBuilder.Entity<Conversation>()
                .HasKey(c => new { c.conversationId });
            modelBuilder.Entity<Donation>()
                .HasKey(d => new { d.donatorId, d.recipientId, d.donatedAt });
            modelBuilder.Entity<Message>()
                .HasKey(m => new { m.messageId });
            modelBuilder.Entity<Subscription>()
                .HasKey(s => new { s.subscriberId, s.subscribedToId });
            modelBuilder.Entity<User>()
                .HasKey(u => new { u.userId });
            modelBuilder.Entity<Video>()
                .HasKey(v => new { v.videoID });
            modelBuilder.Entity<VideoLikes>()
                .HasKey(vl => new { vl.videoId, vl.userId });
            modelBuilder.Entity<User>()
                .HasMany(u => u.Subscribers)
                .WithOne(s => s.subscriber)
                .HasForeignKey(s => s.subscriberId);
            modelBuilder.Entity<User>()
                .HasMany(u => u.Subscriptions)
                .WithOne(s => s.subscribedTo)
                .HasForeignKey(s => s.subscribedToId);
            modelBuilder.Entity<Conversation>()
                .HasMany(c => c.Messages)
                .WithOne(m => m.conversation)
                .HasForeignKey(m => m.conversationId);
            modelBuilder.Entity<User>()
                .HasMany(u => u.Donations)
                .WithOne(d => d.recipient)
                .HasForeignKey(d => d.recipientId);
            modelBuilder.Entity<User>()
                .HasMany(u => u.ConversationsStarted)
                .WithOne(c => c.user1)
                .HasForeignKey(c => c.user1Id);
            modelBuilder.Entity<User>()
                .HasMany(u => u.ConversationsInvited)
                .WithOne(c => c.user2)
                .HasForeignKey(c => c.user2Id);
            modelBuilder.Entity<User>()
                .HasMany(u => u.Comments)
                .WithOne(c => c.user)
                .HasForeignKey(c => c.userId);
            modelBuilder.Entity<Video>()
                .HasMany(v => v.Comments)
                .WithOne(c => c.video)
                .HasForeignKey(c => c.videoId);
            modelBuilder.Entity<Comment>()
                .HasMany(c => c.ChildComments)
                .WithOne(c => c.parentComment)
                .HasForeignKey(c => c.parentCommentId);
            modelBuilder.Entity<Comment>()
                .HasMany(c => c.CLikes)
                .WithOne(cl => cl.comment)
                .HasForeignKey(cl => cl.commentId);
            modelBuilder.Entity<User>()
                .HasMany(u => u.CLikes)
                .WithOne(cl => cl.user)
                .HasForeignKey(cl => cl.userId);
            modelBuilder.Entity<User>()
                .HasMany(u => u.VLikes)
                .WithOne(vl => vl.user)
                .HasForeignKey(vl => vl.userId);
            modelBuilder.Entity<Video>()
                .HasMany(u => u.VLikes)
                .WithOne(vl => vl.video)
                .HasForeignKey(vl => vl.videoId);
            modelBuilder.Entity<Category>()
                .HasMany(c => c.Videos)
                .WithOne(v => v.category)
                .HasForeignKey(v => v.categoryId);
        }
    }
}
