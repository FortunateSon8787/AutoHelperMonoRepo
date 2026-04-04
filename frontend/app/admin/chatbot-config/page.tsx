"use client";

import { useEffect, useState } from "react";
import { useTranslations } from "next-intl";
import { Loader2, Bot, Save, CheckCircle2 } from "lucide-react";

import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { AppHeader } from "@/components/AppHeader";
import { adminService, AdminServiceError } from "@/services/adminService";
import type { AdminChatbotConfig } from "@/types/admin";

// ─── Toggle ───────────────────────────────────────────────────────────────────

function Toggle({
  checked,
  onChange,
  id,
}: {
  checked: boolean;
  onChange: (v: boolean) => void;
  id: string;
}) {
  return (
    <button
      id={id}
      type="button"
      role="switch"
      aria-checked={checked}
      onClick={() => onChange(!checked)}
      className={`relative inline-flex h-6 w-11 shrink-0 cursor-pointer rounded-full border-2 border-transparent transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring ${
        checked ? "bg-primary" : "bg-muted"
      }`}
    >
      <span
        className={`pointer-events-none inline-block h-5 w-5 rounded-full bg-white shadow-lg transition-transform ${
          checked ? "translate-x-5" : "translate-x-0"
        }`}
      />
    </button>
  );
}

// ─── Section Card ─────────────────────────────────────────────────────────────

function SectionCard({
  title,
  children,
}: {
  title: string;
  children: React.ReactNode;
}) {
  return (
    <div className="bg-card border border-border rounded-2xl shadow-card overflow-hidden">
      <div className="px-6 py-4 border-b border-border bg-muted/20">
        <h2 className="text-sm font-semibold text-foreground">{title}</h2>
      </div>
      <div className="px-6 py-5 space-y-5">{children}</div>
    </div>
  );
}

// ─── Field Row ────────────────────────────────────────────────────────────────

function FieldRow({
  label,
  hint,
  children,
  htmlFor,
}: {
  label: string;
  hint?: string;
  children: React.ReactNode;
  htmlFor?: string;
}) {
  return (
    <div className="flex flex-col sm:flex-row sm:items-start gap-3">
      <div className="sm:w-64 shrink-0">
        <Label htmlFor={htmlFor} className="text-sm font-medium text-foreground">
          {label}
        </Label>
        {hint && <p className="text-xs text-muted-foreground mt-0.5">{hint}</p>}
      </div>
      <div className="flex-1">{children}</div>
    </div>
  );
}

// ─── Page ─────────────────────────────────────────────────────────────────────

const PLAN_KEYS = ["None", "Normal", "Pro", "Max"] as const;

