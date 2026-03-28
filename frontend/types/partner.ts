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

export interface PartnerNearbyResult {
  id: string;
  name: string;
  type: string;
  specialization: string;
  description: string;
  address: string;
  locationLat: number;
  locationLng: number;
  distanceKm: number;
  workingOpenFrom: string;
  workingOpenTo: string;
  workingDays: string;
  isOpenNow: boolean;
  contactsPhone: string;
  contactsWebsite: string | null;
  logoUrl: string | null;
  isVerified: boolean;
  averageRating: number;
  reviewsCount: number;
}

export interface SearchPartnersParams {
  lat: number;
  lng: number;
  radiusKm?: number;
  type?: string;
  isOpenNow?: boolean;
}
