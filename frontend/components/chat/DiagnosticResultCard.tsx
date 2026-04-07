"use client";

import {
  Info,
  AlertTriangle,
  AlertCircle,
  CheckCircle,
  ShieldCheck,
  ShieldOff,
} from "lucide-react";
import { useTranslations } from "next-intl";
import type { DiagnosticResult, DiagnosticProblem } from "@/types/chat";

// ─── Props ────────────────────────────────────────────────────────────────────

interface DiagnosticResultCardProps {
  result: DiagnosticResult;
}

// ─── Helpers ──────────────────────────────────────────────────────────────────

function ProbabilityBadge({ probability }: { probability: number }) {
  const pct = Math.round(probability * 100);
  const colorClass =
    pct >= 70
      ? "bg-destructive/10 text-destructive"
      : pct >= 40
        ? "bg-warning/10 text-warning"
        : "bg-info/10 text-info";

  return (
    <span className={`px-2 py-0.5 rounded-md text-xs font-medium ${colorClass}`}>
      {pct}%
    </span>
  );
}

function ProblemCard({
  problem,
  t,
}: {
  problem: DiagnosticProblem;
  t: ReturnType<typeof useTranslations>;
}) {
  return (
    <div className="bg-card rounded-xl p-4 border border-border">
      <div className="flex items-start justify-between mb-2 gap-2">
        <div className="text-sm font-medium text-foreground">{problem.name}</div>
        <ProbabilityBadge probability={problem.probability} />
      </div>
      {problem.possible_causes && (
        <p className="text-sm text-muted-foreground mt-1">
          <span className="font-medium">{t("possibleCauses")}:</span>{" "}
          {problem.possible_causes}
        </p>
      )}
    </div>
  );
}

// ─── Component ────────────────────────────────────────────────────────────────

export function DiagnosticResultCard({ result }: DiagnosticResultCardProps) {
  const t = useTranslations("chat.diagnosticResult");

  const urgencyLabel = result.urgency
    ? (t.has(`urgencyValues.${result.urgency}`)
        ? t(`urgencyValues.${result.urgency}` as Parameters<typeof t>[0])
        : result.urgency.toUpperCase())
    : null;

  const urgencyColorClass =
    result.urgency === "stop_driving"
      ? "bg-destructive/5 border-destructive/30 text-destructive"
      : result.urgency === "high"
        ? "bg-warning/5 border-warning/30 text-warning"
        : result.urgency === "medium"
          ? "bg-warning/5 border-warning/20 text-warning"
          : "bg-info/5 border-info/20 text-info";

  const sortedProblems = result.potential_problems
    ? [...result.potential_problems].sort((a, b) => b.probability - a.probability)
    : [];

  // Collect all recommended actions from problems
  const recommendedActions = sortedProblems
    .filter((p) => p.recommended_actions)
    .map((p) => ({ name: p.name, action: p.recommended_actions! }));

  return (
    <div className="space-y-4">
      {/* Header badge */}
      <div className="flex items-center gap-2">
        <div className="flex items-center gap-2 px-2.5 py-1 bg-info/10 rounded-md">
          <Info className="w-3.5 h-3.5 text-info" />
          <span className="text-xs font-medium text-info">{t("title")}</span>
        </div>
      </div>

      {/* Urgency */}
      {urgencyLabel && (
        <div className={`border rounded-xl p-4 space-y-1 ${urgencyColorClass}`}>
          <div className="flex items-center gap-2">
            <AlertTriangle className="w-4 h-4 flex-shrink-0" />
            <span className="text-sm font-medium">{t("urgency")}</span>
          </div>
          <div className="text-base font-semibold">{urgencyLabel}</div>
        </div>
      )}

      {/* Potential problems */}
      {sortedProblems.length > 0 && (
        <div className="space-y-3">
          <div className="text-sm font-medium text-foreground">{t("summary")}</div>
          <div className="space-y-2">
            {sortedProblems.map((problem, i) => (
              <ProblemCard key={i} problem={problem} t={t} />
            ))}
          </div>
        </div>
      )}

      {/* Current risks */}
      {result.current_risks && (
        <div className="bg-destructive/5 border border-destructive/20 rounded-xl p-4 space-y-2">
          <div className="flex items-center gap-2">
            <AlertCircle className="w-4 h-4 text-destructive" />
            <span className="text-sm font-medium text-destructive">{t("currentRisks")}</span>
          </div>
          <p className="text-sm text-foreground leading-relaxed">{result.current_risks}</p>
        </div>
      )}

      {/* Recommended actions */}
      {recommendedActions.length > 0 && (
        <div className="bg-card rounded-xl p-4 border border-border space-y-3">
          <div className="text-sm font-medium text-foreground">{t("recommendedActions")}</div>
          <ol className="space-y-2">
            {recommendedActions.map((item, i) => (
              <li key={i} className="flex items-start gap-3 text-sm text-foreground">
                <span className="font-medium text-primary flex-shrink-0">{i + 1}.</span>
                <div>
                  <span className="font-medium">{item.name}:</span> {item.action}
                </div>
              </li>
            ))}
          </ol>
        </div>
      )}

      {/* Safe to drive */}
      {result.safe_to_drive !== null && result.safe_to_drive !== undefined && (
        <div
          className={`border-2 rounded-xl p-4 space-y-1 ${
            result.safe_to_drive
              ? "bg-success/5 border-success/30"
              : "bg-warning/5 border-warning/30"
          }`}
        >
          <div
            className={`flex items-center gap-2 text-sm font-medium ${
              result.safe_to_drive ? "text-success" : "text-warning"
            }`}
          >
            {result.safe_to_drive ? (
              <ShieldCheck className="w-4 h-4" />
            ) : (
              <ShieldOff className="w-4 h-4" />
            )}
            <span>{t("safeToDrive")}</span>
          </div>
          <div
            className={`text-base font-semibold ${
              result.safe_to_drive ? "text-success" : "text-warning"
            }`}
          >
            {result.safe_to_drive ? t("safeYes") : t("safeNo")}
          </div>
        </div>
      )}

      {/* Disclaimer */}
      {result.disclaimer && (
        <div className="bg-muted/50 border border-border rounded-xl p-4 space-y-1">
          <div className="text-xs font-medium text-muted-foreground">{t("disclaimer")}</div>
          <p className="text-xs text-muted-foreground leading-relaxed">{result.disclaimer}</p>
        </div>
      )}
    </div>
  );
}
