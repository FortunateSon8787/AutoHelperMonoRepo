"use client";

import { useState } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { Loader2, Building2 } from "lucide-react";
import { useRouter } from "next/navigation";
import { useTranslations } from "next-intl";

import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { partnerService, PartnerServiceError } from "@/services/partnerService";
import { PARTNER_TYPES } from "@/types/partner";

// ─── Page ─────────────────────────────────────────────────────────────────────

export default function PartnerRegisterPage() {
  const t = useTranslations("partner.register");
  const tValidation = useTranslations("partner.register.validation");
  const tErrors = useTranslations("partner.register.errors");
  const router = useRouter();

  const [serverError, setServerError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);

  // ─── Schema ────────────────────────────────────────────────────────────────

  const schema = z.object({
    name: z.string().min(1, tValidation("nameRequired")).max(256, tValidation("nameMaxLength")),
    type: z.string().min(1, tValidation("typeRequired")),
    specialization: z.string().min(1, tValidation("specializationRequired")),
    description: z.string().min(1, tValidation("descriptionRequired")),
    address: z.string().min(1, tValidation("addressRequired")),
    locationLat: z
      .number({ invalid_type_error: tValidation("latInvalid") })
      .min(-90, tValidation("latInvalid"))
      .max(90, tValidation("latInvalid")),
    locationLng: z
      .number({ invalid_type_error: tValidation("lngInvalid") })
      .min(-180, tValidation("lngInvalid"))
      .max(180, tValidation("lngInvalid")),
    workingOpenFrom: z
      .string()
      .min(1, tValidation("timeFormat"))
      .regex(/^\d{2}:\d{2}$/, tValidation("timeFormat")),
    workingOpenTo: z
      .string()
      .min(1, tValidation("timeFormat"))
      .regex(/^\d{2}:\d{2}$/, tValidation("timeFormat")),
    workingDays: z.string().min(1, tValidation("workingDaysRequired")),
    contactsPhone: z.string().min(1, tValidation("phoneRequired")),
    contactsWebsite: z.string().optional().nullable(),
  });

  type FormValues = z.infer<typeof schema>;

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: { locationLat: 55.75, locationLng: 37.61 },
  });

  const onSubmit = async (values: FormValues) => {
    setServerError(null);
    setSuccessMessage(null);
    try {
      await partnerService.registerPartner({
        name: values.name,
        type: values.type,
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
      setSuccessMessage(t("successMessage"));
      setTimeout(() => router.push("/partner/cabinet"), 2000);
    } catch (error) {
      if (error instanceof PartnerServiceError) {
        setServerError(tErrors(error.code as Parameters<typeof tErrors>[0]));
      } else {
        setServerError(tErrors("unknown"));
      }
    }
  };

  return (
    <div className="min-h-screen bg-gray-50 px-4 py-10">
      <div className="max-w-2xl mx-auto">
        {/* Header */}
        <div className="flex items-center gap-3 mb-8">
          <div className="w-9 h-9 rounded-lg bg-gray-900 flex items-center justify-center text-white font-bold text-lg">
            A
          </div>
          <span className="text-xl font-bold text-gray-900">AutoHelper</span>
        </div>

        <div className="bg-white border border-gray-200 rounded-xl p-8 shadow-sm">
          {/* Title */}
          <div className="flex items-center gap-3 mb-6">
            <div className="w-10 h-10 rounded-full bg-gray-100 flex items-center justify-center">
              <Building2 className="h-5 w-5 text-gray-500" />
            </div>
            <div>
              <h1 className="text-xl font-bold text-gray-900">{t("title")}</h1>
              <p className="text-sm text-gray-500">{t("subtitle")}</p>
            </div>
          </div>

          {/* Server error */}
          {serverError && (
            <div className="bg-red-50 border border-red-200 text-red-700 rounded-lg px-4 py-3 text-sm mb-5">
              {serverError}
            </div>
          )}

          {/* Success */}
          {successMessage && (
            <div className="bg-green-50 border border-green-200 text-green-700 rounded-lg px-4 py-3 text-sm mb-5">
              {successMessage}
            </div>
          )}

          <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
            {/* Name */}
            <div className="space-y-1.5">
              <Label htmlFor="name">{t("nameLabel")}</Label>
              <Input
                id="name"
                placeholder={t("namePlaceholder")}
                {...register("name")}
                className={errors.name ? "border-red-500" : ""}
              />
              {errors.name && <p className="text-xs text-red-500">{errors.name.message}</p>}
            </div>

            {/* Type */}
            <div className="space-y-1.5">
              <Label htmlFor="type">{t("typeLabel")}</Label>
              <select
                id="type"
                {...register("type")}
                className={`w-full h-10 rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 ${errors.type ? "border-red-500" : ""}`}
              >
                <option value="">—</option>
                {PARTNER_TYPES.map((pt) => (
                  <option key={pt} value={pt}>
                    {t(`types.${pt}` as Parameters<typeof t>[0])}
                  </option>
                ))}
              </select>
              {errors.type && <p className="text-xs text-red-500">{errors.type.message}</p>}
            </div>

            {/* Specialization */}
            <div className="space-y-1.5">
              <Label htmlFor="specialization">{t("specializationLabel")}</Label>
              <Input
                id="specialization"
                placeholder={t("specializationPlaceholder")}
                {...register("specialization")}
                className={errors.specialization ? "border-red-500" : ""}
              />
              {errors.specialization && <p className="text-xs text-red-500">{errors.specialization.message}</p>}
            </div>

            {/* Description */}
            <div className="space-y-1.5">
              <Label htmlFor="description">{t("descriptionLabel")}</Label>
              <textarea
                id="description"
                placeholder={t("descriptionPlaceholder")}
                rows={3}
                {...register("description")}
                className={`w-full rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 resize-none ${errors.description ? "border-red-500" : ""}`}
              />
              {errors.description && <p className="text-xs text-red-500">{errors.description.message}</p>}
            </div>

            {/* Address */}
            <div className="space-y-1.5">
              <Label htmlFor="address">{t("addressLabel")}</Label>
              <Input
                id="address"
                placeholder={t("addressPlaceholder")}
                {...register("address")}
                className={errors.address ? "border-red-500" : ""}
              />
              {errors.address && <p className="text-xs text-red-500">{errors.address.message}</p>}
            </div>

            {/* Location */}
            <div className="grid grid-cols-2 gap-4">
              <div className="space-y-1.5">
                <Label htmlFor="locationLat">{t("locationLatLabel")}</Label>
                <Input
                  id="locationLat"
                  type="number"
                  step="any"
                  {...register("locationLat", { valueAsNumber: true })}
                  className={errors.locationLat ? "border-red-500" : ""}
                />
                {errors.locationLat && <p className="text-xs text-red-500">{errors.locationLat.message}</p>}
              </div>
              <div className="space-y-1.5">
                <Label htmlFor="locationLng">{t("locationLngLabel")}</Label>
                <Input
                  id="locationLng"
                  type="number"
                  step="any"
                  {...register("locationLng", { valueAsNumber: true })}
                  className={errors.locationLng ? "border-red-500" : ""}
                />
                {errors.locationLng && <p className="text-xs text-red-500">{errors.locationLng.message}</p>}
              </div>
            </div>

            {/* Working hours */}
            <div className="grid grid-cols-2 gap-4">
              <div className="space-y-1.5">
                <Label htmlFor="workingOpenFrom">{t("workingOpenFromLabel")}</Label>
                <Input
                  id="workingOpenFrom"
                  placeholder={t("workingOpenFromPlaceholder")}
                  {...register("workingOpenFrom")}
                  className={errors.workingOpenFrom ? "border-red-500" : ""}
                />
                {errors.workingOpenFrom && <p className="text-xs text-red-500">{errors.workingOpenFrom.message}</p>}
              </div>
              <div className="space-y-1.5">
                <Label htmlFor="workingOpenTo">{t("workingOpenToLabel")}</Label>
                <Input
                  id="workingOpenTo"
                  placeholder={t("workingOpenToPlaceholder")}
                  {...register("workingOpenTo")}
                  className={errors.workingOpenTo ? "border-red-500" : ""}
                />
                {errors.workingOpenTo && <p className="text-xs text-red-500">{errors.workingOpenTo.message}</p>}
              </div>
            </div>

            {/* Working days */}
            <div className="space-y-1.5">
              <Label htmlFor="workingDays">{t("workingDaysLabel")}</Label>
              <Input
                id="workingDays"
                placeholder={t("workingDaysPlaceholder")}
                {...register("workingDays")}
                className={errors.workingDays ? "border-red-500" : ""}
              />
              {errors.workingDays && <p className="text-xs text-red-500">{errors.workingDays.message}</p>}
            </div>

            {/* Phone */}
            <div className="space-y-1.5">
              <Label htmlFor="contactsPhone">{t("contactsPhoneLabel")}</Label>
              <Input
                id="contactsPhone"
                type="tel"
                placeholder={t("contactsPhonePlaceholder")}
                {...register("contactsPhone")}
                className={errors.contactsPhone ? "border-red-500" : ""}
              />
              {errors.contactsPhone && <p className="text-xs text-red-500">{errors.contactsPhone.message}</p>}
            </div>

            {/* Website */}
            <div className="space-y-1.5">
              <Label htmlFor="contactsWebsite">{t("contactsWebsiteLabel")}</Label>
              <Input
                id="contactsWebsite"
                type="url"
                placeholder={t("contactsWebsitePlaceholder")}
                {...register("contactsWebsite")}
              />
            </div>

            <Button type="submit" className="w-full mt-2" disabled={isSubmitting}>
              {isSubmitting ? (
                <>
                  <Loader2 className="h-4 w-4 animate-spin mr-2" />
                  {t("submittingButton")}
                </>
              ) : (
                t("submitButton")
              )}
            </Button>
          </form>
        </div>
      </div>
    </div>
  );
}
