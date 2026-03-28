import axios, { AxiosError } from "axios";
import type {
  AdCampaign,
  CreateAdCampaignRequest,
  UpdateAdCampaignRequest,
} from "@/types/adCampaign";

// ─── Axios Instance ───────────────────────────────────────────────────────────

const api = axios.create({
  baseURL: process.env.NEXT_PUBLIC_API_URL,
  headers: { "Content-Type": "application/json" },
  withCredentials: true,
});

// ─── Error Types ─────────────────────────────────────────────────────────────

export type AdCampaignErrorCode =
  | "unauthorized"
  | "notFound"
  | "conflict"
  | "badRequest"
  | "serverError"
  | "unknown";

export class AdCampaignServiceError extends Error {
  constructor(public readonly code: AdCampaignErrorCode) {
    super(code);
    this.name = "AdCampaignServiceError";
  }
}

function resolveErrorCode(error: unknown): AdCampaignErrorCode {
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

// ─── Ad Campaign Service ──────────────────────────────────────────────────────

export const adCampaignService = {
  async getMyCampaigns(): Promise<AdCampaign[]> {
    try {
      const response = await api.get<AdCampaign[]>("/api/ad-campaigns/my");
      return response.data;
    } catch (error) {
      throw new AdCampaignServiceError(resolveErrorCode(error));
    }
  },

  async createCampaign(data: CreateAdCampaignRequest): Promise<{ campaignId: string }> {
    try {
      const response = await api.post<{ campaignId: string }>("/api/ad-campaigns", data);
      return response.data;
    } catch (error) {
      throw new AdCampaignServiceError(resolveErrorCode(error));
    }
  },

  async updateCampaign(id: string, data: UpdateAdCampaignRequest): Promise<void> {
    try {
      await api.put(`/api/ad-campaigns/${id}`, data);
    } catch (error) {
      throw new AdCampaignServiceError(resolveErrorCode(error));
    }
  },

  async deleteCampaign(id: string): Promise<void> {
    try {
      await api.delete(`/api/ad-campaigns/${id}`);
    } catch (error) {
      throw new AdCampaignServiceError(resolveErrorCode(error));
    }
  },

  async getActiveAds(params: {
    isAuthenticated: boolean;
    isPartner: boolean;
    targetCategory?: string;
  }): Promise<AdCampaign[]> {
    try {
      const response = await api.get<AdCampaign[]>("/api/ads", { params });
      return response.data;
    } catch (error) {
      throw new AdCampaignServiceError(resolveErrorCode(error));
    }
  },
};
