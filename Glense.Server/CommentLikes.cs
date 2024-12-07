namespace Glense.Server
{
    public class CommentLikes
    {
        public Comment comment { get; set; }
        public int commentId { get; set; }
        public User user { get; set; }
        public int userId { get; set; }
        bool isLiked { get; set; }
    }
}
