// ─── Admin Vehicles ───────────────────────────────────────────────────────────

export interface AdminVehicle {
  id: string;
  vin: string;
  brand: string;
  model: string;
  year: number;
  color: string | null;
  mileage: number;
  status: string;
  partnerName: string | null;
  documentUrl: string | null;
  ownerId: string;
}

export interface AdminVehicleListResponse {
  items: AdminVehicle[];
  totalCount: number;
  page: number;
  pageSize: number;
}

// ─── Admin Customers ──────────────────────────────────────────────────────────

export interface AdminCustomer {
  id: string;
  name: string;
  email: string;
  contacts: string | null;
  subscriptionStatus: string;
  subscriptionPlan: string;
  aiRequestsRemaining: number;
  authProvider: string;
  registrationDate: string;
  isBlocked: boolean;
  invalidChatRequestCount: number;
}

export interface AdminCustomerListResponse {
  items: AdminCustomer[];
  totalCount: number;
  page: number;
  pageSize: number;
}
