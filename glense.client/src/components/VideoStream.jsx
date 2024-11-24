import { useState, useEffect } from "react";
import { Link, useParams } from "react-router-dom";
import ReactPlayer from "react-player";
import { Typography, Box, Stack } from "@mui/material";
import {
  CheckCircle,
  ThumbDownOutlined,
  ThumbUpOutlined,
} from "@mui/icons-material";

import { Videos } from ".";
import { videos } from "../utils/constants";

import "../css/VideoStream.css";

function VideoStream() {
  const [videoStream, setvideoStream] = useState(null);

  const [showMoreTags, setShowMoreTags] = useState(false);
  const [showMoreDesc, setShowMoreDesc] = useState(false);

  // const { id } = useParams();
  const id = 'haDjmBT9tu4';

  const publishedAt = 'Nov 22, 2024';
  const channelId = 'mkbhd';
  const title = 'An Honest Review of Apple Intelligence... So Far';
  const description = 'Reviewing every Apple Intelligence feature that\'s come out so far... \n\n Get both the MKBHD Carry-on & Commuter backpack together at http://ridge.com/MKBHD for 30% off\nReviewing every Apple Intelligence feature that\'s come out so far... \n\n Get both the MKBHD Carry-on & Commuter backpack together at http://ridge.com/MKBHD for 30% off\nReviewing every Apple Intelligence feature that\'s come out so far... \n\n Get both the MKBHD Carry-on & Commuter backpack together at http://ridge.com/MKBHD for 30% off\nReviewing every Apple Intelligence feature that\'s come out so far... \n\n Get both the MKBHD Carry-on & Commuter backpack together at http://ridge.com/MKBHD for 30% off\n';
  const channelTitle = 'Marques Brownlee';
  const tags = ['Apple'];
  const viewCount = 2364175;
  const likeCount = 123456;

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
            <Typography className="video-title">{title}</Typography>

            <Stack className="video-details">
              <Link to={`/channel/${channelId}`}>
              <Typography className="channel-title">
                  {channelTitle}
                  <CheckCircle className="check-circle-icon" />
                </Typography>
              </Link>

              <Typography className="like-dislike">
                <ThumbUpOutlined className="thumb-icon" />
                {Number(likeCount).toLocaleString()} {" | "}
                <ThumbDownOutlined className="thumb-icon" />
              </Typography>
            </Stack>

            {/* Description */}
             <Box className="description-container">
              <Box className="description-details">
                <Typography>{Number(viewCount).toLocaleString()} views</Typography>
                <Typography className="publish-date">Published at {publishedAt}</Typography>


                {tags.map((tag) =>
                  tags.length > 10 ? (
                    <Typography
                      key={index}
                      className="tag"
                    >
                      {showMoreTags ? tag : `#${tag.substring(0, 5)}`}
                    </Typography>
                  ) : (
                    <Typography
                      className="tag"
                    >
                      #{tags}
                    </Typography>
                  )
                )}
                {tags.length > 10 && (
                  <button
                    className="toggle-tags-button"
                    onClick={() => setShowMoreTags(!showMoreTags)}
                  >
                    {showMoreTags ? "Show less" : "..."}
                  </button>
                )}

                <Typography className="description-text">
                  {showMoreDesc
                    ? description
                    : `${description.substring(0, 250)}`}
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