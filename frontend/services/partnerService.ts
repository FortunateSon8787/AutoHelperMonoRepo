import axios, { AxiosError } from "axios";
import type {
  PartnerProfile,
  RegisterPartnerRequest,
  UpdatePartnerProfileRequest,
} from "@/types/partner";

// ─── Axios Instance ───────────────────────────────────────────────────────────

const api = axios.create({
  baseURL: process.env.NEXT_PUBLIC_API_URL,
  headers: { "Content-Type": "application/json" },
  withCredentials: true,
});

// ─── Error Types ─────────────────────────────────────────────────────────────

export type PartnerErrorCode =
  | "unauthorized"
  | "notFound"
  | "conflict"
  | "badRequest"
  | "serverError"
  | "unknown";

export class PartnerServiceError extends Error {
  constructor(public readonly code: PartnerErrorCode) {
    super(code);
    this.name = "PartnerServiceError";
  }
}

function resolveErrorCode(error: unknown): PartnerErrorCode {
  if (error instanceof AxiosError) {
    const status = error.response?.status;
    if (status === 401) return "unauthorized";
    if (status === 404) return "notFound";
    if (status === 409) return "conflict";
    if (status === 400) return "badRequest";
    if (status !== undefined && status >= 500) return "serverError";
  }
  return "unknown";
}

// ─── Partner Service ─────────────────────────────────────────────────────────

export const partnerService = {
  async registerPartner(data: RegisterPartnerRequest): Promise<{ partnerId: string }> {
    try {
      const response = await api.post<{ partnerId: string }>("/api/partners/register", data);
      return response.data;
    } catch (error) {
      throw new PartnerServiceError(resolveErrorCode(error));
    }
  },

  async getMyProfile(): Promise<PartnerProfile> {
    try {
      const response = await api.get<PartnerProfile>("/api/partners/me");
      return response.data;
    } catch (error) {
      throw new PartnerServiceError(resolveErrorCode(error));
    }
  },

  async updateMyProfile(data: UpdatePartnerProfileRequest): Promise<void> {
    try {
      await api.put("/api/partners/me", data);
    } catch (error) {
      throw new PartnerServiceError(resolveErrorCode(error));
    }
  },
};
