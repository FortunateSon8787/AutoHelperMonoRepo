import axios, { AxiosError } from "axios";
import type {
  LoginRequest,
  RegisterRequest,
  RegisterResponse,
} from "@/types/auth";

// ─── Axios Instance ───────────────────────────────────────────────────────────

const api = axios.create({
  baseURL: process.env.NEXT_PUBLIC_API_URL,
  headers: { "Content-Type": "application/json" },
  withCredentials: true, // for httpOnly cookie (refresh token)
});

// ─── Error Types ─────────────────────────────────────────────────────────────

export type AuthErrorCode =
  | "invalidCredentials"
  | "emailTaken"
  | "badRequest"
  | "serverError"
  | "unknown";

export class AuthServiceError extends Error {
  constructor(public readonly code: AuthErrorCode) {
    super(code);
    this.name = "AuthServiceError";
  }
}

function resolveErrorCode(error: unknown): AuthErrorCode {
  if (error instanceof AxiosError) {
    const status = error.response?.status;
    if (status === 401) return "invalidCredentials";
    if (status === 409) return "emailTaken";
    if (status === 400) return "badRequest";
    if (status !== undefined && status >= 500) return "serverError";
  }
  return "unknown";
}

// ─── Auth Service ────────────────────────────────────────────────────────────

export const authService = {
  async login(data: LoginRequest): Promise<void> {
    try {
      await api.post("/api/auth/login", data);
    } catch (error) {
      throw new AuthServiceError(resolveErrorCode(error));
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
      throw new AuthServiceError(resolveErrorCode(error));
    }
  },

  async refreshToken(): Promise<void> {
    try {
      await api.post("/api/auth/refresh");
    } catch (error) {
      throw new AuthServiceError(resolveErrorCode(error));
    }
  },

  async logout(): Promise<void> {
    try {
      await api.post("/api/auth/logout");
    } catch (error) {
      throw new AuthServiceError(resolveErrorCode(error));
    }
  },
};
