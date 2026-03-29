import { useState, useEffect } from "react";
import { useParams, Link } from "react-router-dom";
import ReactPlayer from "react-player";
import { Typography, Box, Stack, Button, FormControl, Select, MenuItem, Avatar, Snackbar } from "@mui/material";
import PlaylistAddIcon from '@mui/icons-material/PlaylistAdd';
import EditIcon from '@mui/icons-material/Edit';
import {
  CheckCircle,
  ThumbDown,
  ThumbDownOutlined,
  ThumbUp,
  ThumbUpOutlined,
} from "@mui/icons-material";

import { Videos, VideoComments } from ".";
import { getVideo, getVideos, getPlaylists, addVideoToPlaylist, likeVideo, updateVideoCategory } from "../utils/videoApi";
import { categories } from "../utils/constants";
import { useAuth } from "../context/AuthContext";

import "../css/VideoStream.css";

const demoVideoInfo = {
  title: 'Loading...', channelTitle: '', viewCount: 0,
  likeCount: 0, dislikeCount: 0, publishedAt: '', tags: [], description: ''
};

function formatDate(dateStr) {
  if (!dateStr) return '';
  const d = new Date(dateStr);
  return d.toLocaleDateString('en-US', { year: 'numeric', month: 'short', day: 'numeric' });
}

