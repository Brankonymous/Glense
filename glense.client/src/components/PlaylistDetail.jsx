import { useEffect, useState } from "react";
import { useParams, Link } from "react-router-dom";
import { Box, Typography, Grid, Button, Card, CardMedia, CardContent } from "@mui/material";
import { getPlaylistVideos, removeVideoFromPlaylist } from "../utils/videoApi";

function PlaylistDetail() {
  const { id } = useParams();
  const [videos, setVideos] = useState([]);

  useEffect(() => {
    let mounted = true;
    if (!id) return;
    getPlaylistVideos(id).then(list => { if (mounted) setVideos(Array.isArray(list) ? list : []); }).catch(() => {});
    return () => { mounted = false; };
  }, [id]);

  const handleRemove = async (videoId) => {
    try {
      await removeVideoFromPlaylist(id, videoId);
      setVideos(prev => prev.filter(v => String(v.id) !== String(videoId)));
    } catch (e) { alert('Failed to remove'); }
  };

  return (
    <Box sx={{ p:3 }}>
      <Typography variant="h5">Playlist</Typography>
      <Grid container spacing={2} sx={{ mt:2 }}>
        {videos.map(v => (
          <Grid item xs={12} sm={6} md={4} key={v.id}>
            <Card>
              <Link to={`/video/${v.id}`}>
                <CardMedia image={v.thumbnailUrl || '/'} sx={{ height:140 }} />
              </Link>
              <CardContent>
                <Typography variant="subtitle1">{v.title}</Typography>
                <Button size="small" onClick={() => handleRemove(v.id)}>Remove</Button>
              </CardContent>
            </Card>
          </Grid>
        ))}
      </Grid>
    </Box>
  );
}

export default PlaylistDetail;
