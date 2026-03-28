export interface PartnerProfile {
  id: string;
  name: string;
  type: string;
  specialization: string;
  description: string;
  address: string;
  locationLat: number;
  locationLng: number;
  workingOpenFrom: string;
  workingOpenTo: string;
  workingDays: string;
  contactsPhone: string;
  contactsWebsite: string | null;
  contactsMessengerLinks: string | null;
  logoUrl: string | null;
  isVerified: boolean;
  isActive: boolean;
  showBannersToAnonymous: boolean;
  accountUserId: string;
}

export interface RegisterPartnerRequest {
  name: string;
  type: string;
  specialization: string;
  description: string;
  address: string;
  locationLat: number;
  locationLng: number;
  workingOpenFrom: string;
  workingOpenTo: string;
  workingDays: string;
  contactsPhone: string;
  contactsWebsite?: string | null;
  contactsMessengerLinks?: string | null;
}

export interface UpdatePartnerProfileRequest {
  name: string;
  specialization: string;
  description: string;
  address: string;
  locationLat: number;
  locationLng: number;
  workingOpenFrom: string;
  workingOpenTo: string;
  workingDays: string;
  contactsPhone: string;
  contactsWebsite?: string | null;
  contactsMessengerLinks?: string | null;
}

export const PARTNER_TYPES = [
  "AutoService",
  "CarWash",
  "Towing",
  "AutoShop",
  "Other",
] as const;

export type PartnerType = (typeof PARTNER_TYPES)[number];
