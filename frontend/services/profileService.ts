import { AxiosError } from "axios";
import { apiClient as api } from "@/lib/apiClient";
import type { ClientProfile, UpdateProfileRequest } from "@/types/client";

// ─── Error Types ─────────────────────────────────────────────────────────────

export type ProfileErrorCode =
  | "unauthorized"
  | "notFound"
  | "badRequest"
  | "serverError"
  | "unknown";

export class ProfileServiceError extends Error {
  constructor(public readonly code: ProfileErrorCode) {
    super(code);
    this.name = "ProfileServiceError";
  }
}

function resolveErrorCode(error: unknown): ProfileErrorCode {
  if (error instanceof AxiosError) {
    const status = error.response?.status;
    if (status === 401) return "unauthorized";
    if (status === 404) return "notFound";
    if (status === 400) return "badRequest";
    if (status !== undefined && status >= 500) return "serverError";
  }
  return "unknown";
}

// ─── Profile Service ─────────────────────────────────────────────────────────

export const profileService = {
  async getMyProfile(): Promise<ClientProfile> {
    try {
      const response = await api.get<ClientProfile>("/api/clients/me");
      return response.data;
    } catch (error) {
      throw new ProfileServiceError(resolveErrorCode(error));
    }
  },

  async updateMyProfile(data: UpdateProfileRequest): Promise<void> {
    try {
      await api.put("/api/clients/me", data);
    } catch (error) {
      throw new ProfileServiceError(resolveErrorCode(error));
    }
  },
};
