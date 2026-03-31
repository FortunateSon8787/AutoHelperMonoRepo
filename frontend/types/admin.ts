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
