"use client";

import { useEffect, useState } from "react";
import {
  adCampaignService,
  AdCampaignServiceError,
} from "@/services/adCampaignService";
import type { AdCampaign } from "@/types/adCampaign";

interface AdBannerProps {
  isAuthenticated: boolean;
  isPartner: boolean;
  targetCategory?: string;
}

/**
 * Displays a single rotating ad banner.
 * Banners are never shown to partners (rule 13).
 * Anonymous users see banners only if ShowToAnonymous is true.
 */
export default function AdBanner({
  isAuthenticated,
  isPartner,
  targetCategory,
}: AdBannerProps) {
  const [banner, setBanner] = useState<AdCampaign | null>(null);

  useEffect(() => {
    // Partners never see ad banners
    if (isPartner) return;

    adCampaignService
      .getActiveAds({ isAuthenticated, isPartner, targetCategory })
      .then((ads) => {
        const banners = ads.filter((a) => a.type === "Banner");
        if (banners.length > 0) {
          // Pick random banner from the already-rotated list (server randomizes order)
          setBanner(banners[0]);
        }
      })
      .catch((err) => {
        if (!(err instanceof AdCampaignServiceError)) {
          console.error("Failed to load ad banner", err);
        }
        // Silently fail — ads are non-critical
      });
  }, [isAuthenticated, isPartner, targetCategory]);

  if (!banner) return null;

  return (
    <div
      className="w-full bg-gradient-to-r from-blue-50 to-indigo-50 border border-blue-100 rounded-xl p-4 my-4"
      aria-label="Реклама"
    >
      <div className="flex items-start justify-between gap-2">
        <p className="text-sm text-gray-700 leading-relaxed flex-1">
          {banner.content}
        </p>
        <span className="text-xs text-gray-400 flex-shrink-0 mt-0.5">
          реклама
        </span>
      </div>
    </div>
  );
}
