import { useState, useEffect } from "react";
import { Stack, Typography, Avatar } from "@mui/material";
import { ThumbUpOutlined } from "@mui/icons-material";
import { getComments } from "../utils/videoApi";
import "../css/VideoComments.css";
import { stringToColor } from "../utils/constants";

function VideoComments({ videoId, id }) {
  const resolvedVideoId = videoId || id;
  const [comments, setComments] = useState(null);

  useEffect(() => {
    if (!resolvedVideoId) return;
    let mounted = true;
    getComments(resolvedVideoId)
      .then(data => { if (mounted) setComments(data); })
      .catch(() => { if (mounted) setComments([]); });
    return () => { mounted = false; };
  }, [resolvedVideoId]);

  if (comments === null)
    return (
      <Typography className="loading-text">
        Loading comments..
      </Typography>
    );

  if (comments.length === 0)
    return (
      <Typography className="loading-text">
        No comments yet. Be the first!
      </Typography>
    );

  return (
    <Stack className="video-comments-container">
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
            <Typography className="comment-likes">
              <ThumbUpOutlined className="comment-thumbs-up" />
              {Number(comment.likeCount).toLocaleString()}
            </Typography>
          </Stack>
        </Stack>
      ))}
    </Stack>
  );
}

export default VideoComments;
