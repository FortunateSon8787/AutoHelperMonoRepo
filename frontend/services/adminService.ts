import axios, { AxiosError } from "axios";
import type { AdminCustomer, AdminCustomerListResponse } from "@/types/admin";

// ─── Axios Instance ───────────────────────────────────────────────────────────

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
    search?: string
  ): Promise<AdminCustomerListResponse> {
    try {
      const params: Record<string, string | number> = { page, pageSize };
      if (search) params.search = search;
      const response = await api.get<AdminCustomerListResponse>(
        "/api/admin/customers",
        { params }
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
};
