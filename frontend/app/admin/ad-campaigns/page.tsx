"use client";

import { useEffect, useState, useCallback, useRef } from "react";
import { useTranslations } from "next-intl";
import { Loader2, Megaphone, Search, BarChart2 } from "lucide-react";
import axios from "axios";
import Link from "next/link";

import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { AppHeader } from "@/components/AppHeader";
import { adminService, AdminServiceError } from "@/services/adminService";
import { cn } from "@/lib/utils";
import type { AdminAdCampaign, AdminAdCampaignListResponse } from "@/types/admin";

// ─── Confirmation Modal ───────────────────────────────────────────────────────

function ConfirmModal({
  title,
  message,
  onConfirm,
  onCancel,
  isSubmitting,
}: {
  title: string;
  message: string;
  onConfirm: () => void;
  onCancel: () => void;
  isSubmitting: boolean;
}) {
  const t = useTranslations("admin.adCampaigns.modal");

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40">
      <div className="bg-card border border-border rounded-2xl p-6 shadow-card max-w-sm w-full mx-4">
        <h2 className="text-base font-semibold text-foreground mb-2">{title}</h2>
        <p className="text-sm text-muted-foreground mb-6">{message}</p>
        <div className="flex gap-3 justify-end">
          <Button variant="outline" size="sm" onClick={onCancel} disabled={isSubmitting}>
            {t("cancel")}
          </Button>
          <Button size="sm" onClick={onConfirm} disabled={isSubmitting}>
            {isSubmitting ? <Loader2 className="h-4 w-4 animate-spin" /> : t("confirm")}
          </Button>
        </div>
      </div>
    </div>
  );
}

// ─── Page ─────────────────────────────────────────────────────────────────────

const PAGE_SIZE = 20;

