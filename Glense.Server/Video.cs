using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Glense.Server
{
    public class Video
    {
        public int videoID { get; set; }
        public User uploader { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public string videoUrl { get; set; }
        public DateTime uploadedAt { get; set; }
        public int viewCount { get; set; }
        public int likeCount { get; set; }
        public int dislikeCount { get; set; }
        public string thumbnailUrl { get; set; }
        public Category category { get; set; }
        public int categoryId { get; set; }
        public ICollection<Comment> Comments { get; set; }
        public ICollection<VideoLikes> VLikes { get; set; }


    }
}
