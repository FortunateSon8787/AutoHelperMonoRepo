import axios from "axios";
import type { AxiosInstance, InternalAxiosRequestConfig } from "axios";

// Extended config type to track retry attempts
interface RetryableRequestConfig extends InternalAxiosRequestConfig {
  _retry?: boolean;
}

function createApiClient(refreshUrl: string): AxiosInstance {
  const instance = axios.create({
    baseURL: process.env.NEXT_PUBLIC_API_URL,
    headers: { "Content-Type": "application/json" },
    withCredentials: true,
  });

  instance.interceptors.response.use(
    (response) => response,
    async (error) => {
      const originalRequest = error.config as RetryableRequestConfig | undefined;

      // Only attempt refresh once per request, only on 401, skip refresh endpoint itself
      if (
        !originalRequest ||
        error.response?.status !== 401 ||
        originalRequest._retry ||
        originalRequest.url === refreshUrl
      ) {
        return Promise.reject(error);
      }

      originalRequest._retry = true;

      try {
        await instance.post(refreshUrl);
        return instance(originalRequest);
      } catch {
        // Refresh failed — caller receives the original 401
        return Promise.reject(error);
      }
    }
  );

  return instance;
}

// Client API instance (refreshes via /api/auth/refresh)
export const apiClient = createApiClient("/api/auth/refresh");

// Admin API instance (refreshes via /api/admin/auth/refresh)
export const adminApiClient = createApiClient("/api/admin/auth/refresh");
