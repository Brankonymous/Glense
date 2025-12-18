using Microsoft.EntityFrameworkCore;
using Glense.VideoCatalogue.Models;

namespace Glense.VideoCatalogue.Data
{
    public class VideoCatalogueDbContext : DbContext
    {
        public VideoCatalogueDbContext(DbContextOptions<VideoCatalogueDbContext> options) : base(options)
        {
        }

        public DbSet<Videos> Videos { get; set; } = null!;
        public DbSet<Playlists> Playlists { get; set; } = null!;
        public DbSet<PlaylistVideos> PlaylistVideos { get; set; } = null!;
        public DbSet<Subscriptions> Subscriptions { get; set; } = null!;
        public DbSet<VideoLikes> VideoLikes { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Videos>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK_Videos");
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.Title).HasColumnName("title").HasMaxLength(255).IsRequired();
                entity.Property(e => e.Description).HasColumnName("description");
                entity.Property(e => e.UploadDate).HasColumnName("upload_date");
                entity.Property(e => e.UploaderId).HasColumnName("uploader_id");
                entity.Property(e => e.ThumbnailUrl).HasColumnName("thumbnail_url").HasMaxLength(512);
                entity.Property(e => e.VideoUrl).HasColumnName("video_url").HasMaxLength(512).IsRequired();
                entity.Property(e => e.ViewCount).HasColumnName("view_count");
                entity.Property(e => e.LikeCount).HasColumnName("like_count");
                entity.Property(e => e.DislikeCount).HasColumnName("dislike_count");
            });

            modelBuilder.Entity<Playlists>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK_Playlists");
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(255).IsRequired();
                entity.Property(e => e.Description).HasColumnName("description");
                entity.Property(e => e.CreatorId).HasColumnName("creator_id");
                entity.Property(e => e.CreationDate).HasColumnName("creation_date");
            });

            modelBuilder.Entity<PlaylistVideos>(entity =>
            {
                entity.HasKey(e => new { e.PlaylistId, e.VideoId }).HasName("PK_PlaylistVideos");
                entity.Property(e => e.PlaylistId).HasColumnName("playlist_id");
                entity.Property(e => e.VideoId).HasColumnName("video_id");
                entity.HasOne(e => e.Playlist).WithMany(p => p.PlaylistVideos).HasForeignKey(e => e.PlaylistId).HasConstraintName("FK_PlaylistVideos_Playlists_playlist_id");
                entity.HasOne(e => e.Video).WithMany(v => v.PlaylistVideos).HasForeignKey(e => e.VideoId).HasConstraintName("FK_PlaylistVideos_Videos_video_id");
            });

            modelBuilder.Entity<Subscriptions>(entity =>
            {
                entity.HasKey(e => new { e.SubscriberId, e.SubscribedToId }).HasName("PK_Subscriptions");
                entity.Property(e => e.SubscriberId).HasColumnName("subscriber_id");
                entity.Property(e => e.SubscribedToId).HasColumnName("subscribed_to_id");
                entity.Property(e => e.SubscriptionDate).HasColumnName("subscription_date");
            });

            modelBuilder.Entity<VideoLikes>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.VideoId }).HasName("PK_VideoLikes");
                entity.Property(e => e.UserId).HasColumnName("user_id");
                entity.Property(e => e.VideoId).HasColumnName("video_id");
                entity.Property(e => e.IsLiked).HasColumnName("is_liked");
                entity.HasOne(e => e.Video).WithMany(v => v.VideoLikes).HasForeignKey(e => e.VideoId).HasConstraintName("FK_VideoLikes_Videos_video_id");
            });
        }
    }
}
