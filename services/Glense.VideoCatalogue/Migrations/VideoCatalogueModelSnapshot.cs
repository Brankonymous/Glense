using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Glense.VideoCatalogue.Data;

#nullable disable

namespace Glense.VideoCatalogue.Migrations
{
    [DbContext(typeof(VideoCatalogueDbContext))]
    partial class VideoCatalogueModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "efcore")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            modelBuilder.Entity("Glense.VideoCatalogue.Models.Videos", b =>
            {
                b.Property<Guid>("Id").HasColumnName("id");
                b.Property<string>("Title").HasColumnName("title").IsRequired();
                b.Property<string>("Description").HasColumnName("description");
                b.Property<DateTime>("UploadDate").HasColumnName("upload_date");
                b.Property<int>("UploaderId").HasColumnName("uploader_id");
                b.Property<string>("ThumbnailUrl").HasColumnName("thumbnail_url");
                b.Property<string>("VideoUrl").HasColumnName("video_url").IsRequired();
                b.Property<int>("ViewCount").HasColumnName("view_count");
                b.Property<int>("LikeCount").HasColumnName("like_count");
                b.Property<int>("DislikeCount").HasColumnName("dislike_count");
                b.HasKey("Id");
                b.HasIndex("UploaderId");
                b.ToTable("Videos");
            });

            modelBuilder.Entity("Glense.VideoCatalogue.Models.Playlists", b =>
            {
                b.Property<Guid>("Id").HasColumnName("id");
                b.Property<string>("Name").HasColumnName("name").HasMaxLength(255).IsRequired();
                b.Property<string>("Description").HasColumnName("description");
                b.Property<int>("CreatorId").HasColumnName("creator_id");
                b.Property<DateTime>("CreationDate").HasColumnName("creation_date");
                b.HasKey("Id");
                b.HasIndex("CreatorId");
                b.ToTable("Playlists");
            });

            modelBuilder.Entity("Glense.VideoCatalogue.Models.PlaylistVideos", b =>
            {
                b.Property<Guid>("PlaylistId").HasColumnName("playlist_id");
                b.Property<Guid>("VideoId").HasColumnName("video_id");
                b.HasKey("PlaylistId", "VideoId");
                b.HasIndex("VideoId");
                b.ToTable("PlaylistVideos");
            });

            modelBuilder.Entity("Glense.VideoCatalogue.Models.Subscriptions", b =>
            {
                b.Property<int>("SubscriberId").HasColumnName("subscriber_id");
                b.Property<int>("SubscribedToId").HasColumnName("subscribed_to_id");
                b.Property<DateTime>("SubscriptionDate").HasColumnName("subscription_date");
                b.HasKey("SubscriberId", "SubscribedToId");
                b.HasIndex("SubscribedToId");
                b.ToTable("Subscriptions");
            });

            modelBuilder.Entity("Glense.VideoCatalogue.Models.VideoLikes", b =>
            {
                b.Property<int>("UserId").HasColumnName("user_id");
                b.Property<Guid>("VideoId").HasColumnName("video_id");
                b.Property<bool>("IsLiked").HasColumnName("is_liked");
                b.HasKey("UserId", "VideoId");
                b.HasIndex("VideoId");
                b.ToTable("VideoLikes");
            });
        }
    }
}
