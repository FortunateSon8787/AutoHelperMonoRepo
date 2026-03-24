// ─── Vehicle Owner ────────────────────────────────────────────────────────────

export interface VehicleOwner {
  ownerId: string;
  name: string;
  contacts: string | null;
  avatarUrl: string | null;
}
