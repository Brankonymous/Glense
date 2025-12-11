# Glense Account Microservice

This is the Account microservice for the Glense streaming platform. It handles user authentication, profile management, and notifications.

## Features

- **Authentication**
  - User registration with email and password
  - Login with username or email
  - JWT-based authentication
  - Password hashing with BCrypt

- **Profile Management**
  - Get user profile
  - Update profile information
  - Soft delete account

- **Notifications**
  - Create notifications for users
  - Get user notifications (with pagination)
  - Mark notifications as read
  - Get unread count

## Tech Stack

- .NET 8.0
- PostgreSQL 16
- Entity Framework Core
- JWT Authentication
- BCrypt for password hashing
- Docker & Docker Compose

## Database Schema

### Users Table
- `id` (UUID) - Primary key
- `username` (VARCHAR(50)) - Unique
- `password_hash` (TEXT)
- `email` (VARCHAR(100)) - Unique
- `profile_picture_url` (TEXT) - Nullable
- `account_type` (VARCHAR(50)) - user, creator, admin
- `created_at` (TIMESTAMP)
- `updated_at` (TIMESTAMP) - Nullable
- `is_active` (BOOLEAN)
- `is_verified` (BOOLEAN)

### Notifications Table
- `id` (UUID) - Primary key
- `user_id` (UUID) - Foreign key to users
- `title` (VARCHAR(100))
- `message` (TEXT)
- `type` (VARCHAR(50))
- `is_read` (BOOLEAN)
- `related_entity_id` (UUID) - Nullable
- `created_at` (TIMESTAMP)

## API Endpoints

### Authentication
- `POST /api/auth/register` - Register a new user
- `POST /api/auth/login` - Login user

### Profile
- `GET /api/profile/me` - Get current user profile (requires auth)
- `GET /api/profile/{userId}` - Get user by ID
- `PUT /api/profile/me` - Update current user profile (requires auth)
- `DELETE /api/profile/me` - Deactivate account (requires auth)

### Notifications
- `GET /api/notification` - Get user notifications (requires auth)
- `GET /api/notification/unread-count` - Get unread count (requires auth)
- `PUT /api/notification/{id}/read` - Mark notification as read (requires auth)
- `PUT /api/notification/read-all` - Mark all as read (requires auth)

### Health
- `GET /health` - Service health check

## Running Locally

### Prerequisites
- .NET 8.0 SDK
- PostgreSQL 16
- Docker & Docker Compose (optional)

### Option 1: Using Docker Compose

```bash
# From the root of the project
docker-compose up account_service postgres_account
```

The service will be available at `http://localhost:5001`

### Option 2: Run Manually

1. **Setup PostgreSQL database**
```bash
# Connect to PostgreSQL
psql -U postgres

# Run the schema
\i services/Glense.AccountService/database/schema.sql
```

2. **Update connection string**
Edit `appsettings.Development.json` with your PostgreSQL connection details.

3. **Run migrations**
```bash
cd services/Glense.AccountService
dotnet ef database update
```

4. **Run the service**
```bash
dotnet run
```

## Environment Variables

Create a `.env` file based on `.env.example`:

```env
POSTGRES_HOST=localhost
POSTGRES_PORT=5432
POSTGRES_DB=glense_account
POSTGRES_USER=glense
POSTGRES_PASSWORD=glense123

JWT_SECRET_KEY=YourSuperSecretKeyThatIsAtLeast32CharactersLongForHS256Algorithm
JWT_ISSUER=GlenseAccountService
JWT_AUDIENCE=GlenseApp
```

## Testing with Swagger

Once running, visit `http://localhost:5001/swagger` to test the API endpoints.

## Example API Calls

### Register
```bash
curl -X POST http://localhost:5001/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "username": "johndoe",
    "email": "john@example.com",
    "password": "Password123!",
    "confirmPassword": "Password123!",
    "accountType": "user"
  }'
```

### Login
```bash
curl -X POST http://localhost:5001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "usernameOrEmail": "johndoe",
    "password": "Password123!"
  }'
```

### Get Profile (with JWT)
```bash
curl -X GET http://localhost:5001/api/profile/me \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

## Development Notes

- All passwords are hashed using BCrypt before storage
- JWT tokens expire after 7 days
- UUIDs are used instead of integer IDs for better distribution in microservices
- Soft deletes are implemented (accounts are deactivated, not deleted)
- CORS is enabled for all origins in development
