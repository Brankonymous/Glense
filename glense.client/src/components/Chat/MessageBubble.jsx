import React from "react";
import { Box, Avatar } from "@mui/material";
import "../../css/Chat/MessageBubble.css";
import { stringToColor } from "../../utils/constants";

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