function VideoStream() {
  const [showMoreDesc, setShowMoreDesc] = useState(false);
  const { id } = useParams();
  const [video, setVideo] = useState(null);
  const [related, setRelated] = useState([]);
  const [playlists, setPlaylists] = useState([]);
  const [addingTo, setAddingTo] = useState("");
  const [adding, setAdding] = useState(false);
  const [snackbar, setSnackbar] = useState("");
  const [userLike, setUserLike] = useState(null);
  const [editingCategory, setEditingCategory] = useState(false);
  const { user } = useAuth();

  const isOwner = user && video && String(user.id) === String(video.uploaderId);

  useEffect(() => {
    let mounted = true;
    if (!id) return;
    getVideo(id).then(d => { if (mounted) setVideo(d); }).catch(() => {});
    getVideos().then(list => { if (mounted && Array.isArray(list)) setRelated(list.filter(v => String(v.id) !== String(id)).slice(0, 12)); }).catch(() => {});
    setUserLike(null);
    setShowMoreDesc(false);
    return () => { mounted = false; };
  }, [id]);

  useEffect(() => {
    let mounted = true;
    const uid = user?.id || 0;
    getPlaylists(uid).then(list => { if (mounted && Array.isArray(list)) setPlaylists(list); }).catch(() => {});
    return () => { mounted = false; };
  }, [user]);

  const handleLike = async (isLiked) => {
    if (!user) { setSnackbar("Sign in to like videos"); return; }
    if (!video?.id) return;
    try {
      const resp = await likeVideo(video.id, isLiked);
      setVideo(prev => ({ ...prev, likeCount: resp.likeCount, dislikeCount: resp.dislikeCount }));
      setUserLike(isLiked);
    } catch {
      setSnackbar("Failed to update");
    }
  };

  const handleAddToPlaylist = async () => {
    if (!addingTo || !video?.id) { setSnackbar("Select a playlist first"); return; }
    setAdding(true);
    try {
      await addVideoToPlaylist(addingTo, video.id);
      setSnackbar("Added to playlist!");
      setAddingTo("");
    } catch (e) {
      const msg = e.message || "";
      setSnackbar(msg.includes("already") ? "Already in playlist" : "Failed to add to playlist");
    }
    setAdding(false);
  };

  const handleCategoryChange = async (newCategory) => {
    try {
      await updateVideoCategory(video.id, newCategory);
      setVideo(prev => ({ ...prev, category: newCategory || null }));
      setEditingCategory(false);
      setSnackbar("Category updated");
    } catch {
      setSnackbar("Failed to update category");
    }
  };

  const selectDarkStyles = {
    color: "var(--color-text-secondary)",
    "& .MuiOutlinedInput-notchedOutline": { borderColor: "rgba(255,255,255,0.2)" },
    "&:hover .MuiOutlinedInput-notchedOutline": { borderColor: "rgba(255,255,255,0.4)" },
    "& .MuiSvgIcon-root": { color: "var(--color-text-secondary)" },
  };

  const darkMenuProps = { PaperProps: { sx: { bgcolor: "var(--color-bg-secondary)", color: "var(--color-text-white)" } } };

  const descText = video?.description || demoVideoInfo.description;

  return (
    <Box className="video-stream-container">
      <Stack direction="row" className="video-stream-stack">
      <Box className="video-player-container">
        <Box className="video-player-box">
            <ReactPlayer
              url={video?.videoUrl?.startsWith('http') ? video.videoUrl : `${import.meta.env.VITE_API_URL || 'http://localhost:5050'}/api/videos/${id}/stream`}
              controls
              width="100%"
              height="100%"
            />
            <Typography className="video-title">{video?.title || demoVideoInfo.title}</Typography>

            <Stack className="video-details">
              <Link to={`/channel/${video?.uploaderUsername || video?.uploaderId}`} style={{ display: 'flex', alignItems: 'center', gap: 10, textDecoration: 'none' }}>
                <Avatar sx={{ bgcolor: '#c62828', width: 36, height: 36, fontSize: 16 }}>
                  {(video?.uploaderUsername || video?.title || '?').charAt(0).toUpperCase()}
                </Avatar>
                <Typography className="channel-title">
                  {video?.uploaderUsername || 'Glense'}
                  <CheckCircle className="check-circle-icon" />
                </Typography>
              </Link>

              <Box className="video-actions">
                <Box className="like-dislike">
                  <button className="like-btn" onClick={() => handleLike(true)}>
                    {userLike === true ? <ThumbUp className="thumb-icon active" /> : <ThumbUpOutlined className="thumb-icon" />}
                  </button>
                  {Number(video?.likeCount ?? demoVideoInfo.likeCount).toLocaleString()}
                  <span className="like-separator">|</span>
                  <button className="like-btn" onClick={() => handleLike(false)}>
                    {userLike === false ? <ThumbDown className="thumb-icon active" /> : <ThumbDownOutlined className="thumb-icon" />}
                  </button>
                  {Number(video?.dislikeCount ?? demoVideoInfo.dislikeCount).toLocaleString()}
                </Box>

                {user && playlists.length > 0 && (
                  <Box className="playlist-add-row">
                    <FormControl size="small" sx={{ minWidth: 150 }}>
                      <Select
                        displayEmpty
                        value={addingTo}
                        onChange={(e) => setAddingTo(e.target.value)}
                        renderValue={(selected) => {
                          if (!selected) return 'Add to playlist';
                          const p = playlists.find(x => String(x.id) === String(selected));
                          return p ? p.name : 'Add to playlist';
                        }}
                        sx={selectDarkStyles}
                        MenuProps={darkMenuProps}
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
                      disabled={adding || !addingTo}
                      onClick={handleAddToPlaylist}
                      className="playlist-add-btn"
                    >
                      {adding ? "Adding..." : "Add"}
                    </Button>
                  </Box>
                )}
              </Box>
            </Stack>

            {/* Description */}
            <Box className={`description-container ${showMoreDesc ? 'expanded' : ''}`} onClick={() => { if (!showMoreDesc) setShowMoreDesc(true); }}>
              <Box className="description-details">
                <Typography>{Number(video?.viewCount ?? demoVideoInfo.viewCount).toLocaleString()} views</Typography>
                <Typography className="publish-date">{formatDate(video?.uploadDate)}</Typography>

                {video?.category && (
                  <Typography className="video-category-badge">{video.category}</Typography>
                )}

                {showMoreDesc ? (
                  <>
                    <Typography className="description-text">{descText}</Typography>
                    <button className="toggle-description-button" onClick={(e) => { e.stopPropagation(); setShowMoreDesc(false); }}>
                      Show less
                    </button>
                  </>
                ) : (
                  <Typography className="description-text">
                    {descText.length > 250 ? `${descText.substring(0, 250)}...` : descText}
                  </Typography>
                )}
              </Box>
            </Box>

            {/* Category edit (owner only) */}
            {isOwner && (
              <Box className="category-edit-row">
                {editingCategory ? (
                  <FormControl size="small" sx={{ minWidth: 150 }}>
                    <Select
                      value={video?.category || ""}
                      onChange={(e) => handleCategoryChange(e.target.value)}
                      sx={selectDarkStyles}
                      MenuProps={darkMenuProps}
                      displayEmpty
                    >
                      <MenuItem value="">None</MenuItem>
                      {categories.filter(c => c.name !== "New Videos").map(c => (
                        <MenuItem key={c.name} value={c.name}>{c.name}</MenuItem>
                      ))}
                    </Select>
                  </FormControl>
                ) : (
                  <Button
                    size="small"
                    startIcon={<EditIcon />}
                    onClick={() => setEditingCategory(true)}
                    className="category-edit-btn"
                  >
                    {video?.category ? `Category: ${video.category}` : "Set category"}
                  </Button>
                )}
              </Box>
            )}

            {/* Comments section */}
            <Typography className="comments-section-title">Comments</Typography>
            <VideoComments id={id} />

          </Box>
        </Box>

        <Box className="related-videos-container">
          <Videos videos={related} direction={'column'} />
        </Box>
      </Stack>

      <Snackbar
        open={!!snackbar}
        autoHideDuration={3000}
        onClose={() => setSnackbar("")}
        message={snackbar}
      />
    </Box>
  );
}

export default VideoStream;
