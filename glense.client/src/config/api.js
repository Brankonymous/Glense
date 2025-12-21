// API Base URLs
export const API_BASE_URL = import.meta.env.VITE_ACCOUNT_API_URL || 'http://localhost:5001';

// API Endpoints
export const API_ENDPOINTS = {
  AUTH: {
    LOGIN: '/api/auth/login',
    REGISTER: '/api/auth/register',
  },
  PROFILE: {
    GET: '/api/profile',
    UPDATE: '/api/profile',
  },
  NOTIFICATIONS: {
    GET_ALL: '/api/notification',
    MARK_READ: (id) => `/api/notification/${id}/read`,
  },
};

// Token storage keys
export const TOKEN_KEY = 'glense_auth_token';
export const USER_KEY = 'glense_user';
