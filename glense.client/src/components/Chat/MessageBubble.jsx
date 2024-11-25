import React from "react";
import { Box } from "@mui/material";
import "../../css/Chat/MessageBubble.css";

const MessageBubble = ({ message }) => {
  const isMe = message.isMe;
  return (
    <Box className={`message-bubble ${isMe ? "me" : "other"}`}>
      <Box className="message-bubble-content">
        <div className="message-bubble-text">{message.message}</div>
        <div className="message-bubble-time">{message.time}</div>
      </Box>
    </Box>
  );
};

export default MessageBubble;
