import axios, { AxiosError } from "axios";
import type {
  AdminAdCampaign,
  AdminAdCampaignListResponse,
  AdminCustomer,
  AdminCustomerListResponse,
  AdminVehicle,
  AdminVehicleListResponse,
} from "@/types/admin";

// ─── Axios Instance ───────────────────────────────────────────────────────────

// withCredentials=true обязателен, чтобы браузер отправлял httpOnly auth-куки
// (adminAccessToken / adminRefreshToken), выставленные бэкендом с SameSite=Strict.
const api = axios.create({
  baseURL: process.env.NEXT_PUBLIC_API_URL,
  headers: { "Content-Type": "application/json" },
  withCredentials: true,
});

// ─── Error Types ─────────────────────────────────────────────────────────────

export type AdminServiceErrorCode =
  | "unauthorized"
  | "forbidden"
  | "notFound"
  | "badRequest"
  | "serverError"
  | "unknown";

export class AdminServiceError extends Error {
  constructor(public readonly code: AdminServiceErrorCode) {
    super(code);
    this.name = "AdminServiceError";
  }
}

function resolveErrorCode(error: unknown): AdminServiceErrorCode {
  if (error instanceof AxiosError) {
    const status = error.response?.status;
    if (status === 401) return "unauthorized";
    if (status === 403) return "forbidden";
    if (status === 404) return "notFound";
    if (status === 400) return "badRequest";
    if (status !== undefined && status >= 500) return "serverError";
  }
  return "unknown";
}

// ─── Admin Service ────────────────────────────────────────────────────────────

export const adminService = {
  async getCustomers(
    page: number,
    pageSize: number,
    search?: string,
    signal?: AbortSignal
  ): Promise<AdminCustomerListResponse> {
    try {
      const params: Record<string, string | number> = { page, pageSize };
      if (search) params.search = search;
      const response = await api.get<AdminCustomerListResponse>(
        "/api/admin/customers",
        { params, signal }
      );
      return response.data;
    } catch (error) {
      throw new AdminServiceError(resolveErrorCode(error));
    }
  },

  async getCustomerById(id: string): Promise<AdminCustomer> {
    try {
      const response = await api.get<AdminCustomer>(`/api/admin/customers/${id}`);
      return response.data;
    } catch (error) {
      throw new AdminServiceError(resolveErrorCode(error));
    }
  },

  async blockCustomer(id: string): Promise<void> {
    try {
      await api.post(`/api/admin/customers/${id}/block`);
    } catch (error) {
      throw new AdminServiceError(resolveErrorCode(error));
    }
  },

  async unblockCustomer(id: string): Promise<void> {
    try {
      await api.post(`/api/admin/customers/${id}/unblock`);
    } catch (error) {
      throw new AdminServiceError(resolveErrorCode(error));
    }
  },

  async getVehicles(
    page: number,
    pageSize: number,
    search?: string,
    signal?: AbortSignal
  ): Promise<AdminVehicleListResponse> {
    try {
      const params: Record<string, string | number> = { page, pageSize };
      if (search) params.search = search;
      const response = await api.get<AdminVehicleListResponse>(
        "/api/admin/vehicles",
        { params, signal }
      );
      return response.data;
    } catch (error) {
      throw new AdminServiceError(resolveErrorCode(error));
    }
  },

  async getVehicleById(id: string): Promise<AdminVehicle> {
    try {
      const response = await api.get<AdminVehicle>(`/api/admin/vehicles/${id}`);
      return response.data;
    } catch (error) {
      throw new AdminServiceError(resolveErrorCode(error));
    }
  },

  async getAdCampaigns(
    page: number,
    pageSize: number,
    partnerId?: string,
    signal?: AbortSignal
  ): Promise<AdminAdCampaignListResponse> {
    try {
      const params: Record<string, string | number> = { page, pageSize };
      if (partnerId) params.partnerId = partnerId;
      const response = await api.get<AdminAdCampaignListResponse>(
        "/api/admin/ad-campaigns",
        { params, signal }
      );
      return response.data;
    } catch (error) {
      throw new AdminServiceError(resolveErrorCode(error));
    }
  },

  async activateAdCampaign(id: string): Promise<void> {
    try {
      await api.post(`/api/admin/ad-campaigns/${id}/activate`);
    } catch (error) {
      throw new AdminServiceError(resolveErrorCode(error));
    }
  },

  async deactivateAdCampaign(id: string): Promise<void> {
    try {
      await api.post(`/api/admin/ad-campaigns/${id}/deactivate`);
    } catch (error) {
      throw new AdminServiceError(resolveErrorCode(error));
    }
  },

  async getAdCampaignById(id: string): Promise<AdminAdCampaign> {
    try {
      const response = await api.get<AdminAdCampaign>(`/api/admin/ad-campaigns/${id}`);
      return response.data;
    } catch (error) {
      throw new AdminServiceError(resolveErrorCode(error));
    }
  },
};
