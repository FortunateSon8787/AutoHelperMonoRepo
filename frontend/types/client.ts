// ─── Profile ──────────────────────────────────────────────────────────────────

export interface ClientProfile {
  id: string;
  name: string;
  email: string;
  contacts: string | null;
  subscriptionStatus: string;
  authProvider: string;
  registrationDate: string;
}

export interface UpdateProfileRequest {
  name: string;
  contacts: string | null;
}
