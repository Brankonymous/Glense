import React from "react";
import { Stack, CardMedia, Typography } from "@mui/material";
import { ThumbUpOutlined } from "@mui/icons-material";

import { comments } from "../utils/constants";
import "../css/VideoComments.css";

function VideoComments({  }) {
  if (!comments)
    return (
      <Typography className="loading-text">
        Loading comments..
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
          <CardMedia
            image={comment.imageUrl}
            className="comment-avatar"
          />
          <Stack direction="column">
            <Typography className="comment-name">
              {comment.name}
            </Typography>
            <Typography className="comment-text">
              {comment.commentText}
            </Typography>
            <Typography className="comment-likes">
              <ThumbUpOutlined className ="comment-thumbs-up"/>
              {Number(comment.likeCount).toLocaleString()}
            </Typography>
          </Stack>
        </Stack>
      ))}
    </Stack>
  );
}

export default VideoComments;
