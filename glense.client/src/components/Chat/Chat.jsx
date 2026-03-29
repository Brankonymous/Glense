import { useState, useEffect } from "react";
import ChatWindow from "./ChatWindow";
import ChatSidebar from "./ChatSidebar";
import chatService from "../../utils/chatService";
import { useAuth } from "../../context/AuthContext";
import "../../css/Chat/Chat.css";

function getOtherName(topic, myName) {
    if (!topic) return 'Chat';
    const parts = topic.split(':');
    if (parts.length === 2) {
        return parts[0] === myName ? parts[1] : parts[0];
    }
    return topic;
}

function Chat() {
    const [chats, setChats] = useState([]);
    const [selectedChat, setSelectedChat] = useState(null);
    const { user } = useAuth();
    const displayName = user?.username || 'Anonymous';
    const localSender = 'user';

    const normalizeItems = (res) => {
        if (!res) return [];
        if (Array.isArray(res)) return res;
        return res.items || res.Items || [];
    };

    const enrichChat = (chat) => {
        const topic = chat.topic || chat.Topic || '';
        return { ...chat, displayName: getOtherName(topic, displayName) };
    };

    const loadChats = async () => {
        try {
            const res = await chatService.getChats();
            const items = normalizeItems(res)
                .filter(c => {
                    const topic = c.topic || c.Topic || '';
                    const parts = topic.split(':');
                    return parts.includes(displayName);
                })
                .map(enrichChat);
            setChats(items);
            if (!selectedChat && items.length) {
                const first = items[0];
                setSelectedChat(first);
                await loadMessages(first);
            }
        } catch (err) {
            console.error("getChats", err);
        }
    };

    const loadMessages = async (chat) => {
        if (!chat) return;
        try {
            const res = await chatService.getMessages(chat.id || chat.Id || chat.chatId);
            const msgs = normalizeItems(res).map(m => {
                const raw = m.content || m.Content || '';
                const match = /^([^:]+):\s*(.*)$/.exec(raw);
                const senderName = match ? match[1] : (m.sender || m.Sender || '');
                const messageText = match ? match[2] : raw;
                return {
                    id: m.id || m.Id,
                    sender: senderName,
                    message: messageText,
                    time: new Date(m.createdAtUtc || m.CreatedAtUtc).toLocaleTimeString([], {hour:'2-digit', minute:'2-digit'}),
                    isMe: senderName === displayName
                };
            });
            setSelectedChat(prev => ({ ...enrichChat(chat), messages: msgs }));
        } catch (err) {
            console.error("getMessages", err);
        }
    };

    useEffect(() => { loadChats(); }, []);

    const handleSelect = async (chat) => {
        setSelectedChat(enrichChat(chat));
        await loadMessages(chat);
    };

    const handleCreate = async (otherUsername) => {
        try {
            const topic = `${displayName}:${otherUsername}`;
            const created = await chatService.createChat({ topic });
            await loadChats();
            const id = created?.id || created?.Id || created?.chatId;
            if (id) {
                const c = await chatService.getChat(id);
                handleSelect(c || { id, topic });
            }
        } catch (err) {
            console.error('createChat', err);
            throw err;
        }
    };

    const handleSend = async (text) => {
        if (!selectedChat) return;
        try {
            const payload = { sender: localSender, content: `${displayName}: ${text}` };
            const created = await chatService.postMessage(selectedChat.id || selectedChat.Id || selectedChat.chatId, payload);
            const raw = created?.content || created?.Content || '';
            const match = /^([^:]+):\s*(.*)$/.exec(raw);
            const senderName = match ? match[1] : (created?.sender || created?.Sender || '');
            const messageText = match ? match[2] : raw;
            const m = {
                id: created?.id || created?.Id,
                sender: senderName,
                message: messageText,
                time: new Date(created?.createdAtUtc || created?.CreatedAtUtc).toLocaleTimeString([], {hour:'2-digit', minute:'2-digit'}),
                isMe: senderName === displayName
            };
            setSelectedChat(prev => ({ ...prev, messages: [...(prev?.messages || []), m] }));
        } catch (err) {
            console.error("postMessage", err);
        }
    };

    return (
        <div className="chat-container">
            <ChatSidebar chats={chats} onSelectChat={handleSelect} onCreate={handleCreate} />
            <ChatWindow chat={selectedChat || { profileImage: '', name: '', messages: [] }} onSend={handleSend} />
        </div>
    );
}

export default Chat;
