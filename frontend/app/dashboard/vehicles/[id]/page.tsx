"use client";

import { useEffect, useRef, useState } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { useParams, useRouter } from "next/navigation";
import Link from "next/link";
import { Loader2, ArrowLeft } from "lucide-react";
import { useTranslations } from "next-intl";

import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { AppHeader } from "@/components/AppHeader";
import { cn } from "@/lib/utils";
import { nativeSelectCn } from "@/lib/form-styles";
import { vehicleService, VehicleServiceError } from "@/services/vehicleService";
import type { Vehicle, VehicleStatus } from "@/types/vehicle";

// ─── Status badge ──────────────────────────────────────────────────────────────

const STATUS_COLORS: Record<VehicleStatus, string> = {
  Active: "bg-success/10 text-success",
  ForSale: "bg-accent/10 text-accent",
  InRepair: "bg-amber-50 text-amber-700",
  Recycled: "bg-secondary text-muted-foreground",
  Dismantled: "bg-destructive/10 text-destructive",
};

const ALL_STATUSES: VehicleStatus[] = ["Active", "ForSale", "InRepair", "Recycled", "Dismantled"];

// ─── Page ─────────────────────────────────────────────────────────────────────

export default function VehicleDetailPage() {
  const t = useTranslations("vehicles.form");
  const ts = useTranslations("vehicles.status");
  const tList = useTranslations("vehicles.list");
  const params = useParams<{ id: string }>();
  const router = useRouter();

  const [vehicle, setVehicle] = useState<Vehicle | null>(null);
  const [loadError, setLoadError] = useState<string | null>(null);
  const [serverError, setServerError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  // ─── Status section state ────────────────────────────────────────────────
  const [selectedStatus, setSelectedStatus] = useState<VehicleStatus>("Active");
  const [statusError, setStatusError] = useState<string | null>(null);
  const [statusSuccess, setStatusSuccess] = useState<string | null>(null);
  const [isStatusSubmitting, setIsStatusSubmitting] = useState(false);
  const [partnerName, setPartnerName] = useState("");
  const [partnerNameError, setPartnerNameError] = useState<string | null>(null);
  const [documentFile, setDocumentFile] = useState<File | null>(null);
  const [documentError, setDocumentError] = useState<string | null>(null);
  const fileInputRef = useRef<HTMLInputElement>(null);

  // ─── Validation schema ────────────────────────────────────────────────────

  const editSchema = z.object({
    brand: z
      .string()
      .min(1, t("validation.brandRequired"))
      .max(128, t("validation.brandMaxLength")),
    model: z
      .string()
      .min(1, t("validation.modelRequired"))
      .max(128, t("validation.modelMaxLength")),
    year: z
      .number({ invalid_type_error: t("validation.yearRequired") })
      .int()
      .min(1900, t("validation.yearInvalid"))
      .max(new Date().getFullYear() + 1, t("validation.yearInvalid")),
    color: z.string().max(64).nullable().optional(),
    mileage: z
      .number({ invalid_type_error: t("validation.mileageInvalid") })
      .int()
      .min(0, t("validation.mileageInvalid")),
  });

  type EditFormValues = z.infer<typeof editSchema>;

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors, isSubmitting },
  } = useForm<EditFormValues>({
    resolver: zodResolver(editSchema),
  });

  // ─── Load vehicle ─────────────────────────────────────────────────────────

  useEffect(() => {
    if (!params.id) return;

    vehicleService
      .getById(params.id)
      .then((data) => {
        setVehicle(data);
        setSelectedStatus(data.status);
        reset({
          brand: data.brand,
          model: data.model,
          year: data.year,
          color: data.color ?? "",
          mileage: data.mileage,
        });
      })
      .catch((err: unknown) => {
        setLoadError(err instanceof VehicleServiceError ? t(`errors.${err.code}`) : t("errors.unknown"));
      })
      .finally(() => setIsLoading(false));
  }, [params.id, reset, t]);

  // ─── Save details ─────────────────────────────────────────────────────────

  const onSubmit = async (values: EditFormValues) => {
    if (!params.id) return;
    setServerError(null);
    setSuccessMessage(null);

    try {
      await vehicleService.update(params.id, {
        brand: values.brand,
        model: values.model,
        year: values.year,
        color: values.color ?? null,
        mileage: values.mileage,
      });

      setSuccessMessage(t("saveSuccess"));
      setVehicle((prev) =>
        prev
          ? {
              ...prev,
              brand: values.brand,
              model: values.model,
              year: values.year,
              color: values.color ?? null,
              mileage: values.mileage,
            }
          : prev
      );
    } catch (error) {
      setServerError(error instanceof VehicleServiceError ? t(`errors.${error.code}`) : t("errors.unknown"));
    }
  };

  // ─── Change status ────────────────────────────────────────────────────────

  const onStatusChange = async () => {
    if (!params.id) return;

    setStatusError(null);
    setStatusSuccess(null);
    setPartnerNameError(null);
    setDocumentError(null);

    if (selectedStatus === "InRepair") {
      if (!partnerName.trim()) {
        setPartnerNameError(ts("validation.partnerNameRequired"));
        return;
      }
      if (partnerName.trim().length > 256) {
        setPartnerNameError(ts("validation.partnerNameMaxLength"));
        return;
      }
    }

    if (selectedStatus === "Recycled" || selectedStatus === "Dismantled") {
      if (!documentFile) {
        setDocumentError(ts("validation.documentRequired"));
        return;
      }
      if (documentFile.type !== "application/pdf") {
        setDocumentError(ts("validation.documentNotPdf"));
        return;
      }
      if (documentFile.size > 10 * 1024 * 1024) {
        setDocumentError(ts("validation.documentTooLarge"));
        return;
      }
    }

    setIsStatusSubmitting(true);
    try {
      await vehicleService.updateStatus(params.id, {
        status: selectedStatus,
        partnerName: selectedStatus === "InRepair" ? partnerName.trim() : undefined,
        document: selectedStatus === "Recycled" || selectedStatus === "Dismantled" ? documentFile ?? undefined : undefined,
      });

      setStatusSuccess(ts("changeSuccess"));
      setVehicle((prev) => (prev ? { ...prev, status: selectedStatus } : prev));

      if (selectedStatus !== "InRepair") setPartnerName("");
      if (selectedStatus !== "Recycled" && selectedStatus !== "Dismantled") {
        setDocumentFile(null);
        if (fileInputRef.current) fileInputRef.current.value = "";
      }
    } catch (error) {
      const code = error instanceof VehicleServiceError ? error.code : "unknown";
      const key = code === "notFound" ? "notFound" : code === "badRequest" ? "badRequest" : code === "serverError" ? "serverError" : "unknown";
      setStatusError(ts(`errors.${key}`));
    } finally {
      setIsStatusSubmitting(false);
    }
  };

  // ─── Render ───────────────────────────────────────────────────────────────

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

  return (
    <div className="min-h-screen bg-background">
      <AppHeader />

      <div className="max-w-lg mx-auto px-4 py-10">
        {/* Back link */}
        <Link
          href="/dashboard/vehicles"
          className="inline-flex items-center gap-1.5 text-sm text-muted-foreground hover:text-foreground mb-6 transition-colors"
        >
          <ArrowLeft className="h-4 w-4" />
          {tList("title")}
        </Link>

        {/* ─── Edit details card ───────────────────────────────────────────── */}
        <div className="bg-card border border-border rounded-2xl p-8 shadow-card mb-6">
          <h1 className="text-xl font-semibold text-foreground mb-1">{t("editTitle")}</h1>
          {vehicle && (
            <p className="text-sm text-muted-foreground mb-6">VIN: {vehicle.vin}</p>
          )}

          {serverError && (
            <div className="bg-destructive/5 border border-destructive/20 text-destructive rounded-xl px-4 py-3 text-sm mb-5">
              {serverError}
            </div>
          )}

          {successMessage && (
            <div className="bg-success/5 border border-success/20 text-success rounded-xl px-4 py-3 text-sm mb-5">
              {successMessage}
            </div>
          )}

          <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
            <div className="grid grid-cols-2 gap-4">
              <div className="space-y-1.5">
                <Label htmlFor="brand">{t("brandLabel")}</Label>
                <Input
                  id="brand"
                  placeholder={t("brandPlaceholder")}
                  {...register("brand")}
                  className={errors.brand ? "border-destructive" : ""}
                />
                {errors.brand && <p className="text-xs text-destructive">{errors.brand.message}</p>}
              </div>

              <div className="space-y-1.5">
                <Label htmlFor="model">{t("modelLabel")}</Label>
                <Input
                  id="model"
                  placeholder={t("modelPlaceholder")}
                  {...register("model")}
                  className={errors.model ? "border-destructive" : ""}
                />
                {errors.model && <p className="text-xs text-destructive">{errors.model.message}</p>}
              </div>

              <div className="space-y-1.5">
                <Label htmlFor="year">{t("yearLabel")}</Label>
                <Input
                  id="year"
                  type="number"
                  placeholder={t("yearPlaceholder")}
                  {...register("year", { valueAsNumber: true })}
                  className={errors.year ? "border-destructive" : ""}
                />
                {errors.year && <p className="text-xs text-destructive">{errors.year.message}</p>}
              </div>

              <div className="space-y-1.5">
                <Label htmlFor="color">{t("colorLabel")}</Label>
                <Input
                  id="color"
                  placeholder={t("colorPlaceholder")}
                  {...register("color")}
                  className={errors.color ? "border-destructive" : ""}
                />
                {errors.color && <p className="text-xs text-destructive">{errors.color.message}</p>}
              </div>

              <div className="col-span-2 space-y-1.5">
                <Label htmlFor="mileage">{t("mileageLabel")}</Label>
                <Input
                  id="mileage"
                  type="number"
                  placeholder={t("mileagePlaceholder")}
                  {...register("mileage", { valueAsNumber: true })}
                  className={errors.mileage ? "border-destructive" : ""}
                />
                {errors.mileage && <p className="text-xs text-destructive">{errors.mileage.message}</p>}
              </div>
            </div>

            <Button type="submit" className="w-full mt-2" size="lg" disabled={isSubmitting}>
              {isSubmitting ? (
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

        {/* ─── Status management card ──────────────────────────────────────── */}
        <div className="bg-card border border-border rounded-2xl p-8 shadow-card">
          <h2 className="text-lg font-semibold text-foreground mb-1">{ts("sectionTitle")}</h2>

          {vehicle && (
            <p className="text-sm text-muted-foreground mb-6">
              {ts("currentStatus")}:{" "}
              <span className={cn("inline-block px-2 py-0.5 rounded-lg text-xs font-medium", STATUS_COLORS[vehicle.status])}>
                {ts(`values.${vehicle.status}`)}
              </span>
              {vehicle.partnerName && (
                <span className="ml-2 text-muted-foreground">· {vehicle.partnerName}</span>
              )}
            </p>
          )}

          {statusError && (
            <div className="bg-destructive/5 border border-destructive/20 text-destructive rounded-xl px-4 py-3 text-sm mb-5">
              {statusError}
            </div>
          )}

          {statusSuccess && (
            <div className="bg-success/5 border border-success/20 text-success rounded-xl px-4 py-3 text-sm mb-5">
              {statusSuccess}
            </div>
          )}

          <div className="space-y-4">
            {/* Status select */}
            <div className="space-y-1.5">
              <Label htmlFor="status">{ts("statusLabel")}</Label>
              <select
                id="status"
                value={selectedStatus}
                onChange={(e) => {
                  setSelectedStatus(e.target.value as VehicleStatus);
                  setPartnerNameError(null);
                  setDocumentError(null);
                  setStatusError(null);
                  setStatusSuccess(null);
                }}
                className={nativeSelectCn}
              >
                {ALL_STATUSES.map((s) => (
                  <option key={s} value={s}>
                    {ts(`values.${s}`)}
                  </option>
                ))}
              </select>
            </div>

            {/* InRepair: partner name */}
            {selectedStatus === "InRepair" && (
              <div className="space-y-1.5">
                <Label htmlFor="partnerName">{ts("partnerNameLabel")}</Label>
                <Input
                  id="partnerName"
                  placeholder={ts("partnerNamePlaceholder")}
                  value={partnerName}
                  onChange={(e) => setPartnerName(e.target.value)}
                  className={partnerNameError ? "border-destructive" : ""}
                />
                {partnerNameError && (
                  <p className="text-xs text-destructive">{partnerNameError}</p>
                )}
              </div>
            )}

            {/* Recycled / Dismantled: document upload */}
            {(selectedStatus === "Recycled" || selectedStatus === "Dismantled") && (
              <div className="space-y-1.5">
                <Label htmlFor="document">{ts("documentLabel")}</Label>
                <input
                  ref={fileInputRef}
                  id="document"
                  type="file"
                  accept="application/pdf"
                  onChange={(e) => {
                    setDocumentFile(e.target.files?.[0] ?? null);
                    setDocumentError(null);
                  }}
                  className="block w-full text-sm text-muted-foreground file:mr-4 file:py-1.5 file:px-3 file:rounded-lg file:border-0 file:text-sm file:font-medium file:bg-secondary file:text-foreground hover:file:bg-muted cursor-pointer"
                />
                <p className="text-xs text-muted-foreground">{ts("documentHint")}</p>
                {documentError && (
                  <p className="text-xs text-destructive">{documentError}</p>
                )}
              </div>
            )}

            <Button
              type="button"
              className="w-full"
              size="lg"
              onClick={onStatusChange}
              disabled={isStatusSubmitting}
            >
              {isStatusSubmitting ? (
                <>
                  <Loader2 className="h-4 w-4 animate-spin" />
                  {ts("changingButton")}
                </>
              ) : (
                ts("changeButton")
              )}
            </Button>
          </div>
        </div>
      </div>
    </div>
  );
}
