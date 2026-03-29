import { useEffect, useState } from "react";
import { useParams, Link } from "react-router-dom";
import { Box, Typography, Grid, Button, Stack, Snackbar } from "@mui/material";
import DeleteOutlineIcon from "@mui/icons-material/DeleteOutline";
import { getPlaylistVideos, removeVideoFromPlaylist } from "../utils/videoApi";
import "../css/PlaylistDetail.css";

function PlaylistDetail() {
  const { id } = useParams();
  const [videos, setVideos] = useState([]);

  useEffect(() => {
    let mounted = true;
    if (!id) return;
    getPlaylistVideos(id).then(list => { if (mounted) setVideos(Array.isArray(list) ? list : []); }).catch(() => {});
    return () => { mounted = false; };
  }, [id]);

  const [snackbar, setSnackbar] = useState("");

  const handleRemove = async (videoId) => {
    try {
      await removeVideoFromPlaylist(id, videoId);
      setVideos(prev => prev.filter(v => String(v.id) !== String(videoId)));
      setSnackbar("Video removed");
    } catch { setSnackbar("Failed to remove video"); }
  };

  return (
    <Box className="playlist-detail-page">
      <Stack direction="row" spacing={2} sx={{ alignItems: "center", mb: 1 }}>
        <Link to="/playlists" className="playlist-back-link">&larr; Back to playlists</Link>
        <Typography variant="h5" className="playlist-detail-title">Playlist</Typography>
      </Stack>

      {videos.length === 0 && (
        <Typography className="playlist-detail-empty">No videos in this playlist yet.</Typography>
      )}

      <Grid container spacing={2} sx={{ mt: 1 }}>
        {videos.map(v => (
          <Grid item xs={12} sm={6} md={4} key={v.id}>
            <Box className="playlist-video-card">
              <Link to={`/video/${v.id}`}>
                <Box
                  className="playlist-video-thumb"
                  sx={{ backgroundImage: `url(${v.thumbnailUrl || ""})` }}
                />
              </Link>
              <Stack className="playlist-video-info">
                <Link to={`/video/${v.id}`} className="playlist-video-link">
                  <Typography className="playlist-video-title">{v.title}</Typography>
                </Link>
                <Button
                  size="small"
                  startIcon={<DeleteOutlineIcon />}
                  onClick={() => handleRemove(v.id)}
                  className="playlist-video-remove"
                >
                  Remove
                </Button>
              </Stack>
            </Box>
          </Grid>
        ))}
      </Grid>

      <Snackbar open={!!snackbar} autoHideDuration={3000} onClose={() => setSnackbar("")} message={snackbar} />
    </Box>
  );
}

export default PlaylistDetail;
