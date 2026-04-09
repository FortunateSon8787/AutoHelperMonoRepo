"use client";

import {
  FileCheck,
  DollarSign,
  Shield,
  Star,
  CheckCircle,
  HelpCircle,
  TrendingUp,
  TrendingDown,
  Minus,
} from "lucide-react";
import { useTranslations } from "next-intl";
import type { WorkClarificationResult } from "@/types/chat";

// ─── Props ────────────────────────────────────────────────────────────────────

interface WorkClarificationResultCardProps {
  result: WorkClarificationResult;
}

// ─── Helpers ──────────────────────────────────────────────────────────────────

type RelevanceLevel = "low" | "medium" | "high" | "unclear";
type PriceLevel = "below_market" | "near_market" | "above_market" | "unknown";
type GuaranteeLevel = "weak" | "normal" | "strong" | "unclear";
type HonestyLevel = "poor" | "mixed" | "fair" | "good" | "unknown";

function RelevanceBadge({
  value,
  label,
}: {
  value: string;
  label: string;
}) {
  const colorClass: Record<RelevanceLevel, string> = {
    low: "bg-destructive/10 text-destructive",
    medium: "bg-warning/10 text-warning",
    high: "bg-success/10 text-success",
    unclear: "bg-muted text-muted-foreground",
  };
  const cls =
    colorClass[value as RelevanceLevel] ?? "bg-muted text-muted-foreground";
  return (
    <span className={`px-2 py-0.5 rounded-md text-xs font-medium ${cls}`}>
      {label}
    </span>
  );
}

function PriceIcon({ value }: { value: string }) {
  if (value === "below_market")
    return <TrendingDown className="w-4 h-4 text-success flex-shrink-0" />;
  if (value === "above_market")
    return <TrendingUp className="w-4 h-4 text-destructive flex-shrink-0" />;
  if (value === "near_market")
    return <Minus className="w-4 h-4 text-info flex-shrink-0" />;
  return <HelpCircle className="w-4 h-4 text-muted-foreground flex-shrink-0" />;
}

function PriceBadge({
  value,
  label,
}: {
  value: string;
  label: string;
}) {
  const colorClass: Record<PriceLevel, string> = {
    below_market: "bg-success/10 text-success",
    near_market: "bg-info/10 text-info",
    above_market: "bg-destructive/10 text-destructive",
    unknown: "bg-muted text-muted-foreground",
  };
  const cls =
    colorClass[value as PriceLevel] ?? "bg-muted text-muted-foreground";
  return (
    <span className={`px-2 py-0.5 rounded-md text-xs font-medium ${cls}`}>
      {label}
    </span>
  );
}

function GuaranteeBadge({
  value,
  label,
}: {
  value: string;
  label: string;
}) {
  const colorClass: Record<GuaranteeLevel, string> = {
    weak: "bg-destructive/10 text-destructive",
    normal: "bg-info/10 text-info",
    strong: "bg-success/10 text-success",
    unclear: "bg-muted text-muted-foreground",
  };
  const cls =
    colorClass[value as GuaranteeLevel] ?? "bg-muted text-muted-foreground";
  return (
    <span className={`px-2 py-0.5 rounded-md text-xs font-medium ${cls}`}>
      {label}
    </span>
  );
}

function HonestyBadge({
  value,
  label,
}: {
  value: string;
  label: string;
}) {
  const colorClass: Record<HonestyLevel, string> = {
    poor: "bg-destructive/10 text-destructive border border-destructive/30",
    mixed: "bg-warning/10 text-warning border border-warning/30",
    fair: "bg-info/10 text-info border border-info/30",
    good: "bg-success/10 text-success border border-success/30",
    unknown: "bg-muted text-muted-foreground border border-border",
  };
  const cls =
    colorClass[value as HonestyLevel] ??
    "bg-muted text-muted-foreground border border-border";
  return (
    <span className={`px-3 py-1 rounded-lg text-sm font-semibold ${cls}`}>
      {label.toUpperCase()}
    </span>
  );
}

// ─── Section component ────────────────────────────────────────────────────────

function AssessmentSection({
  icon,
  label,
  badge,
  explanation,
}: {
  icon: React.ReactNode;
  label: string;
  badge: React.ReactNode;
  explanation: string;
}) {
  return (
    <div className="bg-card rounded-xl p-4 border border-border space-y-2">
      <div className="flex items-center justify-between gap-2 flex-wrap">
        <div className="flex items-center gap-2">
          {icon}
          <span className="text-sm font-medium text-foreground">{label}</span>
        </div>
        {badge}
      </div>
      <p className="text-sm text-muted-foreground leading-relaxed">
        {explanation}
      </p>
    </div>
  );
}

// ─── Component ────────────────────────────────────────────────────────────────

