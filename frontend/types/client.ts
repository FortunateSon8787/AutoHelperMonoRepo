// ─── Profile ──────────────────────────────────────────────────────────────────

export interface ClientProfile {
  id: string;
  name: string;
  email: string;
  contacts: string | null;
  subscriptionStatus: string;
  subscriptionPlan: string;
  subscriptionStartDate: string | null;
  subscriptionEndDate: string | null;
  aiRequestsRemaining: number;
  authProvider: string;
  registrationDate: string;
}

// ─── Subscription ─────────────────────────────────────────────────────────────

export interface SubscriptionInfo {
  status: string;
  plan: string;
  startDate: string | null;
  endDate: string | null;
  aiRequestsRemaining: number;
  monthlyPriceUsd: number | null;
  monthlyRequestQuota: number | null;
}

export interface UpdateProfileRequest {
  name: string;
  contacts: string | null;
}
