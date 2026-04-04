"use client";

import { useEffect, useState } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import Link from "next/link";
import { Loader2, Car, Plus, X, MessageCircle } from "lucide-react";
import { useTranslations } from "next-intl";

import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { AppHeader } from "@/components/AppHeader";
import { vehicleService, VehicleServiceError } from "@/services/vehicleService";
import type { Vehicle } from "@/types/vehicle";

// ─── Page ─────────────────────────────────────────────────────────────────────

export default function MyVehiclesPage() {
  const t = useTranslations("vehicles.list");
  const tf = useTranslations("vehicles.form");

  const [vehicles, setVehicles] = useState<Vehicle[]>([]);
  const [loadError, setLoadError] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [showAddForm, setShowAddForm] = useState(false);
  const [formError, setFormError] = useState<string | null>(null);
  const [formSuccess, setFormSuccess] = useState<string | null>(null);

  // ─── Validation schema ────────────────────────────────────────────────────

  const addSchema = z.object({
    vin: z
      .string()
      .min(1, tf("validation.vinRequired"))
      .regex(/^[A-HJ-NPR-Za-hj-npr-z0-9]{17}$/, tf("validation.vinInvalid")),
    brand: z
      .string()
      .min(1, tf("validation.brandRequired"))
      .max(128, tf("validation.brandMaxLength")),
    model: z
      .string()
      .min(1, tf("validation.modelRequired"))
      .max(128, tf("validation.modelMaxLength")),
    year: z
      .number({ invalid_type_error: tf("validation.yearRequired") })
      .int()
      .min(1900, tf("validation.yearInvalid"))
      .max(new Date().getFullYear() + 1, tf("validation.yearInvalid")),
    color: z.string().max(64).nullable().optional(),
    mileage: z
      .number({ invalid_type_error: tf("validation.mileageInvalid") })
      .int()
      .min(0, tf("validation.mileageInvalid")),
  });

  type AddFormValues = z.infer<typeof addSchema>;

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors, isSubmitting },
  } = useForm<AddFormValues>({
    resolver: zodResolver(addSchema),
    defaultValues: { mileage: 0 },
  });

  // ─── Load vehicles ────────────────────────────────────────────────────────

  useEffect(() => {
    vehicleService
      .getMyVehicles()
      .then(setVehicles)
      .catch((err: unknown) => {
        setLoadError(err instanceof VehicleServiceError ? tf(`errors.${err.code}`) : tf("errors.unknown"));
      })
      .finally(() => setIsLoading(false));
  }, [tf]);

  // ─── Add vehicle ──────────────────────────────────────────────────────────

  const onAdd = async (values: AddFormValues) => {
    setFormError(null);
    setFormSuccess(null);
    try {
      const { vehicleId } = await vehicleService.create({
        vin: values.vin.trim().toUpperCase(),
        brand: values.brand,
        model: values.model,
        year: values.year,
        color: values.color ?? null,
        mileage: values.mileage,
      });

      const newVehicle = await vehicleService.getById(vehicleId);
      setVehicles((prev) => [...prev, newVehicle]);
      setFormSuccess(tf("createSuccess"));
      reset({ mileage: 0 });
      setShowAddForm(false);
    } catch (error) {
      setFormError(error instanceof VehicleServiceError ? tf(`errors.${error.code}`) : tf("errors.unknown"));
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
      <AppHeader>
        <Link href="/dashboard/chat">
          <Button size="sm" variant="outline" className="gap-1.5">
            <MessageCircle className="h-4 w-4" />
            AI
          </Button>
        </Link>
      </AppHeader>

      <div className="max-w-2xl mx-auto px-4 py-10">
        {/* Title + Add button */}
        <div className="flex items-center justify-between mb-6">
          <div className="flex items-center gap-2">
            <div className="w-9 h-9 rounded-xl bg-accent/10 flex items-center justify-center">
              <Car className="h-4 w-4 text-accent" />
            </div>
            <h1 className="text-xl font-semibold text-foreground">{t("title")}</h1>
          </div>
          <Button
            size="sm"
            onClick={() => {
              setShowAddForm((v) => !v);
              setFormError(null);
              setFormSuccess(null);
            }}
            variant={showAddForm ? "outline" : "default"}
          >
            {showAddForm ? <X className="h-4 w-4" /> : <Plus className="h-4 w-4" />}
            {showAddForm ? tf("cancelButton") : t("addButton")}
          </Button>
        </div>

        {/* Success message */}
        {formSuccess && (
          <div className="bg-success/5 border border-success/20 text-success rounded-xl px-4 py-3 text-sm mb-5">
            {formSuccess}
          </div>
        )}

        {/* Add form */}
        {showAddForm && (
          <div className="bg-card border border-border rounded-2xl p-6 shadow-card mb-6">
            <h2 className="text-base font-semibold text-foreground mb-4">{tf("addTitle")}</h2>

            {formError && (
              <div className="bg-destructive/5 border border-destructive/20 text-destructive rounded-xl px-4 py-3 text-sm mb-4">
                {formError}
              </div>
            )}

            <form onSubmit={handleSubmit(onAdd)} className="space-y-4">
              <div className="grid grid-cols-2 gap-4">
                <div className="col-span-2 space-y-1.5">
                  <Label htmlFor="vin">{tf("vinLabel")}</Label>
                  <Input
                    id="vin"
                    placeholder={tf("vinPlaceholder")}
                    {...register("vin")}
                    className={errors.vin ? "border-destructive" : ""}
                  />
                  {errors.vin && <p className="text-xs text-destructive">{errors.vin.message}</p>}
                </div>

                <div className="space-y-1.5">
                  <Label htmlFor="brand">{tf("brandLabel")}</Label>
                  <Input
                    id="brand"
                    placeholder={tf("brandPlaceholder")}
                    {...register("brand")}
                    className={errors.brand ? "border-destructive" : ""}
                  />
                  {errors.brand && <p className="text-xs text-destructive">{errors.brand.message}</p>}
                </div>

                <div className="space-y-1.5">
                  <Label htmlFor="model">{tf("modelLabel")}</Label>
                  <Input
                    id="model"
                    placeholder={tf("modelPlaceholder")}
                    {...register("model")}
                    className={errors.model ? "border-destructive" : ""}
                  />
                  {errors.model && <p className="text-xs text-destructive">{errors.model.message}</p>}
                </div>

                <div className="space-y-1.5">
                  <Label htmlFor="year">{tf("yearLabel")}</Label>
                  <Input
                    id="year"
                    type="number"
                    placeholder={tf("yearPlaceholder")}
                    {...register("year", { valueAsNumber: true })}
                    className={errors.year ? "border-destructive" : ""}
                  />
                  {errors.year && <p className="text-xs text-destructive">{errors.year.message}</p>}
                </div>

                <div className="space-y-1.5">
                  <Label htmlFor="color">{tf("colorLabel")}</Label>
                  <Input
                    id="color"
                    placeholder={tf("colorPlaceholder")}
                    {...register("color")}
                    className={errors.color ? "border-destructive" : ""}
                  />
                  {errors.color && <p className="text-xs text-destructive">{errors.color.message}</p>}
                </div>

                <div className="space-y-1.5">
                  <Label htmlFor="mileage">{tf("mileageLabel")}</Label>
                  <Input
                    id="mileage"
                    type="number"
                    placeholder={tf("mileagePlaceholder")}
                    {...register("mileage", { valueAsNumber: true })}
                    className={errors.mileage ? "border-destructive" : ""}
                  />
                  {errors.mileage && <p className="text-xs text-destructive">{errors.mileage.message}</p>}
                </div>
              </div>

              <Button type="submit" className="w-full" disabled={isSubmitting}>
                {isSubmitting ? (
                  <>
                    <Loader2 className="h-4 w-4 animate-spin" />
                    {tf("submittingButton")}
                  </>
                ) : (
                  tf("addButton")
                )}
              </Button>
            </form>
          </div>
        )}

        {/* Vehicle list */}
        {vehicles.length === 0 ? (
          <div className="bg-card border border-border rounded-2xl p-10 text-center shadow-card">
            <div className="w-14 h-14 rounded-2xl bg-muted flex items-center justify-center mx-auto mb-3">
              <Car className="h-7 w-7 text-muted-foreground" />
            </div>
            <p className="text-sm text-muted-foreground">{t("emptyState")}</p>
          </div>
        ) : (
          <div className="space-y-3">
            {vehicles.map((v) => (
              <div
                key={v.id}
                className="bg-card border border-border rounded-2xl px-6 py-4 shadow-card hover:shadow-card-hover transition-shadow flex items-center justify-between"
              >
                <div>
                  <p className="font-semibold text-foreground">
                    {v.brand} {v.model}{" "}
                    <span className="text-muted-foreground font-normal">({v.year})</span>
                  </p>
                  <p className="text-xs text-muted-foreground mt-0.5">
                    {t("vinLabel")}: {v.vin}
                    {v.color ? ` · ${v.color}` : ""}
                    {` · ${v.mileage.toLocaleString()} ${t("km")}`}
                  </p>
                </div>
                <Link href={`/dashboard/vehicles/${v.id}`}>
                  <Button size="sm" variant="outline">
                    {t("editButton")}
                  </Button>
                </Link>
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  );
}
