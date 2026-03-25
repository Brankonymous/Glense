import { apiClient } from './apiClient';
import { API_ENDPOINTS } from '../config/api';

class ProfileService {
  async getProfile() {
    const response = await apiClient.get(API_ENDPOINTS.PROFILE.GET);
    return response;
  }

  async updateProfile(profileData) {
    const response = await apiClient.put(API_ENDPOINTS.PROFILE.UPDATE, profileData);
    return response;
  }

  async searchUsers(query = '', limit = 20) {
    const params = new URLSearchParams({ q: query, limit: limit.toString() });
    const response = await apiClient.get(`/api/profile/search?${params}`);
    return response;
  }

  async getUserById(userId) {
    const response = await apiClient.get(`/api/profile/${userId}`);
    return response;
  }
}

export const profileService = new ProfileService();
