import axios, { AxiosError } from "axios";
import type { ClientProfile, UpdateProfileRequest } from "@/types/client";

// ─── Axios Instance ───────────────────────────────────────────────────────────

const api = axios.create({
  baseURL: process.env.NEXT_PUBLIC_API_URL,
  headers: { "Content-Type": "application/json" },
  withCredentials: true,
});

// Attach the stored access token to every request
api.interceptors.request.use((config) => {
  const token = localStorage.getItem("accessToken");
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// ─── Error Helper ────────────────────────────────────────────────────────────

function resolveErrorMessage(error: unknown): string {
  if (error instanceof AxiosError) {
    const status = error.response?.status;
    if (status === 401) return "Необходима авторизация";
    if (status === 404) return "Профиль не найден";
    if (status === 400) return "Проверьте правильность введённых данных";
    if (status && status >= 500) return "Ошибка сервера. Попробуйте позже";
  }
  return "Что-то пошло не так. Попробуйте ещё раз";
}

// ─── Profile Service ─────────────────────────────────────────────────────────

export const profileService = {
  async getMyProfile(): Promise<ClientProfile> {
    try {
      const response = await api.get<ClientProfile>("/api/clients/me");
      return response.data;
    } catch (error) {
      throw new Error(resolveErrorMessage(error));
    }
  },

  async updateMyProfile(data: UpdateProfileRequest): Promise<void> {
    try {
      await api.put("/api/clients/me", data);
    } catch (error) {
      throw new Error(resolveErrorMessage(error));
    }
  },
};
