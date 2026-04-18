import { AxiosError } from "axios";
import { z } from "zod";
import { apiClient as api } from "@/lib/apiClient";
import type { SubscriptionInfo } from "@/types/client";

// ─── Error Types ─────────────────────────────────────────────────────────────

export type SubscriptionErrorCode =
  | "unauthorized"
  | "notFound"
  | "badRequest"
  | "invalidPlan"
  | "serverError"
  | "unknown";

export class SubscriptionServiceError extends Error {
  constructor(public readonly code: SubscriptionErrorCode) {
    super(code);
    this.name = "SubscriptionServiceError";
  }
}

function resolveErrorCode(error: unknown): SubscriptionErrorCode {
  if (error instanceof AxiosError) {
    const status = error.response?.status;
    if (status === 401) return "unauthorized";
    if (status === 404) return "notFound";
    if (status === 400) return "badRequest";
    if (status !== undefined && status >= 500) return "serverError";
  }
  return "unknown";
}

// ─── Allowed Plans ────────────────────────────────────────────────────────────

/** Must match SubscriptionPlan enum values on the backend (excluding None). */
export const ALLOWED_PLANS = ["Normal", "Pro", "Max"] as const;
export type AllowedPlan = (typeof ALLOWED_PLANS)[number];

export function isAllowedPlan(value: string): value is AllowedPlan {
  return (ALLOWED_PLANS as readonly string[]).includes(value);
}

// ─── Schemas ─────────────────────────────────────────────────────────────────

const planConfigSchema = z.object({
  id: z.string().uuid(),
  plan: z.enum(ALLOWED_PLANS),
  priceUsd: z.number().positive(),
  monthlyQuota: z.number().int().positive(),
});

const planConfigsSchema = z.array(planConfigSchema);

export type PlanConfig = z.infer<typeof planConfigSchema>;

// ─── Subscription Service ─────────────────────────────────────────────────────

export const subscriptionService = {
  async getMySubscription(): Promise<SubscriptionInfo> {
    try {
      const response = await api.get<SubscriptionInfo>("/api/clients/me/subscription");
      return response.data;
    } catch (error) {
      throw new SubscriptionServiceError(resolveErrorCode(error));
    }
  },

  async getPlanConfigs(): Promise<PlanConfig[]> {
    try {
      const response = await api.get<unknown>("/api/subscription-plans");
      const parsed = planConfigsSchema.safeParse(response.data);
      return parsed.success ? parsed.data : [];
    } catch {
      return [];
    }
  },

  async activatePlan(plan: AllowedPlan): Promise<void> {
    try {
      await api.post("/api/clients/me/subscription/activate", { plan });
    } catch (error) {
      throw new SubscriptionServiceError(resolveErrorCode(error));
    }
  },

  async topUpRequests(count: number): Promise<void> {
    try {
      await api.post("/api/clients/me/subscription/topup", { count });
    } catch (error) {
      throw new SubscriptionServiceError(resolveErrorCode(error));
    }
  },
};
