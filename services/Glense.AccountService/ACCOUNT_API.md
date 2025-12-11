# Account Service API Documentation

Base URL: `http://localhost:5001`

## Authentication

Most endpoints require a JWT token. Include it in the Authorization header:
```
Authorization: Bearer YOUR_JWT_TOKEN
```

## Endpoints

### 1. Register User

**POST** `/api/auth/register`

Register a new user account.

**Request Body:**
```json
{
  "username": "johndoe",
  "email": "john@example.com",
  "password": "SecurePassword123!",
  "confirmPassword": "SecurePassword123!",
  "accountType": "user"
}
```

**Response (200 OK):**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "user": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "username": "johndoe",
    "email": "john@example.com",
    "profilePictureUrl": null,
    "accountType": "user",
    "createdAt": "2024-12-07T12:00:00Z",
    "isVerified": false
  },
  "expiresAt": "2024-12-14T12:00:00Z"
}
```

**Error Responses:**
- `400 Bad Request` - Username or email already exists
- `400 Bad Request` - Validation errors

---

### 2. Login

**POST** `/api/auth/login`

Login with username/email and password.

**Request Body:**
```json
{
  "usernameOrEmail": "johndoe",
  "password": "SecurePassword123!"
}
```

**Response (200 OK):**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "user": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "username": "johndoe",
    "email": "john@example.com",
    "profilePictureUrl": null,
    "accountType": "user",
    "createdAt": "2024-12-07T12:00:00Z",
    "isVerified": false
  },
  "expiresAt": "2024-12-14T12:00:00Z"
}
```

**Error Responses:**
- `401 Unauthorized` - Invalid credentials

---

### 3. Get Current User Profile

**GET** `/api/profile/me`

Get the authenticated user's profile.

**Headers:**
```
Authorization: Bearer YOUR_JWT_TOKEN
```

**Response (200 OK):**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "username": "johndoe",
  "email": "john@example.com",
  "profilePictureUrl": "https://example.com/avatar.jpg",
  "accountType": "user",
  "createdAt": "2024-12-07T12:00:00Z",
  "isVerified": true
}
```

**Error Responses:**
- `401 Unauthorized` - Missing or invalid token
- `404 Not Found` - User not found

---

### 4. Get User by ID

**GET** `/api/profile/{userId}`

Get a user's public profile by their ID.

**Parameters:**
- `userId` (path) - The UUID of the user

**Response (200 OK):**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "username": "johndoe",
  "email": "john@example.com",
  "profilePictureUrl": "https://example.com/avatar.jpg",
  "accountType": "creator",
  "createdAt": "2024-12-07T12:00:00Z",
  "isVerified": true
}
```

**Error Responses:**
- `404 Not Found` - User not found or inactive

---

### 5. Update Profile

**PUT** `/api/profile/me`

Update the authenticated user's profile.

**Headers:**
```
Authorization: Bearer YOUR_JWT_TOKEN
```

**Request Body:**
```json
{
  "username": "newusername",
  "email": "newemail@example.com",
  "profilePictureUrl": "https://example.com/new-avatar.jpg"
}
```

**Note:** All fields are optional. Only include fields you want to update.

