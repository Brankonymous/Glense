namespace Glense.Server
{
    public class VideoLikes
    {
        public Video video {  get; set; }
        public int videoId { get; set; }
        public User user { get; set; }
        public int userId { get; set; }
        public bool isLiked { get; set; }
    }
}
