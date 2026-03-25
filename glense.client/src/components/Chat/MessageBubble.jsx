import React from "react";
import { Box, Avatar } from "@mui/material";
import "../../css/Chat/MessageBubble.css";

const colors = ['#e91e63', '#9c27b0', '#673ab7', '#3f51b5', '#2196f3', '#00bcd4', '#009688', '#4caf50', '#ff9800', '#ff5722'];
function stringToColor(str) {
  let h = 0;
  for (let i = 0; i < (str||'').length; i++) h = str.charCodeAt(i) + ((h << 5) - h);
  return colors[Math.abs(h) % colors.length];
}

const MessageBubble = ({ message }) => {
  const isMe = message.isMe;
  const senderName = message.sender || '';

  return (
    <Box className={`message-bubble ${isMe ? "me" : "other"}`}>
      {!isMe && (
        <Avatar sx={{ bgcolor: stringToColor(senderName), width: 28, height: 28, fontSize: 13, flexShrink: 0, mr: 1 }}>
          {senderName.charAt(0).toUpperCase() || '?'}
        </Avatar>
      )}
      <Box className="message-bubble-content">
        {!isMe && senderName && (
          <div className="message-bubble-sender">{senderName}</div>
        )}
        <div className="message-bubble-text">{message.message}</div>
        <div className="message-bubble-time">{message.time}</div>
      </Box>
    </Box>
  );
};

export default MessageBubble;
