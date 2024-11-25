import { useState } from "react";
import ChatWindow from "./ChatWindow";
import ChatSidebar from "./ChatSidebar";
import { chats } from "../../utils/constants";
import "../../css/Chat/Chat.css";

function Chat() {
    const [selectedChat, setSelectedChat] = useState(chats[0]);

    return (
        <div className="chat-container">  
            <ChatSidebar chats={chats} onSelectChat={setSelectedChat} />
            <ChatWindow chat={selectedChat} />
        </div>
    );
}

export default Chat;
