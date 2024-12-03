namespace Glense.Server
{
    public class Comment
    {
        public int commentId {  get; set; }
        public Video video { get; set; }
        public int videoId { get; set; }
        public User user { get; set; }
        public int userId { get; set; }
        public string commentText { get; set; }
        public DateTime createdAt { get; set; }
        public Comment parentComment { get; set; }
        public int parentCommentId { get; set; }
        public int commentLikes { get; set; }
        public ICollection<Comment> ChildComments { get; set; }
        public ICollection<CommentLikes> CLikes { get; set; }

    }
}
