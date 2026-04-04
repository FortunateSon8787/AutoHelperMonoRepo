"use client";

import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { Loader2, FileCheck } from "lucide-react";
import { useTranslations } from "next-intl";

import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import type { WorkClarificationInput } from "@/types/chat";

interface WorkClarificationFormProps {
  onSubmit: (data: WorkClarificationInput, title: string) => Promise<void>;
  isLoading: boolean;
}

export function WorkClarificationForm({ onSubmit, isLoading }: WorkClarificationFormProps) {
  const t = useTranslations("chat.workClarificationForm");

  const schema = z.object({
    worksPerformed: z.string().min(1, t("validation.worksRequired")),
    workReason: z.string().min(1, t("validation.workReasonRequired")),
    laborCost: z
      .number({ invalid_type_error: t("validation.costInvalid") })
      .min(0, t("validation.costInvalid")),
    partsCost: z
      .number({ invalid_type_error: t("validation.costInvalid") })
      .min(0, t("validation.costInvalid")),
    guarantees: z.string().optional(),
  });

  type FormValues = z.infer<typeof schema>;

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: { laborCost: 0, partsCost: 0 },
  });

  const onFormSubmit = async (values: FormValues) => {
    const title = values.worksPerformed.slice(0, 60) + (values.worksPerformed.length > 60 ? "..." : "");
    await onSubmit(
      {
        worksPerformed: values.worksPerformed,
        workReason: values.workReason,
        laborCost: values.laborCost,
        partsCost: values.partsCost,
        guarantees: values.guarantees || undefined,
      },
      title
    );
  };

  return (
    <div className="bg-card border border-border rounded-2xl p-6 shadow-sm max-w-2xl w-full mx-auto">
      <div className="flex items-center gap-3 mb-5">
        <div className="w-10 h-10 bg-gradient-to-br from-primary to-blue-600 rounded-xl flex items-center justify-center shadow-sm">
          <FileCheck className="w-5 h-5 text-white" />
        </div>
        <h2 className="text-base font-semibold text-foreground">{t("title")}</h2>
      </div>

      <form onSubmit={handleSubmit(onFormSubmit)} className="space-y-4">
        <div className="space-y-1.5">
          <Label htmlFor="worksPerformed">{t("worksPerformedLabel")}</Label>
          <textarea
            id="worksPerformed"
            rows={3}
            placeholder={t("worksPerformedPlaceholder")}
            {...register("worksPerformed")}
            className={`w-full px-3 py-2.5 text-sm border rounded-xl bg-input text-foreground placeholder:text-muted-foreground focus:outline-none focus:ring-2 focus:ring-ring resize-none ${
              errors.worksPerformed ? "border-destructive" : "border-border"
            }`}
          />
          {errors.worksPerformed && (
            <p className="text-xs text-destructive">{errors.worksPerformed.message}</p>
          )}
        </div>

        <div className="space-y-1.5">
          <Label htmlFor="workReason">{t("workReasonLabel")}</Label>
          <Input
            id="workReason"
            placeholder={t("workReasonPlaceholder")}
            {...register("workReason")}
            className={errors.workReason ? "border-destructive" : ""}
          />
          {errors.workReason && (
            <p className="text-xs text-destructive">{errors.workReason.message}</p>
          )}
        </div>

        <div className="grid grid-cols-2 gap-4">
          <div className="space-y-1.5">
            <Label htmlFor="laborCost">{t("laborCostLabel")}</Label>
            <Input
              id="laborCost"
              type="number"
              min={0}
              step={0.01}
              placeholder={t("laborCostPlaceholder")}
              {...register("laborCost", { valueAsNumber: true })}
              className={errors.laborCost ? "border-destructive" : ""}
            />
            {errors.laborCost && (
              <p className="text-xs text-destructive">{errors.laborCost.message}</p>
            )}
          </div>

          <div className="space-y-1.5">
            <Label htmlFor="partsCost">{t("partsCostLabel")}</Label>
            <Input
              id="partsCost"
              type="number"
              min={0}
              step={0.01}
              placeholder={t("partsCostPlaceholder")}
              {...register("partsCost", { valueAsNumber: true })}
              className={errors.partsCost ? "border-destructive" : ""}
            />
            {errors.partsCost && (
              <p className="text-xs text-destructive">{errors.partsCost.message}</p>
            )}
          </div>
        </div>

        <div className="space-y-1.5">
          <Label htmlFor="guarantees">{t("guaranteesLabel")}</Label>
          <Input
            id="guarantees"
            placeholder={t("guaranteesPlaceholder")}
            {...register("guarantees")}
          />
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