export default function AdminChatbotConfigPage() {
  const t = useTranslations("admin.chatbotConfig");

  const [config, setConfig] = useState<AdminChatbotConfig | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [loadError, setLoadError] = useState<string | null>(null);
  const [isSaving, setIsSaving] = useState(false);
  const [saveError, setSaveError] = useState<string | null>(null);
  const [saveSuccess, setSaveSuccess] = useState(false);

  useEffect(() => {
    const load = async () => {
      setIsLoading(true);
      setLoadError(null);
      try {
        const data = await adminService.getChatbotConfig();
        setConfig(data);
      } catch (err) {
        setLoadError(
          err instanceof AdminServiceError
            ? t(`errors.${err.code}` as Parameters<typeof t>[0])
            : t("errors.unknown")
        );
      } finally {
        setIsLoading(false);
      }
    };
    load();
  }, [t]);

  const handleSave = async () => {
    if (!config) return;
    setIsSaving(true);
    setSaveError(null);
    setSaveSuccess(false);
    try {
      await adminService.updateChatbotConfig(config);
      setSaveSuccess(true);
      setTimeout(() => setSaveSuccess(false), 3000);
    } catch (err) {
      setSaveError(
        err instanceof AdminServiceError
          ? t("errors.saveFailed")
          : t("errors.unknown")
      );
    } finally {
      setIsSaving(false);
    }
  };

  const setField = <K extends keyof AdminChatbotConfig>(
    key: K,
    value: AdminChatbotConfig[K]
  ) => setConfig((prev) => (prev ? { ...prev, [key]: value } : prev));

  const setPlanLimit = (plan: string, value: number) =>
    setConfig((prev) =>
      prev
        ? {
            ...prev,
            dailyLimitByPlan: { ...prev.dailyLimitByPlan, [plan]: value },
          }
        : prev
    );

  return (
    <div className="min-h-screen bg-background">
      <AppHeader />

      <div className="max-w-3xl mx-auto px-4 py-10">
        {/* Header */}
        <div className="flex items-start gap-3 mb-8">
          <div className="w-10 h-10 rounded-xl bg-primary/10 flex items-center justify-center shrink-0">
            <Bot className="h-5 w-5 text-primary" />
          </div>
          <div>
            <h1 className="text-xl font-semibold text-foreground">{t("title")}</h1>
            <p className="text-sm text-muted-foreground mt-0.5">{t("subtitle")}</p>
          </div>
        </div>

        {/* Loading */}
        {isLoading && (
          <div className="flex justify-center py-20">
            <Loader2 className="h-6 w-6 animate-spin text-muted-foreground" />
          </div>
        )}

        {/* Load error */}
        {loadError && (
          <div className="bg-destructive/5 border border-destructive/20 text-destructive rounded-xl px-4 py-3 text-sm">
            {loadError}
          </div>
        )}

        {/* Form */}
        {config && !isLoading && (
          <div className="space-y-6">
            {/* General Settings */}
            <SectionCard title={t("sections.general")}>
              <FieldRow
                label={t("fields.isEnabled")}
                hint={t("fields.isEnabledHint")}
                htmlFor="isEnabled"
              >
                <Toggle
                  id="isEnabled"
                  checked={config.isEnabled}
                  onChange={(v) => setField("isEnabled", v)}
                />
              </FieldRow>

              <FieldRow
                label={t("fields.maxCharsPerField")}
                hint={t("fields.maxCharsPerFieldHint")}
                htmlFor="maxCharsPerField"
              >
                <Input
                  id="maxCharsPerField"
                  type="number"
                  min={1}
                  max={10000}
                  value={config.maxCharsPerField}
                  onChange={(e) =>
                    setField("maxCharsPerField", parseInt(e.target.value) || 0)
                  }
                  className="max-w-[180px]"
                />
              </FieldRow>
            </SectionCard>

            {/* Daily Limits */}
            <SectionCard title={t("sections.limits")}>
              {PLAN_KEYS.map((plan) => (
                <FieldRow
                  key={plan}
                  label={t(`fields.plan${plan}` as Parameters<typeof t>[0])}
                  hint={t("fields.dailyLimitHint")}
                  htmlFor={`limit-${plan}`}
                >
                  <Input
                    id={`limit-${plan}`}
                    type="number"
                    min={0}
                    value={config.dailyLimitByPlan[plan] ?? 0}
                    onChange={(e) =>
                      setPlanLimit(plan, parseInt(e.target.value) || 0)
                    }
                    className="max-w-[180px]"
                  />
                </FieldRow>
              ))}
            </SectionCard>

            {/* Top-Up Settings */}
            <SectionCard title={t("sections.topUp")}>
              <FieldRow
                label={t("fields.topUpPrice")}
                hint={t("fields.topUpPriceHint")}
                htmlFor="topUpPriceUsd"
              >
                <Input
                  id="topUpPriceUsd"
                  type="number"
                  min={0.01}
                  step={0.01}
                  value={config.topUpPriceUsd}
                  onChange={(e) =>
                    setField("topUpPriceUsd", parseFloat(e.target.value) || 0)
                  }
                  className="max-w-[180px]"
                />
              </FieldRow>

              <FieldRow
                label={t("fields.topUpRequests")}
                hint={t("fields.topUpRequestsHint")}
                htmlFor="topUpRequestCount"
              >
                <Input
                  id="topUpRequestCount"
                  type="number"
                  min={1}
                  value={config.topUpRequestCount}
                  onChange={(e) =>
                    setField("topUpRequestCount", parseInt(e.target.value) || 0)
                  }
                  className="max-w-[180px]"
                />
              </FieldRow>
            </SectionCard>

            {/* Mode 1 */}
            <SectionCard title={t("sections.mode1")}>
              <FieldRow
                label={t("fields.disablePartnerSuggestions")}
                hint={t("fields.disablePartnerSuggestionsHint")}
                htmlFor="disablePartnerSuggestions"
              >
                <Toggle
                  id="disablePartnerSuggestions"
                  checked={config.disablePartnerSuggestionsInMode1}
                  onChange={(v) =>
                    setField("disablePartnerSuggestionsInMode1", v)
                  }
                />
              </FieldRow>
            </SectionCard>

            {/* Save error */}
            {saveError && (
              <div className="bg-destructive/5 border border-destructive/20 text-destructive rounded-xl px-4 py-3 text-sm">
                {saveError}
              </div>
            )}

            {/* Save success */}
            {saveSuccess && (
              <div className="bg-success/5 border border-success/20 text-success rounded-xl px-4 py-3 text-sm flex items-center gap-2">
                <CheckCircle2 className="h-4 w-4 shrink-0" />
                {t("saveSuccess")}
              </div>
            )}

            {/* Save button */}
            <div className="flex justify-end">
              <Button onClick={handleSave} disabled={isSaving} className="gap-2">
                {isSaving ? (
                  <Loader2 className="h-4 w-4 animate-spin" />
                ) : (
                  <Save className="h-4 w-4" />
                )}
                {isSaving ? t("savingButton") : t("saveButton")}
              </Button>
            </div>
          </div>
        )}
      </div>
    </div>
  );
}
