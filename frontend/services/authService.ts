import axios, { AxiosError } from "axios";
import type {
  LoginRequest,
  RegisterRequest,
  TokenResponse,
  RegisterResponse,
} from "@/types/auth";

// ─── Axios Instance ───────────────────────────────────────────────────────────

const api = axios.create({
  baseURL: process.env.NEXT_PUBLIC_API_URL,
  headers: { "Content-Type": "application/json" },
  withCredentials: true, // for httpOnly cookie (refresh token)
});

// ─── Error Helper ────────────────────────────────────────────────────────────

function resolveErrorMessage(error: unknown): string {
  if (error instanceof AxiosError) {
    const status = error.response?.status;
    if (status === 401) return "Неверный email или пароль";
    if (status === 409) return "Пользователь с таким email уже существует";
    if (status === 400) return "Проверьте правильность введённых данных";
    if (status && status >= 500) return "Ошибка сервера. Попробуйте позже";
  }
  return "Что-то пошло не так. Попробуйте ещё раз";
}

// ─── Auth Service ────────────────────────────────────────────────────────────

export const authService = {
  async login(data: LoginRequest): Promise<TokenResponse> {
    try {
      const response = await api.post<TokenResponse>("/api/auth/login", data);
      return response.data;
    } catch (error) {
      throw new Error(resolveErrorMessage(error));
    }
  },

  async register(data: RegisterRequest): Promise<RegisterResponse> {
    try {
      const response = await api.post<RegisterResponse>(
        "/api/auth/register",
        data
      );
      return response.data;
    } catch (error) {
      throw new Error(resolveErrorMessage(error));
    }
  },

  async refreshToken(token: string): Promise<TokenResponse> {
    try {
      const response = await api.post<TokenResponse>("/api/auth/refresh", {
        refreshToken: token,
      });
      return response.data;
    } catch (error) {
      throw new Error(resolveErrorMessage(error));
    }
  },

  async logout(token: string): Promise<void> {
    try {
      await api.post("/api/auth/logout", { refreshToken: token });
    } catch (error) {
      throw new Error(resolveErrorMessage(error));
    }
  },
};