export default function AdminAdCampaignsPage() {
  const t = useTranslations("admin.adCampaigns");

  const [data, setData] = useState<AdminAdCampaignListResponse | null>(null);
  const [loadError, setLoadError] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [partnerIdFilter, setPartnerIdFilter] = useState("");
  const [debouncedFilter, setDebouncedFilter] = useState("");
  const [page, setPage] = useState(1);

  const [actionError, setActionError] = useState<string | null>(null);
  const [modal, setModal] = useState<{
    campaign: AdminAdCampaign;
    type: "activate" | "deactivate";
  } | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  const abortRef = useRef<AbortController | null>(null);

  // ─── Debounce filter ──────────────────────────────────────────────────────

  useEffect(() => {
    const timer = setTimeout(() => {
      setDebouncedFilter(partnerIdFilter);
      setPage(1);
    }, 400);
    return () => clearTimeout(timer);
  }, [partnerIdFilter]);

  // ─── Load data ────────────────────────────────────────────────────────────

  const load = useCallback(async () => {
    // Отменяем предыдущий запрос, если он ещё выполняется
    abortRef.current?.abort();
    const controller = new AbortController();
    abortRef.current = controller;

    setIsLoading(true);
    setLoadError(null);
    try {
      const result = await adminService.getAdCampaigns(
        page,
        PAGE_SIZE,
        debouncedFilter || undefined,
        controller.signal
      );
      setData(result);
    } catch (err) {
      if (axios.isCancel(err)) return;
      setLoadError(
        err instanceof AdminServiceError
          ? t(`errors.${err.code}` as Parameters<typeof t>[0])
          : t("errors.unknown")
      );
    } finally {
      setIsLoading(false);
    }
  }, [page, debouncedFilter, t]);

  useEffect(() => {
    load();
    return () => abortRef.current?.abort();
  }, [load]);

  // ─── Activate / Deactivate ────────────────────────────────────────────────

  const handleConfirm = async () => {
    if (!modal) return;
    setIsSubmitting(true);
    setActionError(null);
    try {
      if (modal.type === "activate") {
        await adminService.activateAdCampaign(modal.campaign.id);
      } else {
        await adminService.deactivateAdCampaign(modal.campaign.id);
      }
      setModal(null);
      await load();
    } catch (err) {
      const key =
        modal.type === "activate" ? "errors.activateFailed" : "errors.deactivateFailed";
      setActionError(
        err instanceof AdminServiceError
          ? t(key as Parameters<typeof t>[0])
          : t("errors.unknown")
      );
    } finally {
      setIsSubmitting(false);
    }
  };

  // ─── Pagination ───────────────────────────────────────────────────────────

  const totalPages = data ? Math.ceil(data.totalCount / PAGE_SIZE) : 1;

  // ─── Render ───────────────────────────────────────────────────────────────

  return (
    <div className="min-h-screen bg-background">
      <AppHeader />

      <div className="max-w-7xl mx-auto px-4 py-10">
        {/* Header */}
        <div className="flex items-center gap-3 mb-6">
          <div className="w-9 h-9 rounded-xl bg-primary/10 flex items-center justify-center">
            <Megaphone className="h-4 w-4 text-primary" />
          </div>
          <h1 className="text-xl font-semibold text-foreground">{t("title")}</h1>
          {data && (
            <span className="ml-auto text-sm text-muted-foreground">
              {t("totalCount", { count: data.totalCount })}
            </span>
          )}
          <Link href="/admin/ad-campaigns/stats">
            <Button variant="outline" size="sm" className="gap-1.5">
              <BarChart2 className="h-4 w-4" />
              {t("statsLink")}
            </Button>
          </Link>
        </div>

        {/* Filter */}
        <div className="relative mb-5">
          <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
          <Input
            className="pl-9"
            placeholder={t("searchPlaceholder")}
            value={partnerIdFilter}
            onChange={(e) => setPartnerIdFilter(e.target.value)}
          />
        </div>

        {/* Action error */}
        {actionError && (
          <div className="bg-destructive/5 border border-destructive/20 text-destructive rounded-xl px-4 py-3 text-sm mb-4">
            {actionError}
          </div>
        )}

        {/* Content */}
        {isLoading ? (
          <div className="flex justify-center py-20">
            <Loader2 className="h-6 w-6 animate-spin text-muted-foreground" />
          </div>
        ) : loadError ? (
          <div className="bg-destructive/5 border border-destructive/20 text-destructive rounded-xl px-6 py-4 text-sm">
            {loadError}
          </div>
        ) : !data || data.items.length === 0 ? (
          <div className="bg-card border border-border rounded-2xl p-10 text-center shadow-card">
            <div className="w-14 h-14 rounded-2xl bg-muted flex items-center justify-center mx-auto mb-3">
              <Megaphone className="h-7 w-7 text-muted-foreground" />
            </div>
            <p className="text-sm text-muted-foreground">{t("emptyState")}</p>
          </div>
        ) : (
          <>
            {/* Table */}
            <div className="bg-card border border-border rounded-2xl shadow-card overflow-hidden">
              <div className="overflow-x-auto">
                <table className="w-full text-sm">
                  <thead>
                    <tr className="border-b border-border bg-muted/30">
                      <th className="text-left px-4 py-3 text-muted-foreground font-medium">
                        {t("columns.type")}
                      </th>
                      <th className="text-left px-4 py-3 text-muted-foreground font-medium">
                        {t("columns.category")}
                      </th>
                      <th className="text-left px-4 py-3 text-muted-foreground font-medium max-w-xs">
                        {t("columns.content")}
                      </th>
                      <th className="text-left px-4 py-3 text-muted-foreground font-medium">
                        {t("columns.partner")}
                      </th>
                      <th className="text-left px-4 py-3 text-muted-foreground font-medium">
                        {t("columns.schedule")}
                      </th>
                      <th className="text-right px-4 py-3 text-muted-foreground font-medium">
                        {t("columns.stats")}
                      </th>
                      <th className="text-center px-4 py-3 text-muted-foreground font-medium">
                        {t("columns.status")}
                      </th>
                      <th className="px-4 py-3" />
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-border">
                    {data.items.map((campaign) => (
                      <tr
                        key={campaign.id}
                        className="hover:bg-muted/20 transition-colors"
                      >
                        <td className="px-4 py-3 text-foreground font-medium">
                          {t(`types.${campaign.type as "OfferBlock" | "Banner"}`)}
                        </td>
                        <td className="px-4 py-3 text-muted-foreground">
                          {t(`categories.${campaign.targetCategory as "AutoService" | "CarWash" | "Towing" | "AutoShop" | "Other"}`)}
                        </td>
                        <td className="px-4 py-3 text-muted-foreground max-w-xs">
                          <span className="line-clamp-2 text-xs">{campaign.content}</span>
                        </td>
                        <td className="px-4 py-3 text-muted-foreground font-mono text-xs">
                          {campaign.partnerId.slice(0, 8)}…
                        </td>
                        <td className="px-4 py-3 text-muted-foreground text-xs whitespace-nowrap">
                          <div>{new Date(campaign.startsAt).toLocaleDateString()}</div>
                          <div>{new Date(campaign.endsAt).toLocaleDateString()}</div>
                        </td>
                        <td className="px-4 py-3 text-right text-muted-foreground text-xs whitespace-nowrap">
                          <div>{campaign.statsImpressions} imp</div>
                          <div>{campaign.statsClicks} clk</div>
                        </td>
                        <td className="px-4 py-3 text-center">
                          {campaign.isActive ? (
                            <span className="inline-flex items-center gap-1 text-xs text-success bg-success/10 px-2 py-0.5 rounded-full">
                              {t("status.active")}
                            </span>
                          ) : (
                            <span className={cn(
                              "inline-flex items-center gap-1 text-xs px-2 py-0.5 rounded-full",
                              "text-muted-foreground bg-secondary"
                            )}>
                              {t("status.inactive")}
                            </span>
                          )}
                        </td>
                        <td className="px-4 py-3 text-right">
                          {campaign.isActive ? (
                            <Button
                              size="sm"
                              variant="outline"
                              className="text-destructive hover:text-destructive border-destructive/30 hover:border-destructive/60 hover:bg-destructive/5"
                              onClick={() => setModal({ campaign, type: "deactivate" })}
                            >
                              {t("actions.deactivate")}
                            </Button>
                          ) : (
                            <Button
                              size="sm"
                              variant="outline"
                              onClick={() => setModal({ campaign, type: "activate" })}
                            >
                              {t("actions.activate")}
                            </Button>
                          )}
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </div>

            {/* Pagination */}
            {totalPages > 1 && (
              <div className="flex items-center justify-center gap-2 mt-5">
                <Button
                  size="sm"
                  variant="outline"
                  onClick={() => setPage((p) => Math.max(1, p - 1))}
                  disabled={page === 1}
                >
                  ‹
                </Button>
                <span className="text-sm text-muted-foreground">
                  {page} / {totalPages}
                </span>
                <Button
                  size="sm"
                  variant="outline"
                  onClick={() => setPage((p) => Math.min(totalPages, p + 1))}
                  disabled={page === totalPages}
                >
                  ›
                </Button>
              </div>
            )}
          </>
        )}
      </div>

      {/* Confirmation modal */}
      {modal && (
        <ConfirmModal
          title={
            modal.type === "activate"
              ? t("modal.activateTitle")
              : t("modal.deactivateTitle")
          }
          message={
            modal.type === "activate"
              ? t("modal.activateConfirm")
              : t("modal.deactivateConfirm")
          }
          onConfirm={handleConfirm}
          onCancel={() => {
            setModal(null);
            setActionError(null);
          }}
          isSubmitting={isSubmitting}
        />
      )}
    </div>
  );
}
