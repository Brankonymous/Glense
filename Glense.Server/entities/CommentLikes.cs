namespace Glense.Server
{
    public class CommentLikes
    {
        public Comment comment { get; set; } = null!;
        public int commentId { get; set; }
        public User user { get; set; } = null!;
        public int userId { get; set; }
        public bool isLiked { get; set; }
    }
}
