"use client";

import { useEffect, useState } from "react";
import { useTranslations } from "next-intl";
import { Loader2, BarChart2, ArrowLeft, TrendingUp, Eye, MousePointerClick, Megaphone } from "lucide-react";
import Link from "next/link";

import { AppHeader } from "@/components/AppHeader";
import { adminService, AdminServiceError } from "@/services/adminService";
import type { AdminAdCampaign } from "@/types/admin";

// ─── Stat Card ────────────────────────────────────────────────────────────────

function StatCard({
  icon: Icon,
  label,
  value,
}: {
  icon: React.ElementType;
  label: string;
  value: string | number;
}) {
  return (
    <div className="bg-card border border-border rounded-2xl p-5 shadow-card">
      <div className="flex items-center gap-3 mb-3">
        <div className="w-8 h-8 rounded-lg bg-primary/10 flex items-center justify-center">
          <Icon className="h-4 w-4 text-primary" />
        </div>
        <span className="text-sm text-muted-foreground">{label}</span>
      </div>
      <p className="text-2xl font-semibold text-foreground">{value}</p>
    </div>
  );
}

// ─── Per-partner aggregated row ───────────────────────────────────────────────

interface PartnerStats {
  partnerId: string;
  campaigns: number;
  impressions: number;
  clicks: number;
  ctr: number;
}

function aggregateByPartner(campaigns: AdminAdCampaign[]): PartnerStats[] {
  const map = new Map<string, PartnerStats>();

  for (const c of campaigns) {
    const existing = map.get(c.partnerId);
    if (existing) {
      existing.campaigns += 1;
      existing.impressions += c.statsImpressions;
      existing.clicks += c.statsClicks;
    } else {
      map.set(c.partnerId, {
        partnerId: c.partnerId,
        campaigns: 1,
        impressions: c.statsImpressions,
        clicks: c.statsClicks,
        ctr: 0,
      });
    }
  }

  return Array.from(map.values()).map((s) => ({
    ...s,
    ctr: s.impressions > 0 ? (s.clicks / s.impressions) * 100 : 0,
  }));
}

// ─── Page ─────────────────────────────────────────────────────────────────────

// Load all pages to compute stats across the full dataset
const LOAD_PAGE_SIZE = 100;

