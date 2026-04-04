"use client";

import { useState } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { Loader2, MapPin, Navigation } from "lucide-react";
import { useTranslations } from "next-intl";

import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import type { PartnerAdviceInput } from "@/types/chat";

interface PartnerAdviceFormProps {
  onSubmit: (data: PartnerAdviceInput, title: string) => Promise<void>;
  isLoading: boolean;
}

export function PartnerAdviceForm({ onSubmit, isLoading }: PartnerAdviceFormProps) {
  const t = useTranslations("chat.partnerAdviceForm");
  const [isLocating, setIsLocating] = useState(false);

  const schema = z.object({
    request: z.string().min(1, t("validation.requestRequired")),
    urgency: z.string().optional(),
    lat: z
      .number({ invalid_type_error: t("validation.latInvalid") })
      .min(-90, t("validation.latInvalid"))
      .max(90, t("validation.latInvalid")),
    lng: z
      .number({ invalid_type_error: t("validation.lngInvalid") })
      .min(-180, t("validation.lngInvalid"))
      .max(180, t("validation.lngInvalid")),
  });

  type FormValues = z.infer<typeof schema>;

  const {
    register,
    handleSubmit,
    setValue,
    formState: { errors },
  } = useForm<FormValues>({ resolver: zodResolver(schema) });

  const handleLocate = () => {
    if (!navigator.geolocation) return;
    setIsLocating(true);
    navigator.geolocation.getCurrentPosition(
      (pos) => {
        setValue("lat", parseFloat(pos.coords.latitude.toFixed(6)));
        setValue("lng", parseFloat(pos.coords.longitude.toFixed(6)));
        setIsLocating(false);
      },
      () => setIsLocating(false)
    );
  };

  const onFormSubmit = async (values: FormValues) => {
    const title = values.request.slice(0, 60) + (values.request.length > 60 ? "..." : "");
    await onSubmit(
      {
        request: values.request,
        lat: values.lat,
        lng: values.lng,
        urgency: values.urgency || undefined,
      },
      title
    );
  };

  return (
    <div className="bg-card border border-border rounded-2xl p-6 shadow-sm max-w-2xl w-full mx-auto">
      <div className="flex items-center gap-3 mb-5">
        <div className="w-10 h-10 bg-gradient-to-br from-emerald-500 to-green-600 rounded-xl flex items-center justify-center shadow-sm">
          <MapPin className="w-5 h-5 text-white" />
        </div>
        <h2 className="text-base font-semibold text-foreground">{t("title")}</h2>
      </div>

      <form onSubmit={handleSubmit(onFormSubmit)} className="space-y-4">
        <div className="space-y-1.5">
          <Label htmlFor="request">{t("requestLabel")}</Label>
          <textarea
            id="request"
            rows={3}
            placeholder={t("requestPlaceholder")}
            {...register("request")}
            className={`w-full px-3 py-2.5 text-sm border rounded-xl bg-input text-foreground placeholder:text-muted-foreground focus:outline-none focus:ring-2 focus:ring-ring resize-none ${
              errors.request ? "border-destructive" : "border-border"
            }`}
          />
          {errors.request && (
            <p className="text-xs text-destructive">{errors.request.message}</p>
          )}
        </div>

        <div className="space-y-1.5">
          <Label htmlFor="urgency">{t("urgencyLabel")}</Label>
          <Input
            id="urgency"
            placeholder={t("urgencyPlaceholder")}
            {...register("urgency")}
          />
        </div>

        {/* Location */}
        <div className="bg-muted/40 rounded-xl p-4 border border-border space-y-3">
          <div className="flex items-center justify-between">
            <div className="text-sm font-medium text-foreground flex items-center gap-2">
              <Navigation className="w-4 h-4 text-muted-foreground" />
              {t("locationTitle")}
            </div>
            <Button
              type="button"
              size="sm"
              variant="outline"
              onClick={handleLocate}
              disabled={isLocating}
              className="gap-1.5 h-7 text-xs"
            >
              {isLocating ? (
                <Loader2 className="w-3 h-3 animate-spin" />
              ) : (
                <Navigation className="w-3 h-3" />
              )}
              {isLocating ? t("locating") : t("useMyLocation")}
            </Button>
          </div>

          <div className="grid grid-cols-2 gap-3">
            <div className="space-y-1.5">
              <Label htmlFor="lat">{t("latLabel")}</Label>
              <Input
                id="lat"
                type="number"
                step={0.000001}
                placeholder="47.0000"
                {...register("lat", { valueAsNumber: true })}
                className={errors.lat ? "border-destructive" : ""}
              />
              {errors.lat && (
                <p className="text-xs text-destructive">{errors.lat.message}</p>
              )}
            </div>
            <div className="space-y-1.5">
              <Label htmlFor="lng">{t("lngLabel")}</Label>
              <Input
                id="lng"
                type="number"
                step={0.000001}
                placeholder="28.0000"
                {...register("lng", { valueAsNumber: true })}
                className={errors.lng ? "border-destructive" : ""}
              />
              {errors.lng && (
                <p className="text-xs text-destructive">{errors.lng.message}</p>
              )}
            </div>
          </div>
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
