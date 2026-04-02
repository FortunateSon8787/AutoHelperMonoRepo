"use client";

import { useEffect, useState, useCallback, useRef } from "react";
import { useTranslations } from "next-intl";
import { Loader2, Users, Search, ShieldBan, ShieldCheck } from "lucide-react";
import axios from "axios";

import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { AppHeader } from "@/components/AppHeader";
import {
  adminService,
  AdminServiceError,
} from "@/services/adminService";
import type { AdminCustomer, AdminCustomerListResponse } from "@/types/admin";

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
  const t = useTranslations("admin.customers.modal");

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

export default function AdminCustomersPage() {
  const t = useTranslations("admin.customers");

  const [data, setData] = useState<AdminCustomerListResponse | null>(null);
  const [loadError, setLoadError] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [search, setSearch] = useState("");
  const [debouncedSearch, setDebouncedSearch] = useState("");
  const [page, setPage] = useState(1);

  const [actionError, setActionError] = useState<string | null>(null);
  const [modal, setModal] = useState<{
    customer: AdminCustomer;
    type: "block" | "unblock";
  } | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

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
      const result = await adminService.getCustomers(
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

  // ─── Block/Unblock ────────────────────────────────────────────────────────

  const handleConfirm = async () => {
    if (!modal) return;
    setIsSubmitting(true);
    setActionError(null);
    try {
      if (modal.type === "block") {
        await adminService.blockCustomer(modal.customer.id);
      } else {
        await adminService.unblockCustomer(modal.customer.id);
      }
      setModal(null);
      await load();
    } catch (err) {
      const key =
        modal.type === "block" ? "errors.blockFailed" : "errors.unblockFailed";
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

      <div className="max-w-6xl mx-auto px-4 py-10">
        {/* Header */}
        <div className="flex items-center gap-3 mb-6">
          <div className="w-9 h-9 rounded-xl bg-primary/10 flex items-center justify-center">
            <Users className="h-4 w-4 text-primary" />
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
              <Users className="h-7 w-7 text-muted-foreground" />
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
                        {t("columns.name")}
                      </th>
                      <th className="text-left px-4 py-3 text-muted-foreground font-medium">
                        {t("columns.email")}
                      </th>
                      <th className="text-left px-4 py-3 text-muted-foreground font-medium">
                        {t("columns.subscription")}
                      </th>
                      <th className="text-right px-4 py-3 text-muted-foreground font-medium">
                        {t("columns.aiRequests")}
                      </th>
                      <th className="text-right px-4 py-3 text-muted-foreground font-medium">
                        {t("columns.invalidRequests")}
                      </th>
                      <th className="text-left px-4 py-3 text-muted-foreground font-medium">
                        {t("columns.registered")}
                      </th>
                      <th className="text-center px-4 py-3 text-muted-foreground font-medium">
                        {t("columns.status")}
                      </th>
                      <th className="px-4 py-3" />
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-border">
                    {data.items.map((customer) => (
                      <tr
                        key={customer.id}
                        className={
                          customer.isBlocked
                            ? "bg-destructive/3 opacity-70"
                            : "hover:bg-muted/20 transition-colors"
                        }
                      >
                        <td className="px-4 py-3 font-medium text-foreground">
                          {customer.name}
                        </td>
                        <td className="px-4 py-3 text-muted-foreground">
                          {customer.email}
                        </td>
                        <td className="px-4 py-3 text-muted-foreground">
                          {customer.subscriptionPlan === "None"
                            ? customer.subscriptionStatus
                            : `${customer.subscriptionPlan} / ${customer.subscriptionStatus}`}
                        </td>
                        <td className="px-4 py-3 text-right text-muted-foreground">
                          {customer.aiRequestsRemaining}
                        </td>
                        <td className="px-4 py-3 text-right">
                          <span
                            className={
                              customer.invalidChatRequestCount > 0
                                ? "text-destructive font-medium"
                                : "text-muted-foreground"
                            }
                          >
                            {customer.invalidChatRequestCount}
                          </span>
                        </td>
                        <td className="px-4 py-3 text-muted-foreground">
                          {new Date(customer.registrationDate).toLocaleDateString()}
                        </td>
                        <td className="px-4 py-3 text-center">
                          {customer.isBlocked ? (
                            <span className="inline-flex items-center gap-1 text-xs text-destructive bg-destructive/10 px-2 py-0.5 rounded-full">
                              <ShieldBan className="h-3 w-3" />
                              {t("status.blocked")}
                            </span>
                          ) : (
                            <span className="inline-flex items-center gap-1 text-xs text-success bg-success/10 px-2 py-0.5 rounded-full">
                              <ShieldCheck className="h-3 w-3" />
                              {t("status.active")}
                            </span>
                          )}
                        </td>
                        <td className="px-4 py-3 text-right">
                          {customer.isBlocked ? (
                            <Button
                              size="sm"
                              variant="outline"
                              onClick={() =>
                                setModal({ customer, type: "unblock" })
                              }
                            >
                              {t("actions.unblock")}
                            </Button>
                          ) : (
                            <Button
                              size="sm"
                              variant="outline"
                              className="text-destructive hover:text-destructive border-destructive/30 hover:border-destructive/60 hover:bg-destructive/5"
                              onClick={() =>
                                setModal({ customer, type: "block" })
                              }
                            >
                              {t("actions.block")}
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
            modal.type === "block"
              ? t("modal.blockTitle")
              : t("modal.unblockTitle")
          }
          message={
            modal.type === "block"
              ? t("modal.blockConfirm", { name: modal.customer.name })
              : t("modal.unblockConfirm", { name: modal.customer.name })
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
