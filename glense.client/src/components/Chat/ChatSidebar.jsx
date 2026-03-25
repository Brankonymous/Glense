import React, { useState } from "react";
import { Box, List, ListItem, ListItemAvatar, Avatar, ListItemText, TextField, Button, ListItemButton } from "@mui/material";
import "../../css/Chat/ChatSideBar.css";

const colors = ['#e91e63', '#9c27b0', '#673ab7', '#3f51b5', '#2196f3', '#00bcd4', '#009688', '#4caf50', '#ff9800', '#ff5722'];
function stringToColor(str) {
  let h = 0;
  for (let i = 0; i < (str||'').length; i++) h = str.charCodeAt(i) + ((h << 5) - h);
  return colors[Math.abs(h) % colors.length];
}

const ChatSidebar = ({ chats, onSelectChat, onCreate }) => {
  const [topic, setTopic] = useState("");

  const handleCreate = async () => {
    if (!topic) return;
    try {
      await onCreate?.(topic);
      setTopic("");
    } catch (err) {
      console.error("create chat", err);
    }
  };

  return (
    <div className="chat-sidebar">
      <Box sx={{ display: 'flex', gap: 1, p: 1 }}>
        <TextField size="small" placeholder="New chat" value={topic} onChange={e => setTopic(e.target.value)} fullWidth />
        <Button variant="contained" size="small" onClick={handleCreate}>Create</Button>
      </Box>
      <List>
        {chats.map((chat, index) => (
          <ListItem key={index} className="chat-sidebar-item" disablePadding>
            <ListItemButton onClick={() => onSelectChat(chat)}>
              <ListItemAvatar>
                <Avatar sx={{ bgcolor: stringToColor(chat.topic || chat.Topic || chat.name || ''), fontSize: 16 }}>
                  {(chat.topic || chat.Topic || chat.name || '?').charAt(0).toUpperCase()}
                </Avatar>
              </ListItemAvatar>
              <ListItemText primary={chat.name || chat.Topic || chat.topic || chat.title || 'Untitled'} />
            </ListItemButton>
          </ListItem>
        ))}
      </List>
    </div>
  );
};

export default ChatSidebar;
