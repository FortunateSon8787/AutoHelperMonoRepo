import axios, { AxiosError } from "axios";

// ─── Axios Instance ───────────────────────────────────────────────────────────

const api = axios.create({
  baseURL: process.env.NEXT_PUBLIC_API_URL,
  headers: { "Content-Type": "application/json" },
  withCredentials: true,
});

// ─── Error Types ─────────────────────────────────────────────────────────────

export type AdminAuthErrorCode =
  | "invalidCredentials"
  | "badRequest"
  | "serverError"
  | "unknown";

export class AdminAuthServiceError extends Error {
  constructor(public readonly code: AdminAuthErrorCode) {
    super(code);
    this.name = "AdminAuthServiceError";
  }
}

function resolveErrorCode(error: unknown): AdminAuthErrorCode {
  if (error instanceof AxiosError) {
    const status = error.response?.status;
    if (status === 401) return "invalidCredentials";
    if (status === 400) return "badRequest";
    if (status !== undefined && status >= 500) return "serverError";
  }
  return "unknown";
}

// ─── Admin Auth Service ───────────────────────────────────────────────────────

export const adminAuthService = {
  async login(email: string, password: string): Promise<void> {
    try {
      await api.post("/api/admin/auth/login", { email, password });
    } catch (error) {
      throw new AdminAuthServiceError(resolveErrorCode(error));
    }
  },

  async logout(): Promise<void> {
    try {
      await api.post("/api/admin/auth/logout");
    } catch {
      // Ignore errors on logout — clear cookies regardless
    }
  },
};
