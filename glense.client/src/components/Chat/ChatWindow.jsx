import React from "react";
import MessageBubble from "./MessageBubble";
import { Box, TextField, IconButton, Avatar } from "@mui/material";
import SendIcon from "@mui/icons-material/Send";
import "../../css/Chat/ChatWindow.css";
import { stringToColor } from "../../utils/constants";
import { useState } from "react";

const ChatWindow = ({ chat, onSend }) => {
  const [text, setText] = useState("");

  const submit = () => {
    const trimmed = (text || "").trim();
    if (!trimmed) return;
    if (onSend) onSend(trimmed);
    setText("");
  };

  const onKeyDown = (e) => {
    if (e.key === "Enter" && !e.shiftKey) {
      e.preventDefault();
      submit();
    }
  };

  return (
    <div className="chat-window">
      {/* Chat Header */}
      <div className="chat-window-header">
        <Avatar sx={{ bgcolor: stringToColor(chat.topic || chat.Topic || chat.name || ''), width: 36, height: 36, fontSize: 16 }}>
          {(chat.topic || chat.Topic || chat.name || '?').charAt(0).toUpperCase()}
        </Avatar>
        <span className="chat-window-header-name">{chat.name || chat.Topic || chat.topic || chat.title || 'Chat'}</span>
      </div>

      {/* Chat Messages */}
      <Box className="chat-window-messages">
        {(chat?.messages || []).map((message, index) => (
          <MessageBubble key={index} message={message} />
        ))}
      </Box>

      {/* Input Field */}
      <div className="chat-window-input">
        <TextField
          fullWidth
          placeholder="Type a message..."
          variant="outlined"
          size="small"
          className="no-focus-outline"
          value={text}
          onChange={(e) => setText(e.target.value)}
          onKeyDown={onKeyDown}
        />
        <IconButton color="primary" onClick={submit}>
          <SendIcon />
        </IconButton>
      </div>
    </div>
  );
};

export default ChatWindow;
