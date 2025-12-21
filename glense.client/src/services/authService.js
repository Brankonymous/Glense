import { apiClient } from './apiClient';
import { API_ENDPOINTS, TOKEN_KEY, USER_KEY } from '../config/api';

class AuthService {
  async login(username, password) {
    const response = await apiClient.post(API_ENDPOINTS.AUTH.LOGIN, {
      usernameOrEmail: username,
      password,
    });

    // Store token and user info
    if (response.token) {
      localStorage.setItem(TOKEN_KEY, response.token);
      localStorage.setItem(USER_KEY, JSON.stringify(response.user));
    }

    return response;
  }

  async register(username, email, password) {
    const response = await apiClient.post(API_ENDPOINTS.AUTH.REGISTER, {
      username,
      email,
      password,
      confirmPassword: password,
      accountType: 'user',
    });

    // Store token and user info
    if (response.token) {
      localStorage.setItem(TOKEN_KEY, response.token);
      localStorage.setItem(USER_KEY, JSON.stringify(response.user));
    }

    return response;
  }

  logout() {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(USER_KEY);
  }

  getCurrentUser() {
    const userStr = localStorage.getItem(USER_KEY);
    if (userStr) {
      try {
        return JSON.parse(userStr);
      } catch (e) {
        return null;
      }
    }
    return null;
  }

  getToken() {
    return localStorage.getItem(TOKEN_KEY);
  }

  isAuthenticated() {
    return !!this.getToken();
  }
}

export const authService = new AuthService();
