# Frontend 

# Frontend Integration with Account Microservice

The frontend is now connected to the Account microservice. Here's how to run everything:

## Running the Application

### 1. Start the Account Microservice (Backend)

```bash
# From the project root
docker-compose up account_service postgres_account
```

### 2. Start the Frontend (Development)

```bash
# In a new terminal, from the project root
cd glense.client
npm install  
npm run dev
```

The frontend will be available at http://localhost:50653 (or the port shown in terminal)


## Testing the Integration

1. Start both backend and frontend as described above
2. Click "Sign-in" in the navbar
3. Try registering a new user:
   - Username: `testuser`
   - Email: `test@example.com`
   - Password: `password123`
4. You should be logged in automatically after registration
5. Try logging out via the profile menu (account icon)
6. Try logging in again with the same credentials

## Environment Configuration

The frontend API URL can be configured via environment variable:
- Default: `http://localhost:5001`
- Custom: Set `VITE_ACCOUNT_API_URL` in a `.env` file

Example `.env` file:
```env
VITE_ACCOUNT_API_URL=http://localhost:5001
```

## Token Storage

JWT tokens are stored in `localStorage`:
- Token key: `glense_auth_token`
- User data key: `glense_user`

Tokens are automatically:
- Saved on successful login/register
- Sent with all API requests via Authorization header
- Cleared on logout or 401 Unauthorized responses