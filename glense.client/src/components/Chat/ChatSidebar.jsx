import React from "react";
import { List, ListItem, ListItemAvatar, Avatar, ListItemText } from "@mui/material";
import "../../css/Chat/ChatSideBar.css";

const ChatSidebar = ({ chats, onSelectChat }) => {
  return (
    <div className="chat-sidebar">
      <List>
        {chats.map((chat, index) => (
          <ListItem
            key={index}
            button
            onClick={() => onSelectChat(chat)}
            className="chat-sidebar-item"
          >
            <ListItemAvatar>
              <Avatar src={chat.profileImage} />
            </ListItemAvatar>
            <ListItemText primary={chat.name} />
          </ListItem>
        ))}
      </List>
    </div>
  );
};

export default ChatSidebar;
