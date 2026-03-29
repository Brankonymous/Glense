const BASE = import.meta.env.VITE_API_URL || 'http://localhost:5050';

function authHeaders() {
  const token = localStorage.getItem('glense_auth_token');
  return token ? { 'Authorization': `Bearer ${token}` } : {};
}

async function handleRes(res) {
  if (!res.ok) {
    const txt = await res.text();
    throw new Error(txt || res.statusText);
  }
  return res.status === 204 ? null : res.json();
}

export async function getVideos() {
  const res = await fetch(`${BASE}/api/videos`);
  return handleRes(res);
}

export async function getVideo(id) {
  const res = await fetch(`${BASE}/api/videos/${id}`);
  return handleRes(res);
}

export async function updateVideoCategory(videoId, category) {
  const res = await fetch(`${BASE}/api/videos/${videoId}/category`, {
    method: 'PATCH',
    headers: { 'Content-Type': 'application/json', ...authHeaders() },
    body: JSON.stringify({ category: category || null }),
  });
  return handleRes(res);
}

export async function uploadVideo(file, title, description, thumbnail = null, category = null) {
  const fd = new FormData();
  fd.append('file', file);
  if (title) fd.append('title', title);
  if (description) fd.append('description', description);
  if (thumbnail) fd.append('thumbnail', thumbnail);
  if (category) fd.append('category', category);

  const res = await fetch(`${BASE}/api/videos/upload`, {
    method: 'POST',
    body: fd,
    headers: authHeaders(),
  });

  return handleRes(res);
}

export async function createPlaylist(name, description) {
  const res = await fetch(`${BASE}/api/playlists`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json', ...authHeaders() },
    body: JSON.stringify({ name, description }),
  });
  return handleRes(res);
}

export async function addVideoToPlaylist(playlistId, videoId) {
  const res = await fetch(`${BASE}/api/playlistvideos`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json', ...authHeaders() },
    body: JSON.stringify({ playlistId, videoId }),
  });
  return handleRes(res);
}

export async function getPlaylistVideos(playlistId) {
  const res = await fetch(`${BASE}/api/playlistvideos/${playlistId}`);
  return handleRes(res);
}

export async function removeVideoFromPlaylist(playlistId, videoId) {
  const res = await fetch(`${BASE}/api/playlistvideos`, {
    method: 'DELETE',
    headers: { 'Content-Type': 'application/json', ...authHeaders() },
    body: JSON.stringify({ playlistId, videoId }),
  });
  return handleRes(res);
}

export async function likeVideo(videoId, isLiked = true) {
  const res = await fetch(`${BASE}/api/videolikes`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json', ...authHeaders() },
    body: JSON.stringify({ videoId, isLiked }),
  });
  return handleRes(res);
}

export async function subscribeTo(subscribedToId) {
  const res = await fetch(`${BASE}/api/subscriptions`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json', ...authHeaders() },
    body: JSON.stringify({ subscribedToId }),
  });
  return handleRes(res);
}

export async function unsubscribeFrom(subscribedToId) {
  const res = await fetch(`${BASE}/api/subscriptions`, {
    method: 'DELETE',
    headers: { 'Content-Type': 'application/json', ...authHeaders() },
    body: JSON.stringify({ subscribedToId }),
  });
  return handleRes(res);
}

export async function getPlaylists(creatorId = 0) {
  const headers = creatorId ? { 'X-Creator-Id': String(creatorId) } : {};
  const res = await fetch(`${BASE}/api/playlists`, { headers });
  return handleRes(res);
}

export async function getComments(videoId) {
  const res = await fetch(`${BASE}/api/videos/${videoId}/comments`);
  return handleRes(res);
}

export async function likeComment(videoId, commentId, isLiked) {
  const res = await fetch(`${BASE}/api/videos/${videoId}/comments/${commentId}/like`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json', ...authHeaders() },
    body: JSON.stringify({ isLiked }),
  });
  return handleRes(res);
}

export async function postComment(videoId, content) {
  const res = await fetch(`${BASE}/api/videos/${videoId}/comments`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      ...authHeaders(),
    },
    body: JSON.stringify({ content }),
  });
  return handleRes(res);
}

export async function getSubscriptions(userId = 0) {
  const headers = userId ? { 'X-User-Id': String(userId) } : {};
  const res = await fetch(`${BASE}/api/subscriptions`, { headers });
  return handleRes(res);
}

export default {
  getVideos,
  getVideo,
  uploadVideo,
  createPlaylist,
  addVideoToPlaylist,
  getPlaylists,
  getPlaylistVideos,
  removeVideoFromPlaylist,
  likeVideo,
  subscribeTo,
  unsubscribeFrom,
  getSubscriptions,
  getComments,
  postComment,
  likeComment,
  updateVideoCategory,
};
