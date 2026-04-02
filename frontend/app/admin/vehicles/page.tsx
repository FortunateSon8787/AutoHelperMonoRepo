"use client";

import { useEffect, useState, useCallback, useRef } from "react";
import { useTranslations } from "next-intl";
import { Loader2, Car, Search } from "lucide-react";
import axios from "axios";

import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { AppHeader } from "@/components/AppHeader";
import { adminService, AdminServiceError } from "@/services/adminService";
import type { AdminVehicleListResponse } from "@/types/admin";

// ─── Status badge ─────────────────────────────────────────────────────────────

const STATUS_COLORS: Record<string, string> = {
  Active: "text-success bg-success/10",
  ForSale: "text-blue-600 bg-blue-50",
  InRepair: "text-amber-600 bg-amber-50",
  Recycled: "text-muted-foreground bg-muted",
  Dismantled: "text-destructive bg-destructive/10",
};

function StatusBadge({ status }: { status: string }) {
  const t = useTranslations("admin.vehicles.status");
  const colorClass = STATUS_COLORS[status] ?? "text-muted-foreground bg-muted";
  return (
    <span className={`inline-flex text-xs px-2 py-0.5 rounded-full font-medium ${colorClass}`}>
      {t(status as Parameters<typeof t>[0])}
    </span>
  );
}

// ─── Page ─────────────────────────────────────────────────────────────────────

const PAGE_SIZE = 20;

export default function AdminVehiclesPage() {
  const t = useTranslations("admin.vehicles");

  const [data, setData] = useState<AdminVehicleListResponse | null>(null);
  const [loadError, setLoadError] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [search, setSearch] = useState("");
  const [debouncedSearch, setDebouncedSearch] = useState("");
  const [page, setPage] = useState(1);

  const abortRef = useRef<AbortController | null>(null);

  // ─── Debounce search ──────────────────────────────────────────────────────

  useEffect(() => {
    const timer = setTimeout(() => {
      setDebouncedSearch(search);
      setPage(1);
    }, 400);
    return () => clearTimeout(timer);
  }, [search]);

  // ─── Load data ────────────────────────────────────────────────────────────

  const load = useCallback(async () => {
    // Отменяем предыдущий запрос, если он ещё выполняется
    abortRef.current?.abort();
    const controller = new AbortController();
    abortRef.current = controller;

    setIsLoading(true);
    setLoadError(null);
    try {
      const result = await adminService.getVehicles(
        page,
        PAGE_SIZE,
        debouncedSearch || undefined,
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
  }, [page, debouncedSearch, t]);

  useEffect(() => {
    load();
    return () => abortRef.current?.abort();
  }, [load]);

  // ─── Pagination ───────────────────────────────────────────────────────────

  const totalPages = data ? Math.ceil(data.totalCount / PAGE_SIZE) : 1;

  // ─── Render ───────────────────────────────────────────────────────────────

  return (
    <div className="min-h-screen bg-background">
      <AppHeader />

      <div className="max-w-6xl mx-auto px-4 py-10">
        {/* Header */}
        <div className="flex items-center gap-3 mb-6">
          <div className="w-9 h-9 rounded-xl bg-primary/10 flex items-center justify-center">
            <Car className="h-4 w-4 text-primary" />
          </div>
          <h1 className="text-xl font-semibold text-foreground">{t("title")}</h1>
          {data && (
            <span className="ml-auto text-sm text-muted-foreground">
              {t("totalCount", { count: data.totalCount })}
            </span>
          )}
        </div>

        {/* Search */}
        <div className="relative mb-5">
          <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
          <Input
            className="pl-9"
            placeholder={t("searchPlaceholder")}
            value={search}
            onChange={(e) => setSearch(e.target.value)}
          />
        </div>

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
              <Car className="h-7 w-7 text-muted-foreground" />
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
                        {t("columns.vin")}
                      </th>
                      <th className="text-left px-4 py-3 text-muted-foreground font-medium">
                        {t("columns.vehicle")}
                      </th>
                      <th className="text-right px-4 py-3 text-muted-foreground font-medium">
                        {t("columns.year")}
                      </th>
                      <th className="text-left px-4 py-3 text-muted-foreground font-medium">
                        {t("columns.color")}
                      </th>
                      <th className="text-right px-4 py-3 text-muted-foreground font-medium">
                        {t("columns.mileage")}
                      </th>
                      <th className="text-center px-4 py-3 text-muted-foreground font-medium">
                        {t("columns.status")}
                      </th>
                      <th className="text-left px-4 py-3 text-muted-foreground font-medium">
                        {t("columns.details")}
                      </th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-border">
                    {data.items.map((vehicle) => (
                      <tr
                        key={vehicle.id}
                        className="hover:bg-muted/20 transition-colors"
                      >
                        <td className="px-4 py-3 font-mono text-xs text-foreground">
                          {vehicle.vin}
                        </td>
                        <td className="px-4 py-3 font-medium text-foreground">
                          {vehicle.brand} {vehicle.model}
                        </td>
                        <td className="px-4 py-3 text-right text-muted-foreground">
                          {vehicle.year}
                        </td>
                        <td className="px-4 py-3 text-muted-foreground">
                          {vehicle.color ?? "—"}
                        </td>
                        <td className="px-4 py-3 text-right text-muted-foreground">
                          {vehicle.mileage.toLocaleString()} {t("kmUnit")}
                        </td>
                        <td className="px-4 py-3 text-center">
                          <StatusBadge status={vehicle.status} />
                        </td>
                        <td className="px-4 py-3 text-muted-foreground text-xs">
                          {vehicle.status === "InRepair" && vehicle.partnerName && (
                            <span>{t("partnerLabel")}: {vehicle.partnerName}</span>
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
    </div>
  );
}
