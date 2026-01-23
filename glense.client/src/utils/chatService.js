const RAW_BASE = import.meta.env.VITE_CHAT_SERVICE_URL || 'http://localhost:5004';

function normalizeBase(raw) {
  if (!raw) return 'http://localhost:5004';
  const s = raw.trim();
  if (/^:\d+$/.test(s)) return `http://localhost${s}`;
  if (/^\d+$/.test(s)) return `http://localhost:${s}`;
  if (!/^https?:\/\//i.test(s)) return `http://${s}`;
  return s.replace(/\/$/, '');
}

const BASE = normalizeBase(RAW_BASE);
try { console.info('ChatService BASE =', BASE); } catch {}

async function request(path, opts = {}) {
  try {
    const res = await fetch(`${BASE}${path}`, {
      headers: { 'Content-Type': 'application/json', ...(opts.headers || {}) },
      ...opts,
    });
    if (!res.ok) {
      const text = await res.text().catch(() => '');
      throw new Error(`${res.status} ${res.statusText} ${text}`);
    }
    if (res.status === 204) return null;
    return res.json().catch(() => null);
  } catch (err) {
    try { console.error('chatService request failed:', err, { path, opts, BASE }); } catch {}
    throw err;
  }
}

export const getChats = (cursor, pageSize = 50) => {
  const params = new URLSearchParams();
  if (cursor) params.set('cursor', cursor);
  params.set('pageSize', pageSize);
  return request(`/api/chats?${params.toString()}`);
};

export const createChat = (body) =>
  request(`/api/chats`, { method: 'POST', body: JSON.stringify(body) });

export const getChat = (chatId) => request(`/api/chats/${chatId}`);

export const deleteChat = (chatId) =>
  request(`/api/chats/${chatId}`, { method: 'DELETE' });

export const getMessages = (chatId, cursor, pageSize = 50) => {
  const params = new URLSearchParams();
  if (cursor) params.set('cursor', cursor);
  params.set('pageSize', pageSize);
  return request(`/api/chats/${chatId}/messages?${params.toString()}`);
};

export const postMessage = (chatId, body) =>
  request(`/api/chats/${chatId}/messages`, {
    method: 'POST',
    body: JSON.stringify(body),
  });

export const getMessage = (messageId) => request(`/api/messages/${messageId}`);

export const deleteMessage = (messageId) =>
  request(`/api/messages/${messageId}`, { method: 'DELETE' });

export default {
  getChats,
  createChat,
  getChat,
  deleteChat,
  getMessages,
  postMessage,
  getMessage,
  deleteMessage,
};
