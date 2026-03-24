import type { VehicleOwner } from "@/types/vehicle";

// ─── Vehicle Service ──────────────────────────────────────────────────────────

export const vehicleService = {
  async getOwnerByVin(vin: string): Promise<VehicleOwner> {
    const res = await fetch(
      `${process.env.NEXT_PUBLIC_API_URL}/api/vehicles/${encodeURIComponent(vin)}/owner`,
      { next: { revalidate: 60 } }
    );

    if (res.status === 404) throw new Error("not_found");
    if (!res.ok) throw new Error("server_error");

    return res.json() as Promise<VehicleOwner>;
  },
};
