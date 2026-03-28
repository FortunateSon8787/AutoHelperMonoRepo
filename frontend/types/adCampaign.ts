export interface AdCampaign {
  id: string;
  partnerId: string;
  type: string;
  targetCategory: string;
  content: string;
  startsAt: string;
  endsAt: string;
  isActive: boolean;
  showToAnonymous: boolean;
  statsImpressions: number;
  statsClicks: number;
}

export interface CreateAdCampaignRequest {
  type: string;
  targetCategory: string;
  content: string;
  startsAt: string;
  endsAt: string;
  showToAnonymous: boolean;
}

export interface UpdateAdCampaignRequest {
  type: string;
  targetCategory: string;
  content: string;
  startsAt: string;
  endsAt: string;
  showToAnonymous: boolean;
}

export const AD_TYPES = ["OfferBlock", "Banner"] as const;
export type AdType = (typeof AD_TYPES)[number];

export const TARGET_CATEGORIES = [
  "AutoService",
  "CarWash",
  "Towing",
  "AutoShop",
  "Other",
] as const;
export type TargetCategory = (typeof TARGET_CATEGORIES)[number];
