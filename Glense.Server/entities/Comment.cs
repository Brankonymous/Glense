namespace Glense.Server
{
    public class Comment
    {
        public int commentId { get; set; }
        public Video video { get; set; } = null!;
        public int videoId { get; set; }
        public User user { get; set; } = null!;
        public int userId { get; set; }
        public string commentText { get; set; } = string.Empty;
        public DateTime createdAt { get; set; }
        public Comment? parentComment { get; set; }
        public int? parentCommentId { get; set; }
        public int commentLikes { get; set; }
        public ICollection<Comment> ChildComments { get; set; } = new List<Comment>();
        public ICollection<CommentLikes> CLikes { get; set; } = new List<CommentLikes>();
    }
}
