import { useEffect, useState } from "react";
import { Box, Typography, TextField, Button, List, ListItem, ListItemText } from "@mui/material";
import { getPlaylists, createPlaylist } from "../utils/videoApi";
import { useAuth } from "../context/AuthContext";
import { Link } from "react-router-dom";

function Playlists() {
  const { user } = useAuth();
  const [playlists, setPlaylists] = useState([]);
  const [name, setName] = useState("");
  const [description, setDescription] = useState("");
  const [message, setMessage] = useState("");

  useEffect(() => {
    let mounted = true;
    getPlaylists(user?.id || 0).then(list => { if (mounted) setPlaylists(Array.isArray(list) ? list : []); }).catch(() => {});
    return () => { mounted = false; };
  }, [user]);

  const handleCreate = async (e) => {
    e.preventDefault();
    try {
      const resp = await createPlaylist(name, description, user?.id || 0);
      setPlaylists(prev => [resp, ...prev]);
      setName(""); setDescription("");
      setMessage("Playlist created");
    } catch (err) {
      setMessage(err.message || String(err));
    }
  };

  return (
    <Box sx={{ p: 3 }}>
      <Typography variant="h5">Your Playlists</Typography>

      <Box component="form" onSubmit={handleCreate} sx={{ my:2, display:'flex', gap:2, alignItems:'center' }}>
        <TextField label="Name" value={name} onChange={(e) => setName(e.target.value)} />
        <TextField label="Description" value={description} onChange={(e) => setDescription(e.target.value)} />
        <Button type="submit" variant="contained">Create</Button>
      </Box>

      {message && <Typography sx={{ mb:2 }}>{message}</Typography>}

      <List>
        {playlists.map(p => (
          <ListItem key={p.id}>
            <ListItemText primary={
              <Link to={`/playlists/${p.id}`} style={{ textDecoration: 'none' }}>{p.name}</Link>
            } secondary={p.description} />
          </ListItem>
        ))}
      </List>
    </Box>
  );
}

export default Playlists;
