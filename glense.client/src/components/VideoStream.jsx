import { useState, useEffect } from "react";
import { useParams } from "react-router-dom";
import { Link } from "react-router-dom";
import ReactPlayer from "react-player";
import { Typography, Box, Stack } from "@mui/material";
import {
  CheckCircle,
  ThumbDownOutlined,
  ThumbUpOutlined,
} from "@mui/icons-material";

import { Videos, VideoComments } from ".";
import { videoInfo as demoVideoInfo } from "../utils/constants";
import { getVideo, getVideos } from "../utils/videoApi";

import "../css/VideoStream.css";

function VideoStream() {
  const [showMoreTags, setShowMoreTags] = useState(false);
  const [showMoreDesc, setShowMoreDesc] = useState(false);
  const { id } = useParams();
  const [video, setVideo] = useState(null);
  const [related, setRelated] = useState([]);
  useEffect(() => {
    let mounted = true;
    if (!id) return;
    getVideo(id).then(d => { if (mounted) setVideo(d); }).catch(() => {});
    getVideos().then(list => { if (mounted && Array.isArray(list)) setRelated(list.filter(v => String(v.id) !== String(id)).slice(0, 12)); }).catch(() => {});
    return () => { mounted = false; };
  }, [id]);

  return (
    <Box className="video-stream-container">
      <Stack direction="row" className="video-stream-stack">
      <Box className="video-player-container">
        <Box className="video-player-box">
            <ReactPlayer 
              url={video?.videoUrl || (id && id.includes('-') ? undefined : `https://www.youtube.com/watch?v=${id}`)}
              controls
              width="100%"
              height="100%"
            />
            <Typography className="video-title">{video?.title || demoVideoInfo.title}</Typography>

            <Stack className="video-details">
              <Link to={`/channel/${video?.uploaderId || demoVideoInfo.channelId}`}>
              <Typography className="channel-title">
                  {video?.channelTitle || demoVideoInfo.channelTitle}
                  <CheckCircle className="check-circle-icon" />
                </Typography>
              </Link>

              <Typography className="like-dislike">
                <ThumbUpOutlined className="thumb-icon" />
                {Number(video?.likeCount ?? demoVideoInfo.likeCount).toLocaleString()} {" | "}
                <ThumbDownOutlined className="thumb-icon" />
                {Number(video?.dislikeCount ?? demoVideoInfo.dislikeCount).toLocaleString()}
              </Typography>
            </Stack>

            {/* Description */}
             <Box className="description-container">
              <Box className="description-details">
                <Typography>{Number(video?.viewCount ?? demoVideoInfo.viewCount).toLocaleString()} views</Typography>
                <Typography className="publish-date">Published at {video?.uploadDate ?? demoVideoInfo.publishedAt}</Typography>


                {(video?.tags || demoVideoInfo.tags || []).map((tag, index) =>
                  (video?.tags || demoVideoInfo.tags).length > 10 ? (
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
                      #{tag}
                    </Typography>
                  )
                )}
                {(video?.tags || demoVideoInfo.tags).length > 10 && (
                  <button
                    className="toggle-tags-button"
                    onClick={() => setShowMoreTags(!showMoreTags)}
                  >
                    {showMoreTags ? "Show less" : "..."}
                  </button>
                )}

                <Typography className="description-text">
                  {showMoreDesc
                    ? (video?.description || demoVideoInfo.description)
                    : `${(video?.description || demoVideoInfo.description).substring(0, 250)}`}
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
          <Videos videos={related} direction={'column'} />
        </Box>
      </Stack>
    </Box>
  );
}

export default VideoStream;