export function WorkClarificationResultCard({
  result,
}: WorkClarificationResultCardProps) {
  const t = useTranslations("chat.workClarificationResult");
  const tRelevance = useTranslations("chat.workClarificationResult.relevanceValues");
  const tPrice = useTranslations("chat.workClarificationResult.priceValues");
  const tGuarantee = useTranslations("chat.workClarificationResult.guaranteeValues");
  const tHonesty = useTranslations("chat.workClarificationResult.honestyValues");

  const relevanceLabel = (v: string) =>
    tRelevance.has(v as Parameters<typeof tRelevance>[0]) ? tRelevance(v as Parameters<typeof tRelevance>[0]) : v;
  const priceLabel = (v: string) =>
    tPrice.has(v as Parameters<typeof tPrice>[0]) ? tPrice(v as Parameters<typeof tPrice>[0]) : v;
  const guaranteeLabel = (v: string) =>
    tGuarantee.has(v as Parameters<typeof tGuarantee>[0]) ? tGuarantee(v as Parameters<typeof tGuarantee>[0]) : v;
  const honestyLabel = (v: string) =>
    tHonesty.has(v as Parameters<typeof tHonesty>[0]) ? tHonesty(v as Parameters<typeof tHonesty>[0]) : v;

  return (
    <div className="space-y-4">
      {/* Header badge */}
      <div className="flex items-center gap-2">
        <div className="flex items-center gap-2 px-2.5 py-1 bg-primary/10 rounded-md">
          <FileCheck className="w-3.5 h-3.5 text-primary" />
          <span className="text-xs font-medium text-primary">{t("title")}</span>
        </div>
      </div>

      {/* Overall honesty */}
      <div className="border-2 border-border rounded-xl p-4 space-y-2">
        <div className="flex items-center gap-2">
          <Star className="w-4 h-4 text-warning flex-shrink-0" />
          <span className="text-sm font-medium text-foreground">
            {t("overallHonesty")}
          </span>
        </div>
        <div className="flex items-center gap-3">
          <HonestyBadge value={result.overall_honesty} label={honestyLabel(result.overall_honesty)} />
        </div>
        {result.overall_explanation && (
          <p className="text-sm text-muted-foreground leading-relaxed">
            {result.overall_explanation}
          </p>
        )}
      </div>

      {/* Work reason relevance */}
      <AssessmentSection
        icon={<CheckCircle className="w-4 h-4 text-info flex-shrink-0" />}
        label={t("workReasonRelevance")}
        badge={<RelevanceBadge value={result.work_reason_relevance} label={relevanceLabel(result.work_reason_relevance)} />}
        explanation={result.work_reason_explanation}
      />

      {/* Labor cost */}
      <AssessmentSection
        icon={<DollarSign className="w-4 h-4 text-muted-foreground flex-shrink-0" />}
        label={t("laborCost")}
        badge={
          <div className="flex items-center gap-1.5">
            <PriceIcon value={result.labor_price_assessment} />
            <PriceBadge value={result.labor_price_assessment} label={priceLabel(result.labor_price_assessment)} />
          </div>
        }
        explanation={result.labor_price_explanation}
      />

      {/* Parts cost */}
      <AssessmentSection
        icon={<DollarSign className="w-4 h-4 text-muted-foreground flex-shrink-0" />}
        label={t("partsCost")}
        badge={
          <div className="flex items-center gap-1.5">
            <PriceIcon value={result.parts_price_assessment} />
            <PriceBadge value={result.parts_price_assessment} label={priceLabel(result.parts_price_assessment)} />
          </div>
        }
        explanation={result.parts_price_explanation}
      />

      {/* Guarantees */}
      <AssessmentSection
        icon={<Shield className="w-4 h-4 text-muted-foreground flex-shrink-0" />}
        label={t("guarantees")}
        badge={<GuaranteeBadge value={result.guarantee_assessment} label={guaranteeLabel(result.guarantee_assessment)} />}
        explanation={result.guarantee_explanation}
      />

      {/* Next service interval */}
      {result.repeat_interval_km && (
        <div className="bg-secondary border border-border rounded-xl p-4">
          <p className="text-sm text-foreground">
            <span className="font-medium">{t("nextService")}: </span>
            {t("nextServiceValue", { km: result.repeat_interval_km.toLocaleString() })}
          </p>
        </div>
      )}

      {/* Disclaimer */}
      {result.disclaimer && (
        <div className="bg-muted/50 border border-border rounded-xl p-4 space-y-1">
          <div className="text-xs font-medium text-muted-foreground">
            {t("disclaimer")}
          </div>
          <p className="text-xs text-muted-foreground leading-relaxed">
            {result.disclaimer}
          </p>
        </div>
      )}
    </div>
  );
}
