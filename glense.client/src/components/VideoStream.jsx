import { useState } from "react";
import { Link } from "react-router-dom";
import ReactPlayer from "react-player";
import { Typography, Box, Stack } from "@mui/material";
import {
  CheckCircle,
  ThumbDownOutlined,
  ThumbUpOutlined,
} from "@mui/icons-material";

import { Videos, VideoComments } from ".";
import { videos, videoInfo } from "../utils/constants";

import "../css/VideoStream.css";

function VideoStream() {
  const [showMoreTags, setShowMoreTags] = useState(false);
  const [showMoreDesc, setShowMoreDesc] = useState(false);

  const id = 'haDjmBT9tu4';

  return (
    <Box className="video-stream-container">
      <Stack direction="row" className="video-stream-stack">
      <Box className="video-player-container">
        <Box className="video-player-box">
            <ReactPlayer 
              url={`https://www.youtube.com/watch?v=${id}`}
              controls
              width="100%"
              height="100%"
            />
            <Typography className="video-title">{videoInfo.title}</Typography>

            <Stack className="video-details">
              <Link to={`/channel/${videoInfo.channelId}`}>
              <Typography className="channel-title">
                  {videoInfo.channelTitle}
                  <CheckCircle className="check-circle-icon" />
                </Typography>
              </Link>

              <Typography className="like-dislike">
                <ThumbUpOutlined className="thumb-icon" />
                {Number(videoInfo.likeCount).toLocaleString()} {" | "}
                <ThumbDownOutlined className="thumb-icon" />
                {Number(videoInfo.dislikeCount).toLocaleString()}
              </Typography>
            </Stack>

            {/* Description */}
             <Box className="description-container">
              <Box className="description-details">
                <Typography>{Number(videoInfo.viewCount).toLocaleString()} views</Typography>
                <Typography className="publish-date">Published at {videoInfo.publishedAt}</Typography>


                {videoInfo.tags.map((tag, index) =>
                  videoInfo.tags.length > 10 ? (
                    <Typography
                      key={index}
                      className="tag"
                    >
                      {showMoreTags ? tag : `#${tag.substring(0, 5)}`}
                    </Typography>
                  ) : (
                    <Typography
                      key={tag}
                      className="tag"
                    >
                      #{videoInfo.tags}
                    </Typography>
                  )
                )}
                {videoInfo.tags.length > 10 && (
                  <button
                    className="toggle-tags-button"
                    onClick={() => setShowMoreTags(!showMoreTags)}
                  >
                    {showMoreTags ? "Show less" : "..."}
                  </button>
                )}

                <Typography className="description-text">
                  {showMoreDesc
                    ? videoInfo.description
                    : `${videoInfo.description.substring(0, 250)}`}
                  <button
                    className="toggle-description-button"
                    onClick={() => setShowMoreDesc(!showMoreDesc)}
                  >
                    {showMoreDesc ? "Show less" : "Show more"}
                  </button>
                </Typography>
              </Box>
            </Box>

            {/* Comments section */}
            <Typography className="comments-section-title">Comments</Typography>
            <VideoComments id={id} />
          </Box>
        </Box>

        <Box className="related-videos-container">
          <Videos videos={videos} direction={'column'} />
        </Box>
      </Stack>
    </Box>
  );
}

export default VideoStream;