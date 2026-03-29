import { useState, useEffect } from "react";
import { Stack, Typography, Avatar, TextField, Button } from "@mui/material";
import { ThumbUp, ThumbUpOutlined, ThumbDown, ThumbDownOutlined } from "@mui/icons-material";
import { getComments, postComment, likeComment } from "../utils/videoApi";
import { useAuth } from "../context/AuthContext";
import "../css/VideoComments.css";

function stringToColor(str) {
  let hash = 0;
  for (let i = 0; i < str.length; i++) {
    hash = str.charCodeAt(i) + ((hash << 5) - hash);
  }
  const colors = ['#e91e63', '#9c27b0', '#673ab7', '#3f51b5', '#2196f3', '#00bcd4', '#009688', '#4caf50', '#ff9800', '#ff5722'];
  return colors[Math.abs(hash) % colors.length];
}

function VideoComments({ videoId, id }) {
  const resolvedVideoId = videoId || id;
  const [comments, setComments] = useState(null);
  const [newComment, setNewComment] = useState("");
  const [submitting, setSubmitting] = useState(false);
  const [userLikes, setUserLikes] = useState({});
  const { user } = useAuth();

  useEffect(() => {
    if (!resolvedVideoId) return;
    let mounted = true;
    getComments(resolvedVideoId)
      .then(data => { if (mounted) setComments(data); })
      .catch(() => { if (mounted) setComments([]); });
    return () => { mounted = false; };
  }, [resolvedVideoId]);

  const handleSubmit = async () => {
    if (!newComment.trim() || submitting) return;
    setSubmitting(true);
    try {
      const created = await postComment(resolvedVideoId, newComment.trim());
      setComments(prev => [created, ...(prev || [])]);
      setNewComment("");
    } catch {
      alert("Failed to post comment");
    }
    setSubmitting(false);
  };

  const handleCommentLike = async (commentId, isLiked) => {
    if (!user) return;
    try {
      const resp = await likeComment(resolvedVideoId, commentId, isLiked);
      setComments(prev => prev.map(c =>
        c.id === commentId ? { ...c, likeCount: resp.likeCount, dislikeCount: resp.dislikeCount } : c
      ));
      setUserLikes(prev => ({ ...prev, [commentId]: isLiked }));
    } catch { /* ignore */ }
  };

  if (comments === null)
    return (
      <Typography className="loading-text">
        Loading comments..
      </Typography>
    );

  return (
    <Stack className="video-comments-container">
      {user && (
        <Stack direction="row" className="comment-form">
          <Avatar sx={{ bgcolor: stringToColor(user.username || "U"), width: 40, height: 40, fontSize: 16, flexShrink: 0 }}>
            {(user.username || "U").charAt(0).toUpperCase()}
          </Avatar>
          <TextField
            fullWidth
            size="small"
            placeholder="Add a comment..."
            value={newComment}
            onChange={(e) => setNewComment(e.target.value)}
            onKeyDown={(e) => { if (e.key === "Enter" && !e.shiftKey) { e.preventDefault(); handleSubmit(); } }}
            className="comment-input"
          />
          <Button
            variant="contained"
            size="small"
            disabled={!newComment.trim() || submitting}
            onClick={handleSubmit}
            className="comment-submit-btn"
          >
            {submitting ? "Posting..." : "Comment"}
          </Button>
        </Stack>
      )}

      {comments.length === 0 && (
        <Typography className="loading-text">
          No comments yet. Be the first!
        </Typography>
      )}

      {comments.map((comment) => (
        <Stack
          direction="row"
          className="comment-item"
          key={comment.id}
        >
          <Avatar
            sx={{
              bgcolor: stringToColor(comment.username),
              width: 40,
              height: 40,
              fontSize: 16,
              flexShrink: 0,
            }}
          >
            {comment.username?.charAt(0).toUpperCase()}
          </Avatar>
          <Stack direction="column" sx={{ ml: 1.5 }}>
            <Typography className="comment-name">
              {comment.username}
            </Typography>
            <Typography className="comment-text">
              {comment.content}
            </Typography>
            <Stack direction="row" className="comment-actions">
              <button className="comment-like-btn" onClick={() => handleCommentLike(comment.id, true)}>
                {userLikes[comment.id] === true ? <ThumbUp className="comment-icon active" /> : <ThumbUpOutlined className="comment-icon" />}
              </button>
              <span className="comment-count">{comment.likeCount || 0}</span>
              <button className="comment-like-btn" onClick={() => handleCommentLike(comment.id, false)}>
                {userLikes[comment.id] === false ? <ThumbDown className="comment-icon active" /> : <ThumbDownOutlined className="comment-icon" />}
              </button>
              <span className="comment-count">{comment.dislikeCount || 0}</span>
            </Stack>
          </Stack>
        </Stack>
      ))}
    </Stack>
  );
}

export default VideoComments;
