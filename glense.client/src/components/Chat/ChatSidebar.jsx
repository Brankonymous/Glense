import { useState, useEffect, useRef } from "react";
import { Box, List, ListItem, ListItemAvatar, Avatar, ListItemText, TextField, ListItemButton, Typography, Stack } from "@mui/material";
import { profileService } from "../../services/profileService";
import { useAuth } from "../../context/AuthContext";
import "../../css/Chat/ChatSidebar.css";

const colors = ['#e91e63', '#9c27b0', '#673ab7', '#3f51b5', '#2196f3', '#00bcd4', '#009688', '#4caf50', '#ff9800', '#ff5722'];
function stringToColor(str) {
  let h = 0;
  for (let i = 0; i < (str||'').length; i++) h = str.charCodeAt(i) + ((h << 5) - h);
  return colors[Math.abs(h) % colors.length];
}

const ChatSidebar = ({ chats, onSelectChat, onCreate }) => {
  const [query, setQuery] = useState("");
  const [results, setResults] = useState([]);
  const [searching, setSearching] = useState(false);
  const { user } = useAuth();
  const timerRef = useRef(null);

  useEffect(() => {
    if (!query.trim()) { setResults([]); return; }
    clearTimeout(timerRef.current);
    timerRef.current = setTimeout(async () => {
      setSearching(true);
      try {
        const users = await profileService.searchUsers(query, 10);
        setResults((users || []).filter(u => u.id !== user?.id));
      } catch { setResults([]); }
      setSearching(false);
    }, 300);
    return () => clearTimeout(timerRef.current);
  }, [query, user?.id]);

  const handleSelectUser = async (selectedUser) => {
    try {
      await onCreate?.(selectedUser.username);
      setQuery("");
      setResults([]);
    } catch (err) {
      console.error("create chat", err);
    }
  };

  return (
    <div className="chat-sidebar">
      <Box sx={{ p: 1, position: 'relative' }}>
        <TextField
          size="small"
          placeholder="Search users..."
          value={query}
          onChange={e => setQuery(e.target.value)}
          fullWidth
          className="chat-search-input"
        />
        {results.length > 0 && (
          <Box className="chat-search-results">
            {results.map(u => (
              <Stack
                key={u.id}
                direction="row"
                className="chat-search-item"
                onClick={() => handleSelectUser(u)}
              >
                <Avatar sx={{ bgcolor: stringToColor(u.username), width: 32, height: 32, fontSize: 14 }}>
                  {u.username?.charAt(0).toUpperCase()}
                </Avatar>
                <Typography className="chat-search-name">{u.username}</Typography>
              </Stack>
            ))}
          </Box>
        )}
        {searching && <Typography className="chat-search-hint">Searching...</Typography>}
        {query && !searching && results.length === 0 && (
          <Typography className="chat-search-hint">No users found</Typography>
        )}
      </Box>
      <List>
        {chats.map((chat, index) => (
          <ListItem key={index} className="chat-sidebar-item" disablePadding>
            <ListItemButton onClick={() => onSelectChat(chat)}>
              <ListItemAvatar>
                <Avatar sx={{ bgcolor: stringToColor(chat.displayName || chat.topic || ''), fontSize: 16 }}>
                  {(chat.displayName || chat.topic || '?').charAt(0).toUpperCase()}
                </Avatar>
              </ListItemAvatar>
              <ListItemText primary={chat.displayName || chat.topic || 'Untitled'} />
            </ListItemButton>
          </ListItem>
        ))}
      </List>
    </div>
  );
};

export default ChatSidebar;
