"use client";

import { useForm, useWatch } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { Loader2, Stethoscope } from "lucide-react";
import { useTranslations } from "next-intl";

import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import type { DiagnosticsInput } from "@/types/chat";

interface DiagnosticsFormProps {
  onSubmit: (data: DiagnosticsInput, title: string) => Promise<void>;
  isLoading: boolean;
}

export function DiagnosticsForm({ onSubmit, isLoading }: DiagnosticsFormProps) {
  const t = useTranslations("chat.diagnosticsForm");

  const schema = z.object({
    symptoms: z.string().min(1, t("validation.symptomsRequired")).max(800, t("validation.symptomsMaxLength")),
    recentEvents: z.string().max(300, t("validation.recentEventsMaxLength")).optional(),
    previousIssues: z.string().max(300, t("validation.previousIssuesMaxLength")).optional(),
  });

  type FormValues = z.infer<typeof schema>;

  const {
    register,
    handleSubmit,
    control,
    formState: { errors },
  } = useForm<FormValues>({ resolver: zodResolver(schema) });

  const symptoms = useWatch({ control, name: "symptoms", defaultValue: "" });
  const recentEvents = useWatch({ control, name: "recentEvents", defaultValue: "" });
  const previousIssues = useWatch({ control, name: "previousIssues", defaultValue: "" });

  const onFormSubmit = async (values: FormValues) => {
    const title = values.symptoms.slice(0, 60) + (values.symptoms.length > 60 ? "..." : "");
    await onSubmit(
      {
        symptoms: values.symptoms,
        recentEvents: values.recentEvents || undefined,
        previousIssues: values.previousIssues || undefined,
      },
      title
    );
  };

  return (
    <div className="bg-card border border-border rounded-2xl p-6 shadow-sm max-w-2xl w-full mx-auto">
      <div className="flex items-center gap-3 mb-5">
        <div className="w-10 h-10 bg-gradient-to-br from-sky-400 to-cyan-500 rounded-xl flex items-center justify-center shadow-sm">
          <Stethoscope className="w-5 h-5 text-white" />
        </div>
        <h2 className="text-base font-semibold text-foreground">{t("title")}</h2>
      </div>

      <form onSubmit={handleSubmit(onFormSubmit)} className="space-y-4">
        <div className="space-y-1.5">
          <div className="flex items-center justify-between">
            <Label htmlFor="symptoms">{t("symptomsLabel")}</Label>
            <span className={`text-xs ${(symptoms?.length ?? 0) > 800 ? "text-destructive" : "text-muted-foreground"}`}>
              {symptoms?.length ?? 0}/800
            </span>
          </div>
          <textarea
            id="symptoms"
            rows={4}
            maxLength={800}
            placeholder={t("symptomsPlaceholder")}
            {...register("symptoms")}
            className={`w-full px-3 py-2.5 text-sm border rounded-xl bg-input text-foreground placeholder:text-muted-foreground focus:outline-none focus:ring-2 focus:ring-ring resize-none ${
              errors.symptoms ? "border-destructive" : "border-border"
            }`}
          />
          {errors.symptoms && (
            <p className="text-xs text-destructive">{errors.symptoms.message}</p>
          )}
        </div>

        <div className="space-y-1.5">
          <div className="flex items-center justify-between">
            <Label htmlFor="recentEvents">{t("recentEventsLabel")}</Label>
            <span className={`text-xs ${(recentEvents?.length ?? 0) > 300 ? "text-destructive" : "text-muted-foreground"}`}>
              {recentEvents?.length ?? 0}/300
            </span>
          </div>
          <Input
            id="recentEvents"
            maxLength={300}
            placeholder={t("recentEventsPlaceholder")}
            {...register("recentEvents")}
          />
          {errors.recentEvents && (
            <p className="text-xs text-destructive">{errors.recentEvents.message}</p>
          )}
        </div>

        <div className="space-y-1.5">
          <div className="flex items-center justify-between">
            <Label htmlFor="previousIssues">{t("previousIssuesLabel")}</Label>
            <span className={`text-xs ${(previousIssues?.length ?? 0) > 300 ? "text-destructive" : "text-muted-foreground"}`}>
              {previousIssues?.length ?? 0}/300
            </span>
          </div>
          <Input
            id="previousIssues"
            maxLength={300}
            placeholder={t("previousIssuesPlaceholder")}
            {...register("previousIssues")}
          />
          {errors.previousIssues && (
            <p className="text-xs text-destructive">{errors.previousIssues.message}</p>
          )}
        </div>

        <Button type="submit" className="w-full" disabled={isLoading}>
          {isLoading ? (
            <>
              <Loader2 className="h-4 w-4 animate-spin" />
              {t("submittingButton")}
            </>
          ) : (
            t("submitButton")
          )}
        </Button>
      </form>
    </div>
  );
}
