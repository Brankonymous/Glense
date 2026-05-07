# Glense Frontend

React + Vite single-page app for the Glense platform. Talks to the backend exclusively through the API Gateway at `http://localhost:5050`.

For a step-by-step setup walkthrough, see [../FRONTEND_SETUP.md](../FRONTEND_SETUP.md).

## Project layout

```
src/
├── App.jsx               Root router/layout
├── main.jsx              Vite entry (mounts <App/>)
├── index.css             Global styles
├── assets/               Static images / icons
├── components/           Reusable + page-level React components
├── config/
│   └── api.js            VITE_API_URL + endpoint catalogue
├── context/              React contexts (AuthContext, …)
├── css/                  Component-scoped stylesheets
├── services/
│   └── apiClient.js      Fetch wrapper with JWT injection
└── utils/                Helpers (formatters, validators, …)
```

## Scripts

From `glense.client/`:

| Script | What it does |
|--------|--------------|
| `npm install` | Install dependencies |
| `npm run dev` | Start Vite dev server on `http://localhost:5173` (HMR) |
| `npm run build` | Production build to `dist/` |
| `npm run preview` | Preview the production build locally |
| `npm run lint` | Run ESLint over the project |

## Environment variables

| Variable | Default | Purpose |
|----------|---------|---------|
| `VITE_API_URL` | `http://localhost:5050` | Base URL for REST and SignalR — should always point at the gateway |

Override locally in `glense.client/.env.local`.

## Services layer

All HTTP calls go through [src/services/apiClient.js](src/services/apiClient.js):

- Prefixes every request with `API_BASE_URL` (from [src/config/api.js](src/config/api.js)).
- Reads JWT from `localStorage` (`glense_auth_token`) and attaches `Authorization: Bearer <token>` automatically.
- Sets `Content-Type: application/json` by default.
- Throws on non-2xx responses; callers handle the error.

Endpoint paths live in [src/config/api.js](src/config/api.js). To add a new API call, add the path there and a thin wrapper module under `src/services/`.

## Auth

`AuthContext` (under [src/context](src/context)) is the single source of truth for the current user.

- After login/register, the JWT is stored under `glense_auth_token` and the user DTO under `glense_user`.
- On app load, the context restores the user from `localStorage` (no extra round-trip).
- `logout()` clears both keys.

## Real-time chat

The chat client connects to the SignalR hub at `${VITE_API_URL}/hubs/chat` (e.g. `http://localhost:5050/hubs/chat`). The gateway proxies WebSocket upgrades to the Chat service.

See [../services/Glense.ChatService/README.md](../services/Glense.ChatService/README.md) for the available hub methods.

## Build / deploy

The frontend is containerised via [Dockerfile](Dockerfile) and deployed as the `frontend` Kubernetes deployment ([../k8s/frontend-deployment.yaml](../k8s/frontend-deployment.yaml)). When started via `python3 scripts/start.py`, it is port-forwarded to `http://localhost:3000`.
