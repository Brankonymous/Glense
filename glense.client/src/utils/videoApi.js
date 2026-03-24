const BASE = import.meta.env.VITE_VIDEO_CATALOGUE_API || 'http://localhost:5002';

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

export async function uploadVideo(file, title, description, uploaderId = 0) {
  const fd = new FormData();
  fd.append('file', file);
  if (title) fd.append('title', title);
  if (description) fd.append('description', description);

  const res = await fetch(`${BASE}/api/videos/upload`, {
    method: 'POST',
    body: fd,
    headers: {
      ...(uploaderId ? { 'X-Uploader-Id': String(uploaderId) } : {}),
    },
  });

  return handleRes(res);
}

export async function createPlaylist(name, description, creatorId = 0) {
  const res = await fetch(`${BASE}/api/playlists`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json', ...(creatorId ? { 'X-Creator-Id': String(creatorId) } : {}) },
    body: JSON.stringify({ name, description }),
  });
  return handleRes(res);
}

export async function addVideoToPlaylist(playlistId, videoId) {
  const res = await fetch(`${BASE}/api/playlistvideos`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
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
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ playlistId, videoId }),
  });
  return handleRes(res);
}

export async function likeVideo(videoId, isLiked = true, userId = 0) {
  const res = await fetch(`${BASE}/api/videolikes`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json', ...(userId ? { 'X-User-Id': String(userId) } : {}) },
    body: JSON.stringify({ videoId, isLiked }),
  });
  return handleRes(res);
}

export async function subscribeTo(subscribedToId, userId = 0) {
  const res = await fetch(`${BASE}/api/subscriptions`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json', ...(userId ? { 'X-User-Id': String(userId) } : {}) },
    body: JSON.stringify({ subscribedToId }),
  });
  return handleRes(res);
}

export async function unsubscribeFrom(subscribedToId, userId = 0) {
  const res = await fetch(`${BASE}/api/subscriptions`, {
    method: 'DELETE',
    headers: { 'Content-Type': 'application/json', ...(userId ? { 'X-User-Id': String(userId) } : {}) },
    body: JSON.stringify({ subscribedToId }),
  });
  return handleRes(res);
}

export async function getPlaylists(creatorId = 0) {
  const headers = creatorId ? { 'X-Creator-Id': String(creatorId) } : {};
  const res = await fetch(`${BASE}/api/playlists`, { headers });
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
};
