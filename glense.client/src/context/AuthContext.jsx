/* eslint-disable react/prop-types */
import { createContext, useContext, useState, useEffect } from 'react';
import { authService } from '../services/authService';

const AuthContext = createContext(null);

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
};

export const AuthProvider = ({ children }) => {
  const [user, setUser] = useState(null);
  const [isAuthenticated, setIsAuthenticated] = useState(false);
  const [isLoading, setIsLoading] = useState(true);

  const logout = () => {
    authService.logout();
    setUser(null);
    setIsAuthenticated(false);
  };

  useEffect(() => {
    // Check if user is already logged in and token is still valid
    const currentUser = authService.getCurrentUser();
    const token = authService.getToken();
    if (currentUser && token) {
      // Verify token is still accepted by the server
      const baseUrl = import.meta.env.VITE_API_URL || 'http://localhost:5050';
      fetch(`${baseUrl}/api/chats?pageSize=1`, { headers: { 'Authorization': `Bearer ${token}` } })
        .then(res => {
          if (res.ok) {
            setUser(currentUser);
            setIsAuthenticated(true);
          } else {
            authService.logout();
          }
        })
        .catch(() => {
          // Network error - keep auth state, user might be offline
          setUser(currentUser);
          setIsAuthenticated(true);
        })
        .finally(() => setIsLoading(false));
      return;
    }
    setIsLoading(false);

    // Listen for unauthorized events
    const handleUnauthorized = () => {
      logout();
    };
    window.addEventListener('unauthorized', handleUnauthorized);

    return () => {
      window.removeEventListener('unauthorized', handleUnauthorized);
    };
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const login = async (username, password) => {
    try {
      const response = await authService.login(username, password);
      setUser(response.user);
      setIsAuthenticated(true);
      return { success: true, data: response };
    } catch (error) {
      return {
        success: false,
        error: error.message || 'Login failed. Please check your credentials.'
      };
    }
  };

  const register = async (username, email, password) => {
    try {
      const response = await authService.register(username, email, password);
      setUser(response.user);
      setIsAuthenticated(true);
      return { success: true, data: response };
    } catch (error) {
      return {
        success: false,
        error: error.message || 'Registration failed. Please try again.'
      };
    }
  };

  const value = {
    user,
    isAuthenticated,
    isLoading,
    login,
    register,
    logout,
  };

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
};
