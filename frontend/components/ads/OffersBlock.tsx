"use client";

import { useEffect, useState } from "react";
import {
  adCampaignService,
  AdCampaignServiceError,
} from "@/services/adCampaignService";
import type { AdCampaign } from "@/types/adCampaign";
import { useTranslations } from "next-intl";

interface OffersBlockProps {
  isAuthenticated: boolean;
  isPartner: boolean;
  targetCategory?: string;
}

/**
 * Displays a themed block of partner offers (OfferBlock campaigns).
 * Partners never see ads (rule 13).
 * Anonymous users see offers only if ShowToAnonymous is true.
 */
export default function OffersBlock({
  isAuthenticated,
  isPartner,
  targetCategory,
}: OffersBlockProps) {
  const t = useTranslations("adCampaigns");
  const [offers, setOffers] = useState<AdCampaign[]>([]);

  useEffect(() => {
    // Partners never see offer blocks
    if (isPartner) return;

    adCampaignService
      .getActiveAds({ isAuthenticated, isPartner, targetCategory })
      .then((ads) => {
        setOffers(ads.filter((a) => a.type === "OfferBlock"));
      })
      .catch((err) => {
        if (!(err instanceof AdCampaignServiceError)) {
          console.error("Failed to load offers block", err);
        }
        // Silently fail — ads are non-critical
      });
  }, [isAuthenticated, isPartner, targetCategory]);

  if (offers.length === 0) return null;

  return (
    <section
      className="my-6"
      aria-label={t("title")}
    >
      <div className="flex items-center justify-between mb-3">
        <h2 className="text-base font-semibold text-gray-700">{t("title")}</h2>
        <span className="text-xs text-gray-400">реклама</span>
      </div>

      <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
        {offers.map((offer) => (
          <div
            key={offer.id}
            className="bg-white border border-gray-200 rounded-xl p-4 shadow-sm hover:shadow-md transition-shadow"
          >
            <div className="flex items-center gap-2 mb-2">
              <span className="inline-block px-2 py-0.5 bg-blue-50 text-blue-600 rounded text-xs font-medium">
                {t(`categories.${offer.targetCategory as "AutoService" | "CarWash" | "Towing" | "AutoShop" | "Other"}`)}
              </span>
            </div>
            <p className="text-sm text-gray-700 leading-relaxed">
              {offer.content}
            </p>
          </div>
        ))}
      </div>
    </section>
  );
}
