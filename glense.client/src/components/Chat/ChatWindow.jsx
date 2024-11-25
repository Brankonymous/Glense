import React from "react";
import MessageBubble from "./MessageBubble";
import { Box, TextField, IconButton } from "@mui/material";
import SendIcon from "@mui/icons-material/Send";
import "../../css/Chat/ChatWindow.css";

const ChatWindow = ({ chat }) => {
  return (
    <div className="chat-window">
      {/* Chat Header */}
      <div className="chat-window-header">
        <img
          src={chat.profileImage}
          alt={chat.name}
          className="chat-window-header-image"
        />
        <span className="chat-window-header-name">{chat.name}</span>
      </div>

      {/* Chat Messages */}
      <Box className="chat-window-messages">
        {chat.messages.map((message, index) => (
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
        />
        <IconButton color="primary">
          <SendIcon />
        </IconButton>
      </div>
    </div>
  );
};

export default ChatWindow;
