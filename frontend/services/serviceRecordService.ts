import axios, { AxiosError } from "axios";
import type {
  ServiceRecord,
  CreateServiceRecordRequest,
  UpdateServiceRecordRequest,
} from "@/types/serviceRecord";

// ─── Authenticated Axios Instance ─────────────────────────────────────────────

const api = axios.create({
  baseURL: process.env.NEXT_PUBLIC_API_URL,
  headers: { "Content-Type": "application/json" },
  withCredentials: true,
});

// ─── Error Types ─────────────────────────────────────────────────────────────

export type ServiceRecordErrorCode =
  | "unauthorized"
  | "notFound"
  | "badRequest"
  | "serverError"
  | "unknown";

export class ServiceRecordServiceError extends Error {
  constructor(public readonly code: ServiceRecordErrorCode) {
    super(code);
    this.name = "ServiceRecordServiceError";
  }
}

function resolveErrorCode(error: unknown): ServiceRecordErrorCode {
  if (error instanceof AxiosError) {
    const status = error.response?.status;
    if (status === 401) return "unauthorized";
    if (status === 404) return "notFound";
    if (status === 400) return "badRequest";
    if (status !== undefined && status >= 500) return "serverError";
  }
  return "unknown";
}

// ─── Service Record Service ───────────────────────────────────────────────────

export const serviceRecordService = {
  // Public SSR — no auth required
  async getPublicByVin(vin: string): Promise<ServiceRecord[]> {
    const res = await fetch(
      `${process.env.NEXT_PUBLIC_API_URL}/api/vehicles/${encodeURIComponent(vin)}/service-records`,
      { next: { revalidate: 60 } }
    );

    if (res.status === 404) throw new Error("not_found");
    if (!res.ok) throw new Error("server_error");

    return res.json() as Promise<ServiceRecord[]>;
  },

  // Authenticated — get all records for owned vehicle
  async getByVehicleId(vehicleId: string): Promise<ServiceRecord[]> {
    try {
      const response = await api.get<ServiceRecord[]>(
        `/api/vehicles/${vehicleId}/service-records`
      );
      return response.data;
    } catch (error) {
      throw new ServiceRecordServiceError(resolveErrorCode(error));
    }
  },

  // Authenticated — get single record
  async getById(id: string): Promise<ServiceRecord> {
    try {
      const response = await api.get<ServiceRecord>(`/api/service-records/${id}`);
      return response.data;
    } catch (error) {
      throw new ServiceRecordServiceError(resolveErrorCode(error));
    }
  },

  // Upload PDF document first, get URL, then call create
  async uploadDocument(file: File): Promise<string> {
    try {
      const formData = new FormData();
      formData.append("document", file);
      const response = await api.post<{ documentUrl: string }>(
        "/api/service-records/document",
        formData,
        { headers: { "Content-Type": "multipart/form-data" } }
      );
      return response.data.documentUrl;
    } catch (error) {
      throw new ServiceRecordServiceError(resolveErrorCode(error));
    }
  },

  async create(
    vehicleId: string,
    data: CreateServiceRecordRequest
  ): Promise<{ serviceRecordId: string }> {
    try {
      const response = await api.post<{ serviceRecordId: string }>(
        `/api/vehicles/${vehicleId}/service-records`,
        data
      );
      return response.data;
    } catch (error) {
      throw new ServiceRecordServiceError(resolveErrorCode(error));
    }
  },

  async update(id: string, data: UpdateServiceRecordRequest): Promise<void> {
    try {
      await api.put(`/api/service-records/${id}`, data);
    } catch (error) {
      throw new ServiceRecordServiceError(resolveErrorCode(error));
    }
  },

  async delete(id: string): Promise<void> {
    try {
      await api.delete(`/api/service-records/${id}`);
    } catch (error) {
      throw new ServiceRecordServiceError(resolveErrorCode(error));
    }
  },

  // Returns the API proxy URL for streaming a service record's PDF document.
  // Use this instead of documentUrl directly — documentUrl may point to a private
  // internal storage address (e.g. http://minio:9000) that is unreachable from the browser.
  getDocumentProxyUrl(id: string): string {
    return `${process.env.NEXT_PUBLIC_API_URL}/api/service-records/${id}/document`;
  },
};
