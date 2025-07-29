using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Glense.Server
{
    public class Video
    {
        public int videoID { get; set; }
        public User uploader { get; set; } = null!;
        public string title { get; set; } = string.Empty;
        public string description { get; set; } = string.Empty;
        public string videoUrl { get; set; } = string.Empty;
        public DateTime uploadedAt { get; set; }
        public int viewCount { get; set; }
        public int likeCount { get; set; }
        public int dislikeCount { get; set; }
        public string? thumbnailUrl { get; set; }
        public Category category { get; set; } = null!;
        public int categoryId { get; set; }
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<VideoLikes> VLikes { get; set; } = new List<VideoLikes>();
    }
}
