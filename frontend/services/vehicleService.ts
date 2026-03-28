import axios, { AxiosError } from "axios";
import type {
  Vehicle,
  VehicleOwner,
  PublicVehicle,
  CreateVehicleRequest,
  UpdateVehicleRequest,
  UpdateVehicleStatusRequest,
} from "@/types/vehicle";

// ─── Authenticated Axios Instance ─────────────────────────────────────────────

const api = axios.create({
  baseURL: process.env.NEXT_PUBLIC_API_URL,
  headers: { "Content-Type": "application/json" },
  withCredentials: true,
});

// ─── Error Types ─────────────────────────────────────────────────────────────

export type VehicleErrorCode =
  | "unauthorized"
  | "notFound"
  | "conflict"
  | "badRequest"
  | "serverError"
  | "unknown";

export class VehicleServiceError extends Error {
  constructor(public readonly code: VehicleErrorCode) {
    super(code);
    this.name = "VehicleServiceError";
  }
}

function resolveErrorCode(error: unknown): VehicleErrorCode {
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

// ─── Vehicle Service ──────────────────────────────────────────────────────────

export const vehicleService = {
  // Public SSR — no auth required
  async getOwnerByVin(vin: string): Promise<VehicleOwner> {
    const res = await fetch(
      `${process.env.NEXT_PUBLIC_API_URL}/api/vehicles/${encodeURIComponent(vin)}/owner`,
      { next: { revalidate: 60 } }
    );

    if (res.status === 404) throw new Error("not_found");
    if (!res.ok) throw new Error("server_error");

    return res.json() as Promise<VehicleOwner>;
  },

  async getByVin(vin: string): Promise<PublicVehicle> {
    const res = await fetch(
      `${process.env.NEXT_PUBLIC_API_URL}/api/vehicles/${encodeURIComponent(vin)}/details`,
      { next: { revalidate: 60 } }
    );

    if (res.status === 404) throw new Error("not_found");
    if (!res.ok) throw new Error("server_error");

    return res.json() as Promise<PublicVehicle>;
  },

  // Authenticated CRUD
  async getMyVehicles(): Promise<Vehicle[]> {
    try {
      const response = await api.get<Vehicle[]>("/api/vehicles");
      return response.data;
    } catch (error) {
      throw new VehicleServiceError(resolveErrorCode(error));
    }
  },

  async getById(id: string): Promise<Vehicle> {
    try {
      const response = await api.get<Vehicle>(`/api/vehicles/${id}`);
      return response.data;
    } catch (error) {
      throw new VehicleServiceError(resolveErrorCode(error));
    }
  },

  async create(data: CreateVehicleRequest): Promise<{ vehicleId: string }> {
    try {
      const response = await api.post<{ vehicleId: string }>("/api/vehicles", data);
      return response.data;
    } catch (error) {
      throw new VehicleServiceError(resolveErrorCode(error));
    }
  },

  async update(id: string, data: UpdateVehicleRequest): Promise<void> {
    try {
      await api.put(`/api/vehicles/${id}`, data);
    } catch (error) {
      throw new VehicleServiceError(resolveErrorCode(error));
    }
  },

  async updateStatus(id: string, data: UpdateVehicleStatusRequest): Promise<void> {
    try {
      const formData = new FormData();
      formData.append("status", data.status);
      if (data.partnerName) formData.append("partnerName", data.partnerName);
      if (data.document) formData.append("document", data.document);
      await api.put(`/api/vehicles/${id}/status`, formData, {
        headers: { "Content-Type": "multipart/form-data" },
      });
    } catch (error) {
      throw new VehicleServiceError(resolveErrorCode(error));
    }
  },
};
