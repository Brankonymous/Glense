import React, { useEffect, useState } from "react";
import { Box, Button, List, ListItem, TextField, Typography } from "@mui/material";
import chatService from "../utils/chatService";

export default function ChatDetail({ chatId }) {
  const [messages, setMessages] = useState([]);
  const [text, setText] = useState("");
  const [displayName, setDisplayName] = useState(() => {
    try { return localStorage.getItem('chat.displayName') || 'Alice'; } catch { return 'Alice'; }
  });

  const load = async () => {
    try {
      const data = await chatService.getMessages(chatId);
      // API returns PagedResponse<T> — normalize to array
      let items = [];
      if (!data) items = [];
      else if (Array.isArray(data)) items = data;
      else items = data.Items || data.items || [];
      setMessages(items);
    } catch (err) {
      console.error("getMessages", err);
    }
  };

  useEffect(() => {
    if (!chatId) return;
    load();
    // optional: poll every 5s
    const t = setInterval(load, 5000);
    return () => clearInterval(t);
  }, [chatId]);

  const handleSend = async () => {
    if (!text) return;
    try {
      // backend expects { sender: 'user'|'system', content }
      // prefix content with local display name so messages show as from Alice/Bob
      const payload = { sender: "user", content: `${displayName}: ${text}` };
      await chatService.postMessage(chatId, payload);
      setText("");
      await load();
    } catch (err) {
      console.error("postMessage", err);
    }
  };

  return (
    <Box>
      <Typography variant="h6">Chat</Typography>
      <List>
        {messages.map((m) => (
          <ListItem key={m.id || m.messageId}>
            <div style={{ width: "100%" }}>
              <div style={{ fontSize: 12, color: "#666" }}>{m.sender || m.Sender || m.senderId}</div>
              <div>{m.content || m.Content}</div>
            </div>
          </ListItem>
        ))}
      </List>

      <Box style={{ display: "flex", gap: 8, alignItems: 'center' }}>
        <TextField
          size="small"
          placeholder="You (Alice/Bob)"
          value={displayName}
          onChange={(e) => { setDisplayName(e.target.value); try { localStorage.setItem('chat.displayName', e.target.value); } catch {} }}
          style={{ width: 120 }}
        />
        <TextField
          fullWidth
          size="small"
          placeholder="Type a message"
          value={text}
          onChange={(e) => setText(e.target.value)}
        />
        <Button variant="contained" onClick={handleSend}>
          Send
        </Button>
      </Box>
    </Box>
  );
}
