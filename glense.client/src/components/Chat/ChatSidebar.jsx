import React, { useState } from "react";
import { Box, List, ListItem, ListItemAvatar, Avatar, ListItemText, TextField, Button, ListItemButton } from "@mui/material";
import "../../css/Chat/ChatSideBar.css";

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
                <Avatar src={chat.profileImage} />
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
