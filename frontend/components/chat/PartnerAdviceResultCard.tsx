"use client";

import {
  MapPin,
  Star,
  Phone,
  Globe,
  Clock,
  AlertTriangle,
  ShieldCheck,
  Navigation,
  MessageSquare,
} from "lucide-react";
import { useTranslations } from "next-intl";
import type { PartnerAdviceResult, PartnerAdviceEntry } from "@/types/chat";

// ─── Props ────────────────────────────────────────────────────────────────────

interface PartnerAdviceResultCardProps {
  result: PartnerAdviceResult;
}

// ─── Partner card ─────────────────────────────────────────────────────────────

function PartnerCard({
  partner,
  index,
  t,
}: {
  partner: PartnerAdviceEntry;
  index: number;
  t: ReturnType<typeof useTranslations>;
}) {
  return (
    <div className="bg-card border border-border rounded-xl p-4 space-y-3">
      {/* Header row */}
      <div className="flex items-start gap-3">
        {/* Index badge */}
        <div className="w-7 h-7 rounded-full bg-primary/10 text-primary text-xs font-semibold flex items-center justify-center flex-shrink-0 mt-0.5">
          {index}
        </div>

        <div className="flex-1 min-w-0">
          {/* Name + badges */}
          <div className="flex flex-wrap items-center gap-1.5 mb-1">
            <span className="text-sm font-semibold text-foreground">{partner.name}</span>
            {partner.is_priority && (
              <span className="inline-flex items-center gap-1 px-1.5 py-0.5 bg-success/10 text-success text-xs rounded-md font-medium">
                <ShieldCheck className="w-3 h-3" />
                {t("priorityBadge")}
              </span>
            )}
            {partner.has_warning && (
              <span className="inline-flex items-center gap-1 px-1.5 py-0.5 bg-destructive/10 text-destructive text-xs rounded-md font-medium">
                <AlertTriangle className="w-3 h-3" />
                {t("warningBadge")}
              </span>
            )}
          </div>

          {/* Meta row: distance + open/closed */}
          <div className="flex flex-wrap items-center gap-3 text-xs text-muted-foreground">
            {partner.distance_km > 0 && (
              <span className="flex items-center gap-1">
                <Navigation className="w-3 h-3" />
                {partner.distance_km.toFixed(1)} {t("km")}
              </span>
            )}
            {partner.is_open_now !== null && partner.is_open_now !== undefined && (
              <span
                className={`flex items-center gap-1 font-medium ${
                  partner.is_open_now ? "text-success" : "text-muted-foreground"
                }`}
              >
                <Clock className="w-3 h-3" />
                {partner.is_open_now ? t("openNow") : t("closed")}
              </span>
            )}
            {partner.rating !== null && partner.rating !== undefined && (
              <span className="flex items-center gap-1">
                <Star className="w-3 h-3 text-warning fill-warning" />
                <span className="font-medium text-foreground">{partner.rating.toFixed(1)}</span>
                {partner.reviews_count !== null && partner.reviews_count !== undefined && (
                  <span>({partner.reviews_count} {t("reviews")})</span>
                )}
              </span>
            )}
          </div>
        </div>
      </div>

      {/* Details */}
      <div className="space-y-1.5 pl-10">
        {partner.address && (
          <div className="flex items-start gap-2 text-xs">
            <MapPin className="w-3 h-3 mt-0.5 flex-shrink-0 text-muted-foreground" />
            <a
              href={`https://www.google.com/maps/search/?api=1&query=${encodeURIComponent(partner.address)}`}
              target="_blank"
              rel="noopener noreferrer"
              className="text-primary hover:underline"
            >
              {partner.address}
            </a>
          </div>
        )}
        {partner.services && (
          <div className="flex items-start gap-2 text-xs text-muted-foreground">
            <MessageSquare className="w-3 h-3 mt-0.5 flex-shrink-0" />
            <span>{partner.services}</span>
          </div>
        )}
        {partner.phone && (
          <div className="flex items-center gap-2 text-xs">
            <Phone className="w-3 h-3 text-muted-foreground flex-shrink-0" />
            <a
              href={`tel:${partner.phone}`}
              className="text-primary hover:underline"
            >
              {partner.phone}
            </a>
          </div>
        )}
        {partner.website && (() => {
          let safeUrl: string | null = null;
          try {
            const u = new URL(partner.website);
            if (u.protocol === "https:" || u.protocol === "http:") safeUrl = partner.website;
          } catch { /* invalid URL */ }
          return safeUrl ? (
            <div className="flex items-center gap-2 text-xs">
              <Globe className="w-3 h-3 text-muted-foreground flex-shrink-0" />
              <a
                href={safeUrl}
                target="_blank"
                rel="noopener noreferrer"
                className="text-primary hover:underline truncate"
              >
                {partner.website}
              </a>
            </div>
          ) : partner.website ? (
            <div className="flex items-center gap-2 text-xs">
              <Globe className="w-3 h-3 text-muted-foreground flex-shrink-0" />
              <span className="text-muted-foreground truncate">{partner.website}</span>
            </div>
          ) : null;
        })()}
      </div>
    </div>
  );
}

// ─── Component ────────────────────────────────────────────────────────────────

export function PartnerAdviceResultCard({ result }: PartnerAdviceResultCardProps) {
  const t = useTranslations("chat.partnerAdviceResult");

  const partners = result.partners ?? [];

  return (
    <div className="space-y-4">
      {/* Header badge */}
      <div className="flex items-center gap-2">
        <div className="flex items-center gap-2 px-2.5 py-1 bg-success/10 rounded-md">
          <MapPin className="w-3.5 h-3.5 text-success" />
          <span className="text-xs font-medium text-success">{t("title")}</span>
          {partners.length > 0 && (
            <span className="text-xs font-medium text-success">— {partners.length}</span>
          )}
        </div>
      </div>

      {/* Summary */}
      {result.summary && (
        <div className="bg-info/5 border border-info/20 rounded-xl px-4 py-3">
          <p className="text-sm text-foreground leading-relaxed">{result.summary}</p>
        </div>
      )}

      {/* Partners list or empty state */}
      {partners.length === 0 ? (
        <div className="bg-muted/50 border border-border rounded-xl px-4 py-5 text-center">
          <MapPin className="w-8 h-8 text-muted-foreground mx-auto mb-2 opacity-40" />
          <p className="text-sm text-muted-foreground">{t("noPartners")}</p>
        </div>
      ) : (
        <div className="space-y-3">
          {partners.map((partner, i) => (
            <PartnerCard
              key={i}
              partner={partner}
              index={i + 1}
              t={t}
            />
          ))}
        </div>
      )}
    </div>
  );
}
