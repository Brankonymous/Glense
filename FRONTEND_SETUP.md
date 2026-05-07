# Frontend Setup

The Glense frontend is a React + Vite SPA that talks to the backend **through the API Gateway** (`http://localhost:5050`). It does not call individual microservices directly.

For an in-depth tour of the frontend project, see [glense.client/README.md](glense.client/README.md).

## Prerequisites

- Node.js v22+
- The backend stack running (gateway reachable on `http://localhost:5050`). See [DEV_QUICKSTART.md](DEV_QUICKSTART.md).

## Run the frontend (standalone dev)

```bash
cd glense.client
npm install
npm run dev
```

Vite serves the app at `http://localhost:5173` by default.

When the full stack is started via `python3 scripts/start.py` (Kubernetes mode), the frontend is port-forwarded to `http://localhost:3000` instead.

## Environment variables

The frontend reads its configuration from Vite environment variables. Defaults are sensible for local development.

| Variable | Default | Purpose |
|----------|---------|---------|
| `VITE_API_URL` | `http://localhost:5050` | Base URL for all REST and SignalR calls (always the gateway) |

To override, create `glense.client/.env.local`:

```env
VITE_API_URL=http://localhost:5050
```

## How API calls work

All HTTP calls go through [glense.client/src/services/apiClient.js](glense.client/src/services/apiClient.js), which:

1. Prefixes every request with `VITE_API_URL`.
2. Reads the JWT from `localStorage` (`glense_auth_token`) and attaches it as `Authorization: Bearer <token>`.
3. Sends `Content-Type: application/json` by default.

Endpoint paths are centralized in [glense.client/src/config/api.js](glense.client/src/config/api.js).

## Token storage

JWTs and the user object are persisted in `localStorage`:

| Key | Contents |
|-----|----------|
| `glense_auth_token` | JWT returned by `/api/auth/login` or `/api/auth/register` |
| `glense_user` | Serialized user DTO (id, username, email, accountType, …) |

`AuthContext` (under [glense.client/src/context](glense.client/src/context)) reads these on app load and exposes `login`, `logout`, and `user` to the component tree.

## Smoke test

1. Start the backend (see [DEV_QUICKSTART.md](DEV_QUICKSTART.md)).
2. `cd glense.client && npm run dev`.
3. Open the dev URL, click **Sign in**, register a user, and confirm you land logged-in.
4. Open browser dev tools → Application → Local Storage to verify `glense_auth_token` was written.
