import React, { useEffect, useState } from "react";
import { Box, Button, List, ListItem, ListItemButton, TextField, Typography } from "@mui/material";
import ChatDetail from "./ChatDetail";
import chatService from "../utils/chatService";

import "../css/Chat.css";

export default function ChatList() {
  const [chats, setChats] = useState([]);
  const [selected, setSelected] = useState(null);
  const [newTitle, setNewTitle] = useState("");

  const load = async () => {
    try {
      const data = await chatService.getChats();
      // API returns a PagedResponse<T> with Items or items; normalize to an array
      let items = [];
      if (!data) items = [];
      else if (Array.isArray(data)) items = data;
      else items = data.Items || data.items || [];
      setChats(items);
    } catch (err) {
      console.error("getChats", err);
    }
  };

  useEffect(() => {
    load();
  }, []);

  const handleCreate = async () => {
    if (!newTitle) return;
    try {
      // backend expects `Topic` in CreateChatRequest; use camel-case `topic`
      const created = await chatService.createChat({ topic: newTitle });
      setNewTitle("");
      await load();
      setSelected(created?.id || created?.chatId || null);
    } catch (err) {
      console.error("createChat", err);
    }
  };

  return (
    <Box className="chat-list-root">
      <Box className="chat-list-panel">
        <Typography variant="h6">Chats</Typography>
        <List className="chat-list">
          {chats.map((c) => (
            <ListItem key={c.id || c.chatId} disablePadding>
              <ListItemButton
                className={selected === (c.id || c.chatId) ? "selected" : ""}
                onClick={() => setSelected(c.id || c.chatId)}
              >
                {c.topic || c.Topic || c.title || c.name || "Untitled"}
              </ListItemButton>
            </ListItem>
          ))}
        </List>

        <Box className="chat-create">
          <TextField
            size="small"
            placeholder="New chat title"
            value={newTitle}
            onChange={(e) => setNewTitle(e.target.value)}
          />
          <Button onClick={handleCreate} variant="contained" size="small">
            Create
          </Button>
        </Box>
      </Box>

      <Box className="chat-detail-panel">
        {selected ? (
          <ChatDetail chatId={selected} />
        ) : (
          <Typography>Select a chat to open</Typography>
        )}
      </Box>
    </Box>
  );
}
