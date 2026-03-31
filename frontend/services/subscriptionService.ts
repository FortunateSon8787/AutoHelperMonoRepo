import axios, { AxiosError } from "axios";
import type { SubscriptionInfo } from "@/types/client";

// ─── Axios Instance ───────────────────────────────────────────────────────────

const api = axios.create({
  baseURL: process.env.NEXT_PUBLIC_API_URL,
  headers: { "Content-Type": "application/json" },
  withCredentials: true,
});

// ─── Error Types ─────────────────────────────────────────────────────────────

export type SubscriptionErrorCode =
  | "unauthorized"
  | "notFound"
  | "badRequest"
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

// ─── Types ────────────────────────────────────────────────────────────────────

export interface PlanConfig {
  id: string;
  plan: string;
  priceUsd: number;
  monthlyQuota: number;
}

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
      const response = await api.get<PlanConfig[]>("/api/subscription-plans");
      return response.data;
    } catch {
      return [];
    }
  },

  async activatePlan(plan: string): Promise<void> {
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
