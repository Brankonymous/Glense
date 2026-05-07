# Glense Frontend

React 18 + Vite SPA for Glense. Material UI for components, `react-router-dom` v7 for routing, `react-player` for video playback, plain `fetch`-based API client with JWT injection.

## Quick start

```bash
npm install
npm run dev          # http://localhost:5173
npm run build
npm run preview
npm run lint
```

The dev server expects the API gateway at `http://localhost:5050` — start the backend first (see [../docs/SETUP.md](../docs/SETUP.md)).

## Project layout

```
src/
  App.jsx                React Router config
  main.jsx               Entry point, mounts <App/>
  config/api.js          Base URLs (reads VITE_* env vars)
  services/
    apiClient.js         fetch wrapper: attaches Bearer token, handles 401 by clearing storage
    authService.js       login / register / logout
    profileService.js    profile + search
    notificationService.js
  context/AuthContext.jsx   Auth state, persisted in localStorage
  components/
    Navbar.jsx           Top bar + Searchbar
    Searchbar.jsx
    SearchResults.jsx    Combined videos + channels result page
    Feed.jsx             Home grid of videos
    VideoCard.jsx, VideoStream.jsx, VideoComments.jsx
    Upload.jsx           multipart upload form
    ChannelDetail.jsx, ChannelCard.jsx
    Playlists.jsx, PlaylistDetail.jsx
    Chat/                Chat list + room (SignalR)
    Donations/           Donation modal + history
    Sidebar.jsx, SignInPopup.jsx
  css/, assets/, utils/
```

## Routes (see [App.jsx](src/App.jsx))

| Path | Component |
|------|-----------|
| `/` | `Feed` |
| `/search/:searchTerm` | `SearchResults` |
| `/video/:id` | `VideoStream` |
| `/channel/:id` | `ChannelDetail` |
| `/upload` | `Upload` |
| `/playlists` | `Playlists` |
| `/playlists/:id` | `PlaylistDetail` |
| `/chat/:id` | `Chat` |
| `/donations` | `Donations` |

## Auth lifecycle

- Token returned by `POST /api/auth/login` or `/api/auth/register` is stored in `localStorage["glense_auth_token"]`; the user DTO is stored in `localStorage["glense_user"]`.
- Every outbound request goes through [`services/apiClient.js`](src/services/apiClient.js), which adds `Authorization: Bearer <token>` when a token is present.
- A `401` response clears both storage keys and triggers a redirect to login (handled by [`AuthContext.jsx`](src/context/AuthContext.jsx)).

## Environment variables

Create a `.env` file in this folder if you need overrides:

| Variable | Default | Used for |
|----------|---------|----------|
| `VITE_API_URL` | `http://localhost:5050` | API gateway base URL — preferred |
| `VITE_ACCOUNT_API_URL` | (legacy) | Direct Account URL — only when bypassing the gateway |

`VITE_*` variables are exposed at build time. Restart `npm run dev` after editing.

## Related docs

- High-level: [../README.md](../README.md)
- Setup: [../docs/SETUP.md](../docs/SETUP.md)
- Auth & token shape: [../docs/flows/02-login-and-jwt.md](../docs/flows/02-login-and-jwt.md)
- Search: [../docs/flows/04-search.md](../docs/flows/04-search.md)
- Real-time chat: [../docs/flows/07-chat-realtime.md](../docs/flows/07-chat-realtime.md)
