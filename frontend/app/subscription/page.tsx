"use client";

import { useEffect, useState } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { CheckCircle2, CreditCard, Loader2, Zap } from "lucide-react";
import { useTranslations } from "next-intl";

import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { AppHeader } from "@/components/AppHeader";
import {
  subscriptionService,
  SubscriptionServiceError,
} from "@/services/subscriptionService";
import type { SubscriptionInfo } from "@/types/client";

// ─── Plan definitions ─────────────────────────────────────────────────────────

interface PlanDef {
  key: string;
  priceUsd: number;
  quotaPerMonth: number;
}

const PLANS: PlanDef[] = [
  { key: "Normal", priceUsd: 4.99, quotaPerMonth: 30 },
  { key: "Pro", priceUsd: 7.99, quotaPerMonth: 100 },
  { key: "Max", priceUsd: 12.99, quotaPerMonth: 300 },
];

// ─── Page ─────────────────────────────────────────────────────────────────────

export default function SubscriptionPage() {
  const t = useTranslations("subscription");
  const tErrors = useTranslations("subscription.errors");
  const tValidation = useTranslations("subscription.validation");

  const [subscription, setSubscription] = useState<SubscriptionInfo | null>(null);
  const [loadError, setLoadError] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [activatingPlan, setActivatingPlan] = useState<string | null>(null);
  const [actionError, setActionError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);

  const topUpSchema = z.object({
    count: z
      .number({ invalid_type_error: tValidation("countRequired") })
      .min(1, tValidation("countMin"))
      .max(100, tValidation("countMax")),
  });

  type TopUpFormValues = z.infer<typeof topUpSchema>;

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors, isSubmitting },
  } = useForm<TopUpFormValues>({
    resolver: zodResolver(topUpSchema),
  });

  useEffect(() => {
    subscriptionService
      .getMySubscription()
      .then(setSubscription)
      .catch((err: unknown) => {
        if (err instanceof SubscriptionServiceError) {
          setLoadError(tErrors(err.code));
        } else {
          setLoadError(tErrors("unknown"));
        }
      })
      .finally(() => setIsLoading(false));
  }, [tErrors]);

  const handleActivate = async (planKey: string) => {
    setActivatingPlan(planKey);
    setActionError(null);
    setSuccessMessage(null);
    try {
      await subscriptionService.activatePlan(planKey);
      const updated = await subscriptionService.getMySubscription();
      setSubscription(updated);
      setSuccessMessage(t("activateSuccess"));
    } catch (err: unknown) {
      if (err instanceof SubscriptionServiceError) {
        setActionError(tErrors(err.code));
      } else {
        setActionError(tErrors("unknown"));
      }
    } finally {
      setActivatingPlan(null);
    }
  };

  const onTopUp = async (values: TopUpFormValues) => {
    setActionError(null);
    setSuccessMessage(null);
    try {
      await subscriptionService.topUpRequests(values.count);
      const updated = await subscriptionService.getMySubscription();
      setSubscription(updated);
      setSuccessMessage(t("topUpSuccess"));
      reset();
    } catch (err: unknown) {
      if (err instanceof SubscriptionServiceError) {
        setActionError(tErrors(err.code));
      } else {
        setActionError(tErrors("unknown"));
      }
    }
  };

  if (isLoading) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-background">
        <Loader2 className="h-6 w-6 animate-spin text-muted-foreground" />
      </div>
    );
  }

  if (loadError) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-background px-4">
        <div className="bg-destructive/5 border border-destructive/20 text-destructive rounded-xl px-6 py-4 text-sm">
          {loadError}
        </div>
      </div>
    );
  }

  const currentPlanKey = subscription?.plan ?? "None";
  const isActive = subscription?.status === "Premium";

  return (
    <div className="min-h-screen bg-background">
      <AppHeader />

      <div className="max-w-2xl mx-auto px-4 py-10 space-y-8">

        {/* ── Current subscription status ──────────────────────────────────── */}
        <div className="bg-card border border-border rounded-2xl p-8 shadow-card">
          <div className="flex items-center gap-3 mb-6">
            <div className="w-11 h-11 rounded-xl bg-primary/10 flex items-center justify-center">
              <CreditCard className="h-5 w-5 text-primary" />
            </div>
            <div>
              <h1 className="text-xl font-semibold text-foreground">{t("title")}</h1>
              <p className="text-sm text-muted-foreground">{t("subtitle")}</p>
            </div>
          </div>

          <div className="grid grid-cols-2 gap-4 sm:grid-cols-3">
            {/* Plan */}
            <div className="bg-background rounded-xl px-4 py-3">
              <p className="text-xs text-muted-foreground mb-1">{t("currentPlanLabel")}</p>
              <p className="text-sm font-semibold text-foreground">
                {t(`plans.${currentPlanKey}`)}
              </p>
            </div>

            {/* Status */}
            <div className="bg-background rounded-xl px-4 py-3">
              <p className="text-xs text-muted-foreground mb-1">{t("statusLabel")}</p>
              <span
                className={`inline-block text-xs font-medium px-2 py-0.5 rounded-full ${
                  isActive
                    ? "bg-success/10 text-success"
                    : "bg-muted text-muted-foreground"
                }`}
              >
                {subscription?.status ?? "Free"}
              </span>
            </div>

            {/* Requests remaining */}
            <div className="bg-background rounded-xl px-4 py-3">
              <p className="text-xs text-muted-foreground mb-1">{t("requestsRemainingLabel")}</p>
              <p className="text-sm font-semibold text-foreground">
                {subscription?.aiRequestsRemaining ?? 0}
              </p>
            </div>

            {/* Valid until — only if active */}
            {isActive && subscription?.endDate && (
              <div className="bg-background rounded-xl px-4 py-3 col-span-2 sm:col-span-3">
                <p className="text-xs text-muted-foreground mb-1">{t("validUntilLabel")}</p>
                <p className="text-sm font-medium text-foreground">
                  {new Date(subscription.endDate).toLocaleDateString()}
                </p>
              </div>
            )}
          </div>
        </div>

        {/* ── Action feedback ──────────────────────────────────────────────── */}
        {actionError && (
          <div className="bg-destructive/5 border border-destructive/20 text-destructive rounded-xl px-4 py-3 text-sm">
            {actionError}
          </div>
        )}
        {successMessage && (
          <div className="bg-success/5 border border-success/20 text-success rounded-xl px-4 py-3 text-sm">
            {successMessage}
          </div>
        )}

        {/* ── Plan cards ───────────────────────────────────────────────────── */}
        <div>
          <h2 className="text-base font-semibold text-foreground mb-1">{t("upgradeTitle")}</h2>
          <p className="text-sm text-muted-foreground mb-4">{t("upgradeSubtitle")}</p>

          <div className="grid gap-4 sm:grid-cols-3">
            {PLANS.map((plan) => {
              const isCurrent = plan.key === currentPlanKey;
              const isActivating = activatingPlan === plan.key;

              return (
                <div
                  key={plan.key}
                  className={`bg-card border rounded-2xl p-5 shadow-card hover:shadow-card-hover transition-shadow flex flex-col gap-4 ${
                    isCurrent
                      ? "border-primary/40 ring-1 ring-primary/20"
                      : "border-border"
                  }`}
                >
                  {/* Plan header */}
                  <div>
                    <div className="flex items-center justify-between mb-1">
                      <span className="text-sm font-semibold text-foreground">
                        {t(`plans.${plan.key}`)}
                      </span>
                      {isCurrent && (
                        <span className="text-xs font-medium text-primary bg-primary/10 px-2 py-0.5 rounded-full">
                          {t("currentPlanBadge")}
                        </span>
                      )}
                    </div>
                    <div className="flex items-baseline gap-1">
                      <span className="text-2xl font-bold text-foreground">
                        ${plan.priceUsd}
                      </span>
                      <span className="text-xs text-muted-foreground">{t("perMonth")}</span>
                    </div>
                  </div>

                  {/* Quota */}
                  <div className="flex items-center gap-1.5 text-sm text-muted-foreground">
                    <Zap className="h-3.5 w-3.5 text-accent" />
                    <span>
                      {plan.quotaPerMonth} {t("requestsPerMonth")}
                    </span>
                  </div>

                  {/* CTA */}
                  <Button
                    size="sm"
                    variant={isCurrent ? "outline" : "default"}
                    className="mt-auto w-full"
                    disabled={isCurrent || isActivating !== false}
                    onClick={() => handleActivate(plan.key)}
                  >
                    {isActivating ? (
                      <>
                        <Loader2 className="h-3.5 w-3.5 animate-spin" />
                        {t("activatingButton")}
                      </>
                    ) : isCurrent ? (
                      <CheckCircle2 className="h-3.5 w-3.5 text-success" />
                    ) : (
                      t("activateButton")
                    )}
                  </Button>
                </div>
              );
            })}
          </div>
        </div>

        {/* ── Top-up form ──────────────────────────────────────────────────── */}
        <div className="bg-card border border-border rounded-2xl p-8 shadow-card">
          <div className="flex items-center gap-3 mb-5">
            <div className="w-11 h-11 rounded-xl bg-accent/10 flex items-center justify-center">
              <Zap className="h-5 w-5 text-accent" />
            </div>
            <div>
              <h2 className="text-base font-semibold text-foreground">{t("topUpTitle")}</h2>
              <p className="text-sm text-muted-foreground">{t("topUpSubtitle")}</p>
            </div>
          </div>

          <form onSubmit={handleSubmit(onTopUp)} className="flex items-end gap-3">
            <div className="flex-1 space-y-1.5">
              <Label htmlFor="count">{t("topUpCountLabel")}</Label>
              <Input
                id="count"
                type="number"
                min={1}
                max={100}
                placeholder={t("topUpCountPlaceholder")}
                {...register("count", { valueAsNumber: true })}
                className={errors.count ? "border-destructive focus-visible:ring-destructive" : ""}
              />
              {errors.count && (
                <p className="text-xs text-destructive">{errors.count.message}</p>
              )}
            </div>

            <Button type="submit" disabled={isSubmitting} className="shrink-0">
              {isSubmitting ? (
                <>
                  <Loader2 className="h-4 w-4 animate-spin" />
                  {t("toppingUpButton")}
                </>
              ) : (
                t("topUpButton")
              )}
            </Button>
          </form>
        </div>

      </div>
    </div>
  );
}
