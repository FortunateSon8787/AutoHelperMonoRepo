// ─── Vehicle Owner ────────────────────────────────────────────────────────────

export interface VehicleOwner {
  ownerId: string;
  name: string;
  contacts: string | null;
  avatarUrl: string | null;
}

// ─── Vehicle (authenticated CRUD) ─────────────────────────────────────────────

export type VehicleStatus = "Active" | "ForSale" | "InRepair" | "Recycled" | "Dismantled";

export interface Vehicle {
  id: string;
  vin: string;
  brand: string;
  model: string;
  year: number;
  color: string | null;
  mileage: number;
  status: VehicleStatus;
  ownerId: string;
  partnerName: string | null;
  documentUrl: string | null;
}

export interface CreateVehicleRequest {
  vin: string;
  brand: string;
  model: string;
  year: number;
  color: string | null;
  mileage: number;
}

export interface UpdateVehicleRequest {
  brand: string;
  model: string;
  year: number;
  color: string | null;
  mileage: number;
}

export interface UpdateVehicleStatusRequest {
  status: VehicleStatus;
  partnerName?: string;
  document?: File;
}