**Response (200 OK):**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "username": "newusername",
  "email": "newemail@example.com",
  "profilePictureUrl": "https://example.com/new-avatar.jpg",
  "accountType": "user",
  "createdAt": "2024-12-07T12:00:00Z",
  "isVerified": false
}
```

**Error Responses:**
- `400 Bad Request` - Username or email already taken
- `401 Unauthorized` - Missing or invalid token
- `404 Not Found` - User not found

---

### 6. Delete Account

**DELETE** `/api/profile/me`

Soft delete (deactivate) the authenticated user's account.

**Headers:**
```
Authorization: Bearer YOUR_JWT_TOKEN
```

**Response (200 OK):**
```json
{
  "message": "Account deactivated successfully"
}
```

**Error Responses:**
- `401 Unauthorized` - Missing or invalid token
- `404 Not Found` - User not found

---

### 7. Get Notifications

**GET** `/api/notification`

Get notifications for the authenticated user.

**Headers:**
```
Authorization: Bearer YOUR_JWT_TOKEN
```

**Query Parameters:**
- `isRead` (optional) - Filter by read status (true/false)
- `skip` (optional) - Number of items to skip (default: 0)
- `take` (optional) - Number of items to return (default: 20, max: 100)

**Example:**
```
GET /api/notification?isRead=false&skip=0&take=10
```

**Response (200 OK):**
```json
[
  {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "title": "New Subscriber",
    "message": "User123 subscribed to your channel",
    "type": "subscription",
    "isRead": false,
    "relatedEntityId": "9ea85f64-5717-4562-b3fc-2c963f66afa6",
    "createdAt": "2024-12-07T12:00:00Z"
  }
]
```

**Error Responses:**
- `401 Unauthorized` - Missing or invalid token

---

### 8. Get Unread Notification Count

**GET** `/api/notification/unread-count`

Get the count of unread notifications.

**Headers:**
```
Authorization: Bearer YOUR_JWT_TOKEN
```

**Response (200 OK):**
```json
{
  "count": 5
}
```

**Error Responses:**
- `401 Unauthorized` - Missing or invalid token

---

### 9. Mark Notification as Read

**PUT** `/api/notification/{notificationId}/read`

Mark a specific notification as read.

**Headers:**
```
Authorization: Bearer YOUR_JWT_TOKEN
```

**Parameters:**
- `notificationId` (path) - The UUID of the notification

**Response (200 OK):**
```json
{
  "message": "Notification marked as read"
}
```

**Error Responses:**
- `401 Unauthorized` - Missing or invalid token
- `404 Not Found` - Notification not found

---

### 10. Mark All Notifications as Read

**PUT** `/api/notification/read-all`

Mark all notifications for the authenticated user as read.

**Headers:**
```
Authorization: Bearer YOUR_JWT_TOKEN
```

**Response (200 OK):**
```json
{
  "message": "All notifications marked as read"
}
```

**Error Responses:**
- `401 Unauthorized` - Missing or invalid token

---

### 11. Health Check

**GET** `/health`

Check the health status of the service.

**Response (200 OK):**
```json
{
  "status": "healthy",
  "service": "account",
  "timestamp": "2024-12-07T12:00:00Z"
}
```

---

## Error Response Format

All error responses follow this format:

```json
{
  "message": "Error description here"
}
```

Common HTTP status codes:
- `200 OK` - Success
- `400 Bad Request` - Invalid request data
- `401 Unauthorized` - Missing or invalid authentication
- `404 Not Found` - Resource not found
- `500 Internal Server Error` - Server error

---

## JWT Token

The JWT token contains the following claims:

```json
{
  "sub": "user-id-uuid",
  "unique_name": "username",
  "email": "user@example.com",
  "account_type": "user",
  "jti": "token-id",
  "exp": 1234567890,
  "iss": "GlenseAccountService",
  "aud": "GlenseApp"
}
```

Token expires after 7 days.

---

## Account Types

- `user` - Regular user
- `creator` - Content creator
- `admin` - Administrator

---

## Notification Types

- `subscription` - New subscription notification
- `donation` - Donation received
- `comment` - New comment on video
- `system` - System notification

---

## Testing with cURL

### Register and Login Flow

```bash
# 1. Register
RESPONSE=$(curl -s -X POST http://localhost:5001/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "username": "testuser",
    "email": "test@example.com",
    "password": "Test123!",
    "confirmPassword": "Test123!",
    "accountType": "user"
  }')

# 2. Extract token
TOKEN=$(echo $RESPONSE | jq -r '.token')

# 3. Get profile
curl -X GET http://localhost:5001/api/profile/me \
  -H "Authorization: Bearer $TOKEN"

# 4. Update profile
curl -X PUT http://localhost:5001/api/profile/me \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "profilePictureUrl": "https://example.com/avatar.jpg"
  }'
```

