# Glense Account Service - Quick Start Guide

This guide will help you get the Account microservice up and running quickly.

## Prerequisites

- Docker Desktop installed and running
- .NET 8.0 SDK (optional, only if running without Docker)
- Git

### Using the Setup Script

```bash
# Make sure you're in the project root
cd /Glense

# Run the setup script
./scripts/setup-account-service.sh
```

This will:
1. Create the `.env` file from the example
2. Start PostgreSQL database
3. Start the Account Service
4. Show you the service URLs


## Testing the Service

### 1. Check Health

```bash
curl http://localhost:5001/health
```

Expected response:
```json
{
  "status": "healthy",
  "service": "account",
  "timestamp": "2024-12-07T..."
}
```

### 2. Open Swagger UI

Visit in your browser: `http://localhost:5001/swagger`

### 3. Register a New User

```bash
curl -X POST http://localhost:5001/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "username": "testuser",
    "email": "test@example.com",
    "password": "Test123!",
    "confirmPassword": "Test123!",
    "accountType": "user"
  }'
```

Expected response:
```json
{
  "token": "eyJhbGciOiJIUzI1NiIs...",
  "user": {
    "id": "...",
    "username": "testuser",
    "email": "test@example.com",
    ...
  },
  "expiresAt": "2024-12-14T..."
}
```

### 4. Login

```bash
curl -X POST http://localhost:5001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "usernameOrEmail": "testuser",
    "password": "Test123!"
  }'
```

### 5. Get Your Profile (Using JWT Token)

```bash
# Replace YOUR_TOKEN with the token from login/register response
curl -X GET http://localhost:5001/api/profile/me \
  -H "Authorization: Bearer YOUR_TOKEN"
```

## Stopping the Services

```bash
# Stop services
docker-compose down

# Stop and remove all data (including database)
docker-compose down -v
```

## Troubleshooting

### Port 5001 Already in Use

```bash
# Find what's using the port
lsof -i :5001

# Kill the process or change the port in docker-compose.yml
```

### PostgreSQL Connection Failed

```bash
# Check if PostgreSQL container is running
docker ps | grep postgres_account

# View PostgreSQL logs
docker logs glense_postgres_account

# Restart PostgreSQL
docker-compose restart postgres_account
```

### Service Won't Start

```bash
# View service logs
docker logs glense_account_service

# Rebuild the service
docker-compose build account_service
docker-compose up -d account_service
```

### Database Schema Not Created

```bash
# Connect to PostgreSQL
docker exec -it glense_postgres_account psql -U glense -d glense_account

# Check tables
\dt

# If tables don't exist, run schema manually
\i /docker-entrypoint-initdb.d/schema.sql
```

## API Endpoints Summary

- **POST** `/api/auth/register` - Register new user
- **POST** `/api/auth/login` - Login user
- **GET** `/api/profile/me` - Get current user (requires auth)
- **PUT** `/api/profile/me` - Update profile (requires auth)
- **GET** `/api/profile/{userId}` - Get user by ID
- **GET** `/api/notification` - Get notifications (requires auth)
- **PUT** `/api/notification/{id}/read` - Mark as read (requires auth)
- **GET** `/health` - Health check

## Database Access

To access the PostgreSQL database directly:

```bash
# Using Docker
docker exec -it glense_postgres_account psql -U glense -d glense_account

# Using local psql
psql -h localhost -p 5432 -U glense -d glense_account
# Password: glense123
```