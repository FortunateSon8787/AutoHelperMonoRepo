"use client";

import { useEffect, useState } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { Loader2, Building2, CheckCircle, Clock, XCircle } from "lucide-react";
import { useTranslations } from "next-intl";

import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { AppHeader } from "@/components/AppHeader";
import { nativeTextareaCn } from "@/lib/form-styles";
import { partnerService, PartnerServiceError } from "@/services/partnerService";
import type { PartnerProfile } from "@/types/partner";

// ─── Page ─────────────────────────────────────────────────────────────────────

export default function PartnerCabinetPage() {
  const t = useTranslations("partner.cabinet");
  const tErrors = useTranslations("partner.cabinet.errors");
  const tRegValidation = useTranslations("partner.register.validation");

  const [profile, setProfile] = useState<PartnerProfile | null>(null);
  const [loadError, setLoadError] = useState<string | null>(null);
  const [serverError, setServerError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [isEditing, setIsEditing] = useState(false);

  // ─── Schema ────────────────────────────────────────────────────────────────

  const schema = z.object({
    name: z.string().min(1, tRegValidation("nameRequired")).max(256, tRegValidation("nameMaxLength")),
    specialization: z.string().min(1, tRegValidation("specializationRequired")),
    description: z.string().min(1, tRegValidation("descriptionRequired")),
    address: z.string().min(1, tRegValidation("addressRequired")),
    locationLat: z
      .number({ invalid_type_error: tRegValidation("latInvalid") })
      .min(-90, tRegValidation("latInvalid"))
      .max(90, tRegValidation("latInvalid")),
    locationLng: z
      .number({ invalid_type_error: tRegValidation("lngInvalid") })
      .min(-180, tRegValidation("lngInvalid"))
      .max(180, tRegValidation("lngInvalid")),
    workingOpenFrom: z
      .string()
      .regex(/^\d{2}:\d{2}$/, tRegValidation("timeFormat")),
    workingOpenTo: z
      .string()
      .regex(/^\d{2}:\d{2}$/, tRegValidation("timeFormat")),
    workingDays: z.string().min(1, tRegValidation("workingDaysRequired")),
    contactsPhone: z.string().min(1, tRegValidation("phoneRequired")),
    contactsWebsite: z.string().optional().nullable(),
  });

  type FormValues = z.infer<typeof schema>;

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors, isSubmitting },
  } = useForm<FormValues>({ resolver: zodResolver(schema) });

  useEffect(() => {
    partnerService
      .getMyProfile()
      .then((data) => {
        setProfile(data);
        reset({
          name: data.name,
          specialization: data.specialization,
          description: data.description,
          address: data.address,
          locationLat: data.locationLat,
          locationLng: data.locationLng,
          workingOpenFrom: data.workingOpenFrom,
          workingOpenTo: data.workingOpenTo,
          workingDays: data.workingDays,
          contactsPhone: data.contactsPhone,
          contactsWebsite: data.contactsWebsite ?? "",
        });
      })
      .catch((err: unknown) => {
        if (err instanceof PartnerServiceError) {
          setLoadError(tErrors(err.code as Parameters<typeof tErrors>[0]));
        } else {
          setLoadError(tErrors("unknown"));
        }
      })
      .finally(() => setIsLoading(false));
  }, [reset, tErrors]);

  const onSubmit = async (values: FormValues) => {
    setServerError(null);
    setSuccessMessage(null);
    try {
      await partnerService.updateMyProfile({
        name: values.name,
        specialization: values.specialization,
        description: values.description,
        address: values.address,
        locationLat: values.locationLat,
        locationLng: values.locationLng,
        workingOpenFrom: values.workingOpenFrom,
        workingOpenTo: values.workingOpenTo,
        workingDays: values.workingDays,
        contactsPhone: values.contactsPhone,
        contactsWebsite: values.contactsWebsite || null,
      });
      setSuccessMessage(t("saveSuccess"));
      setProfile((prev) =>
        prev ? { ...prev, name: values.name, specialization: values.specialization } : prev
      );
      setIsEditing(false);
    } catch (error) {
      if (error instanceof PartnerServiceError) {
        setServerError(tErrors(error.code as Parameters<typeof tErrors>[0]));
      } else {
        setServerError(tErrors("unknown"));
      }
    }
  };

  const handleCancel = () => {
    if (profile) {
      reset({
        name: profile.name,
        specialization: profile.specialization,
        description: profile.description,
        address: profile.address,
        locationLat: profile.locationLat,
        locationLng: profile.locationLng,
        workingOpenFrom: profile.workingOpenFrom,
        workingOpenTo: profile.workingOpenTo,
        workingDays: profile.workingDays,
        contactsPhone: profile.contactsPhone,
        contactsWebsite: profile.contactsWebsite ?? "",
      });
    }
    setIsEditing(false);
    setServerError(null);
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

  return (
    <div className="min-h-screen bg-background">
      <AppHeader />

      <div className="max-w-2xl mx-auto px-4 py-10">
        {/* Pending verification banner */}
        {profile && !profile.isActive && (
          <div className="flex items-center gap-2 bg-amber-50 border border-amber-200 text-amber-800 rounded-xl px-4 py-3 text-sm mb-5">
            <Clock className="h-4 w-4 flex-shrink-0" />
            <span>
              {profile.isVerified ? t("inactive") : t("pendingVerification")}
            </span>
          </div>
        )}

        <div className="bg-card border border-border rounded-2xl p-8 shadow-card">
          {/* Title row */}
          <div className="flex items-center justify-between mb-6">
            <div className="flex items-center gap-3">
              <div className="w-11 h-11 rounded-xl bg-primary/10 flex items-center justify-center">
                <Building2 className="h-5 w-5 text-primary" />
              </div>
              <div>
                <h1 className="text-xl font-semibold text-foreground">{t("title")}</h1>
                <p className="text-sm text-muted-foreground">
                  {t(`types.${profile?.type as Parameters<typeof t>[0]}`)}
                </p>
              </div>
            </div>

            {!isEditing && (
              <Button variant="outline" size="sm" onClick={() => setIsEditing(true)}>
                {t("editButton")}
              </Button>
            )}
          </div>

          {/* Verification status */}
          <div className="flex items-center gap-4 mb-6 pb-6 border-b border-border text-xs text-muted-foreground">
            <span className="flex items-center gap-1">
              {profile?.isVerified ? (
                <CheckCircle className="h-3.5 w-3.5 text-success" />
              ) : (
                <Clock className="h-3.5 w-3.5 text-amber-500" />
              )}
              <span className={profile?.isVerified ? "text-success" : "text-amber-600"}>
                {profile?.isVerified ? t("verified") : t("notVerified")}
              </span>
            </span>
            <span className="flex items-center gap-1">
              {profile?.isActive ? (
                <CheckCircle className="h-3.5 w-3.5 text-success" />
              ) : (
                <XCircle className="h-3.5 w-3.5 text-muted-foreground" />
              )}
              <span className={profile?.isActive ? "text-success" : "text-muted-foreground"}>
                {profile?.isActive ? t("active") : t("notActive")}
              </span>
            </span>
          </div>

          {/* Server error */}
          {serverError && (
            <div className="bg-destructive/5 border border-destructive/20 text-destructive rounded-xl px-4 py-3 text-sm mb-5">
              {serverError}
            </div>
          )}

          {/* Success */}
          {successMessage && (
            <div className="bg-success/5 border border-success/20 text-success rounded-xl px-4 py-3 text-sm mb-5">
              {successMessage}
            </div>
          )}

          {/* View mode */}
          {!isEditing && profile && (
            <div className="space-y-4 text-sm">
              <InfoRow label={t("nameLabel")} value={profile.name} />
              <InfoRow label={t("specializationLabel")} value={profile.specialization} />
              <InfoRow label={t("descriptionLabel")} value={profile.description} />
              <InfoRow label={t("addressLabel")} value={profile.address} />
              <InfoRow
                label={t("workingHoursLabel")}
                value={`${profile.workingOpenFrom} – ${profile.workingOpenTo} (${profile.workingDays})`}
              />
              <InfoRow label={t("phoneLabel")} value={profile.contactsPhone} />
              {profile.contactsWebsite && (
                <InfoRow label={t("websiteLabel")} value={profile.contactsWebsite} />
              )}
            </div>
          )}

          {/* Edit mode */}
          {isEditing && (
            <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
              <div className="space-y-1.5">
                <Label htmlFor="name">{t("nameLabel")}</Label>
                <Input id="name" {...register("name")} className={errors.name ? "border-destructive" : ""} />
                {errors.name && <p className="text-xs text-destructive">{errors.name.message}</p>}
              </div>

              <div className="space-y-1.5">
                <Label htmlFor="specialization">{t("specializationLabel")}</Label>
                <Input id="specialization" {...register("specialization")} className={errors.specialization ? "border-destructive" : ""} />
                {errors.specialization && <p className="text-xs text-destructive">{errors.specialization.message}</p>}
              </div>

              <div className="space-y-1.5">
                <Label htmlFor="description">{t("descriptionLabel")}</Label>
                <textarea
                  id="description"
                  rows={3}
                  {...register("description")}
                  className={errors.description ? `${nativeTextareaCn} border-destructive` : nativeTextareaCn}
                />
                {errors.description && <p className="text-xs text-destructive">{errors.description.message}</p>}
              </div>

              <div className="space-y-1.5">
                <Label htmlFor="address">{t("addressLabel")}</Label>
                <Input id="address" {...register("address")} className={errors.address ? "border-destructive" : ""} />
                {errors.address && <p className="text-xs text-destructive">{errors.address.message}</p>}
              </div>

              <div className="grid grid-cols-2 gap-4">
                <div className="space-y-1.5">
                  <Label htmlFor="locationLat">Lat</Label>
                  <Input id="locationLat" type="number" step="any" {...register("locationLat", { valueAsNumber: true })} className={errors.locationLat ? "border-destructive" : ""} />
                </div>
                <div className="space-y-1.5">
                  <Label htmlFor="locationLng">Lng</Label>
                  <Input id="locationLng" type="number" step="any" {...register("locationLng", { valueAsNumber: true })} className={errors.locationLng ? "border-destructive" : ""} />
                </div>
              </div>

              <div className="grid grid-cols-2 gap-4">
                <div className="space-y-1.5">
                  <Label htmlFor="workingOpenFrom">Open from</Label>
                  <Input id="workingOpenFrom" placeholder="09:00" {...register("workingOpenFrom")} className={errors.workingOpenFrom ? "border-destructive" : ""} />
                </div>
                <div className="space-y-1.5">
                  <Label htmlFor="workingOpenTo">Open to</Label>
                  <Input id="workingOpenTo" placeholder="18:00" {...register("workingOpenTo")} className={errors.workingOpenTo ? "border-destructive" : ""} />
                </div>
              </div>

              <div className="space-y-1.5">
                <Label htmlFor="workingDays">{t("workingHoursLabel")}</Label>
                <Input id="workingDays" placeholder="Mon-Fri" {...register("workingDays")} className={errors.workingDays ? "border-destructive" : ""} />
              </div>

              <div className="space-y-1.5">
                <Label htmlFor="contactsPhone">{t("phoneLabel")}</Label>
                <Input id="contactsPhone" type="tel" {...register("contactsPhone")} className={errors.contactsPhone ? "border-destructive" : ""} />
                {errors.contactsPhone && <p className="text-xs text-destructive">{errors.contactsPhone.message}</p>}
              </div>

              <div className="space-y-1.5">
                <Label htmlFor="contactsWebsite">{t("websiteLabel")}</Label>
                <Input id="contactsWebsite" type="url" {...register("contactsWebsite")} />
              </div>

              <div className="flex gap-3 mt-2">
                <Button type="submit" className="flex-1" size="lg" disabled={isSubmitting}>
                  {isSubmitting ? (
                    <>
                      <Loader2 className="h-4 w-4 animate-spin" />
                      {t("savingButton")}
                    </>
                  ) : (
                    t("saveButton")
                  )}
                </Button>
                <Button type="button" variant="outline" size="lg" className="flex-1" onClick={handleCancel}>
                  {t("cancelButton")}
                </Button>
              </div>
            </form>
          )}
        </div>
      </div>
    </div>
  );
}

// ─── Helper component ─────────────────────────────────────────────────────────

function InfoRow({ label, value }: { label: string; value: string }) {
  return (
    <div className="flex gap-2">
      <span className="text-muted-foreground min-w-[140px]">{label}:</span>
      <span className="text-foreground">{value}</span>
    </div>
  );
}
