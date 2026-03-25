import { useState, useEffect } from "react";
import { useParams } from "react-router-dom";
import { Link } from "react-router-dom";
import ReactPlayer from "react-player";
import { Typography, Box, Stack, Button, FormControl, Select, MenuItem, Avatar } from "@mui/material";
import PlaylistAddIcon from '@mui/icons-material/PlaylistAdd';
import {
  CheckCircle,
  ThumbDownOutlined,
  ThumbUpOutlined,
} from "@mui/icons-material";

import { Videos, VideoComments } from ".";
const demoVideoInfo = {
  title: 'Loading...', channelTitle: '', viewCount: 0,
  likeCount: 0, dislikeCount: 0, publishedAt: '', tags: [], description: ''
};
import { getVideo, getVideos, getPlaylists, addVideoToPlaylist } from "../utils/videoApi";
import { profileService } from "../services/profileService";
import { useAuth } from "../context/AuthContext";

import "../css/VideoStream.css";

function VideoStream() {
  const [showMoreTags, setShowMoreTags] = useState(false);
  const [showMoreDesc, setShowMoreDesc] = useState(false);
  const { id } = useParams();
  const [video, setVideo] = useState(null);
  const [uploader, setUploader] = useState(null);
  const [related, setRelated] = useState([]);
  const [playlists, setPlaylists] = useState([]);
  const [addingTo, setAddingTo] = useState("");
  const [adding, setAdding] = useState(false);
  const { user } = useAuth();
  useEffect(() => {
    let mounted = true;
    if (!id) return;
    getVideo(id).then(d => {
      if (!mounted) return;
      setVideo(d);
      if (d?.uploaderId && d.uploaderId !== '00000000-0000-0000-0000-000000000000') {
        profileService.getUserById(d.uploaderId)
          .then(p => { if (mounted) setUploader(p); })
          .catch(() => {});
      }
    }).catch(() => {});
    getVideos().then(list => { if (mounted && Array.isArray(list)) setRelated(list.filter(v => String(v.id) !== String(id)).slice(0, 12)); }).catch(() => {});
    return () => { mounted = false; };
  }, [id]);

  useEffect(() => {
    let mounted = true;
    const uid = user?.id || 0;
    getPlaylists(uid).then(list => { if (mounted && Array.isArray(list)) setPlaylists(list); }).catch(() => {});
    return () => { mounted = false; };
  }, [user]);

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
              <Link to={`/channel/${video?.uploaderId}`} style={{ display: 'flex', alignItems: 'center', gap: 10, textDecoration: 'none' }}>
                <Avatar sx={{ bgcolor: '#c62828', width: 36, height: 36, fontSize: 16 }}>
                  {(uploader?.username || video?.title || '?').charAt(0).toUpperCase()}
                </Avatar>
                <Typography className="channel-title">
                  {uploader?.username || 'Glense'}
                  <CheckCircle className="check-circle-icon" />
                </Typography>
              </Link>

              <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                <Typography className="like-dislike">
                  <ThumbUpOutlined className="thumb-icon" />
                  {Number(video?.likeCount ?? demoVideoInfo.likeCount).toLocaleString()} {" | "}
                  <ThumbDownOutlined className="thumb-icon" />
                  {Number(video?.dislikeCount ?? demoVideoInfo.dislikeCount).toLocaleString()}
                </Typography>

                <FormControl size="small" sx={{ minWidth: 160 }}>
                  <Select
                    displayEmpty
                    value={addingTo}
                    onChange={(e) => setAddingTo(e.target.value)}
                    renderValue={(selected) => {
                      if (!selected) return 'Add to playlist';
                      const p = playlists.find(x => String(x.id) === String(selected));
                      return p ? p.name : 'Add to playlist';
                    }}
                  >
                    <MenuItem value="">Add to playlist</MenuItem>
                    {playlists.map(p => (
                      <MenuItem key={p.id} value={p.id}>{p.name}</MenuItem>
                    ))}
                  </Select>
                </FormControl>

                <Button
                  variant="outlined"
                  size="small"
                  startIcon={<PlaylistAddIcon />}
                  disabled={adding}
                  onClick={async () => {
                    if (!addingTo || !video?.id) { alert('Select a playlist'); return; }
                    setAdding(true);
                    try {
                      await addVideoToPlaylist(addingTo, video.id);
                      alert('Added to playlist');
                    } catch (e) {
                      alert('Failed to add to playlist');
                    }
                    setAdding(false);
                  }}
                >
                  Add
                </Button>
              </Box>
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