export default function AdminAdCampaignsStatsPage() {
  const t = useTranslations("admin.adCampaigns.stats");

  const [campaigns, setCampaigns] = useState<AdminAdCampaign[]>([]);
  const [loadError, setLoadError] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    async function loadAll() {
      setIsLoading(true);
      setLoadError(null);
      try {
        // Load first page to know total, then load remaining pages
        const first = await adminService.getAdCampaigns(1, LOAD_PAGE_SIZE);
        const totalPages = Math.ceil(first.totalCount / LOAD_PAGE_SIZE);

        if (totalPages <= 1) {
          setCampaigns(first.items);
        } else {
          const remaining = await Promise.all(
            Array.from({ length: totalPages - 1 }, (_, i) =>
              adminService.getAdCampaigns(i + 2, LOAD_PAGE_SIZE)
            )
          );
          setCampaigns([
            ...first.items,
            ...remaining.flatMap((r) => r.items),
          ]);
        }
      } catch (err) {
        setLoadError(
          err instanceof AdminServiceError ? err.code : "unknown"
        );
      } finally {
        setIsLoading(false);
      }
    }

    loadAll();
  }, []);

  // ─── Derived stats ────────────────────────────────────────────────────────

  const totalCampaigns = campaigns.length;
  const activeCampaigns = campaigns.filter((c) => c.isActive).length;
  const totalImpressions = campaigns.reduce((s, c) => s + c.statsImpressions, 0);
  const totalClicks = campaigns.reduce((s, c) => s + c.statsClicks, 0);
  const overallCtr =
    totalImpressions > 0 ? ((totalClicks / totalImpressions) * 100).toFixed(2) : "0.00";

  const partnerStats = aggregateByPartner(campaigns);
  const topByImpressions = [...partnerStats]
    .sort((a, b) => b.impressions - a.impressions)
    .slice(0, 10);
  const topByClicks = [...partnerStats]
    .sort((a, b) => b.clicks - a.clicks)
    .slice(0, 10);

  // ─── Render ───────────────────────────────────────────────────────────────

  return (
    <div className="min-h-screen bg-background">
      <AppHeader />

      <div className="max-w-6xl mx-auto px-4 py-10">
        {/* Header */}
        <div className="flex items-center gap-3 mb-6">
          <div className="w-9 h-9 rounded-xl bg-primary/10 flex items-center justify-center">
            <BarChart2 className="h-4 w-4 text-primary" />
          </div>
          <h1 className="text-xl font-semibold text-foreground">{t("title")}</h1>
          <Link
            href="/admin/ad-campaigns"
            className="ml-auto inline-flex items-center gap-1.5 text-sm text-muted-foreground hover:text-foreground transition-colors"
          >
            <ArrowLeft className="h-3.5 w-3.5" />
            {t("backToList")}
          </Link>
        </div>

        {isLoading ? (
          <div className="flex justify-center py-20">
            <Loader2 className="h-6 w-6 animate-spin text-muted-foreground" />
          </div>
        ) : loadError ? (
          <div className="bg-destructive/5 border border-destructive/20 text-destructive rounded-xl px-6 py-4 text-sm">
            {loadError}
          </div>
        ) : campaigns.length === 0 ? (
          <div className="bg-card border border-border rounded-2xl p-10 text-center shadow-card">
            <div className="w-14 h-14 rounded-2xl bg-muted flex items-center justify-center mx-auto mb-3">
              <Megaphone className="h-7 w-7 text-muted-foreground" />
            </div>
            <p className="text-sm text-muted-foreground">{t("noData")}</p>
          </div>
        ) : (
          <>
            {/* Summary cards */}
            <div className="grid grid-cols-2 md:grid-cols-5 gap-4 mb-8">
              <StatCard icon={Megaphone} label={t("totalCampaigns")} value={totalCampaigns} />
              <StatCard icon={TrendingUp} label={t("activeCampaigns")} value={activeCampaigns} />
              <StatCard icon={Eye} label={t("totalImpressions")} value={totalImpressions.toLocaleString()} />
              <StatCard icon={MousePointerClick} label={t("totalClicks")} value={totalClicks.toLocaleString()} />
              <StatCard icon={BarChart2} label={t("overallCtr")} value={`${overallCtr}%`} />
            </div>

            {/* Top tables */}
            <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
              {/* Top by impressions */}
              <div className="bg-card border border-border rounded-2xl shadow-card overflow-hidden">
                <div className="px-5 py-4 border-b border-border">
                  <h2 className="text-sm font-semibold text-foreground">{t("topByImpressions")}</h2>
                </div>
                <div className="overflow-x-auto">
                  <table className="w-full text-sm">
                    <thead>
                      <tr className="border-b border-border bg-muted/30">
                        <th className="text-left px-4 py-2.5 text-muted-foreground font-medium">
                          {t("columns.partnerId")}
                        </th>
                        <th className="text-right px-4 py-2.5 text-muted-foreground font-medium">
                          {t("columns.campaigns")}
                        </th>
                        <th className="text-right px-4 py-2.5 text-muted-foreground font-medium">
                          {t("columns.impressions")}
                        </th>
                        <th className="text-right px-4 py-2.5 text-muted-foreground font-medium">
                          {t("columns.ctr")}
                        </th>
                      </tr>
                    </thead>
                    <tbody className="divide-y divide-border">
                      {topByImpressions.map((s) => (
                        <tr key={s.partnerId} className="hover:bg-muted/20 transition-colors">
                          <td className="px-4 py-2.5 font-mono text-xs text-muted-foreground">
                            {s.partnerId.slice(0, 8)}…
                          </td>
                          <td className="px-4 py-2.5 text-right text-muted-foreground">{s.campaigns}</td>
                          <td className="px-4 py-2.5 text-right text-foreground font-medium">
                            {s.impressions.toLocaleString()}
                          </td>
                          <td className="px-4 py-2.5 text-right text-muted-foreground">
                            {s.ctr.toFixed(2)}%
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              </div>

              {/* Top by clicks */}
              <div className="bg-card border border-border rounded-2xl shadow-card overflow-hidden">
                <div className="px-5 py-4 border-b border-border">
                  <h2 className="text-sm font-semibold text-foreground">{t("topByClicks")}</h2>
                </div>
                <div className="overflow-x-auto">
                  <table className="w-full text-sm">
                    <thead>
                      <tr className="border-b border-border bg-muted/30">
                        <th className="text-left px-4 py-2.5 text-muted-foreground font-medium">
                          {t("columns.partnerId")}
                        </th>
                        <th className="text-right px-4 py-2.5 text-muted-foreground font-medium">
                          {t("columns.campaigns")}
                        </th>
                        <th className="text-right px-4 py-2.5 text-muted-foreground font-medium">
                          {t("columns.clicks")}
                        </th>
                        <th className="text-right px-4 py-2.5 text-muted-foreground font-medium">
                          {t("columns.ctr")}
                        </th>
                      </tr>
                    </thead>
                    <tbody className="divide-y divide-border">
                      {topByClicks.map((s) => (
                        <tr key={s.partnerId} className="hover:bg-muted/20 transition-colors">
                          <td className="px-4 py-2.5 font-mono text-xs text-muted-foreground">
                            {s.partnerId.slice(0, 8)}…
                          </td>
                          <td className="px-4 py-2.5 text-right text-muted-foreground">{s.campaigns}</td>
                          <td className="px-4 py-2.5 text-right text-foreground font-medium">
                            {s.clicks.toLocaleString()}
                          </td>
                          <td className="px-4 py-2.5 text-right text-muted-foreground">
                            {s.ctr.toFixed(2)}%
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              </div>
            </div>
          </>
        )}
      </div>
    </div>
  );
}
