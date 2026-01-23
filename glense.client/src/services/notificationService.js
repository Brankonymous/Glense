import { apiClient } from './apiClient';
import { API_ENDPOINTS } from '../config/api';

class NotificationService {
  async getNotifications() {
    const response = await apiClient.get(API_ENDPOINTS.NOTIFICATIONS.GET_ALL);
    return response;
  }

  async markAsRead(notificationId) {
    const response = await apiClient.put(
      API_ENDPOINTS.NOTIFICATIONS.MARK_READ(notificationId)
    );
    return response;
  }
}

export const notificationService = new NotificationService();
