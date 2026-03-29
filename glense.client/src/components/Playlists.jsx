import { useEffect, useState } from "react";
import { Box, Typography, TextField, Button, Stack } from "@mui/material";
import { getPlaylists, createPlaylist } from "../utils/videoApi";
import { useAuth } from "../context/AuthContext";
import { Link } from "react-router-dom";
import "../css/Playlists.css";

function Playlists() {
  const { user } = useAuth();
  const [playlists, setPlaylists] = useState([]);
  const [name, setName] = useState("");
  const [description, setDescription] = useState("");
  const [message, setMessage] = useState("");
  const [messageType, setMessageType] = useState("error");

  useEffect(() => {
    let mounted = true;
    getPlaylists(user?.id || 0).then(list => { if (mounted) setPlaylists(Array.isArray(list) ? list : []); }).catch(() => {});
    return () => { mounted = false; };
  }, [user]);

  const handleCreate = async (e) => {
    e.preventDefault();
    if (!name.trim()) {
      setMessage("Please enter a playlist name.");
      setMessageType("error");
      return;
    }
    try {
      const resp = await createPlaylist(name, description);
      setPlaylists(prev => [resp, ...prev]);
      setName(""); setDescription("");
      setMessage("Playlist created!");
      setMessageType("success");
    } catch (err) {
      setMessage(err.message || String(err));
      setMessageType("error");
    }
  };

  return (
    <Box className="playlists-page">
      <Box className="playlists-form" component="form" onSubmit={handleCreate}>
        <Typography variant="h5">Your Playlists</Typography>

        <Stack spacing={2}>
          <TextField label="Name" value={name} onChange={(e) => setName(e.target.value)} fullWidth />
          <TextField label="Description" value={description} onChange={(e) => setDescription(e.target.value)} fullWidth multiline rows={2} />
          <Button variant="contained" type="submit" disabled={!name.trim()}>Create</Button>
        </Stack>

        {message && (
          <Typography sx={{ mt: 1, color: messageType === "success" ? "#4caf50" : "#f44336" }}>
            {message}
          </Typography>
        )}
      </Box>

      <Stack className="playlists-list" spacing={1.5}>
        {playlists.length === 0 && (
          <Typography className="playlists-empty">No playlists yet. Create one above!</Typography>
        )}
        {playlists.map(p => (
          <Link to={`/playlists/${p.id}`} key={p.id} className="playlist-item">
            <Typography className="playlist-item-name">{p.name}</Typography>
            {p.description && (
              <Typography className="playlist-item-desc">{p.description}</Typography>
            )}
          </Link>
        ))}
      </Stack>
    </Box>
  );
}

export default Playlists;
