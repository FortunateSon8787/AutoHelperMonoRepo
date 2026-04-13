"use client";

import { useState } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { Loader2, MapPin, Navigation, RefreshCw } from "lucide-react";
import { useTranslations } from "next-intl";

import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import type { PartnerAdviceInput } from "@/types/chat";

interface PartnerAdviceFormProps {
  onSubmit: (data: PartnerAdviceInput, title: string) => Promise<void>;
  isLoading: boolean;
}

const GMAPS_KEY = process.env.NEXT_PUBLIC_GOOGLE_MAPS_KEY ?? "";

function buildStaticMapUrl(lat: number, lng: number): string {
  const params = new URLSearchParams({
    center: `${lat},${lng}`,
    zoom: "14",
    size: "600x200",
    scale: "2",
    maptype: "roadmap",
    markers: `color:red|${lat},${lng}`,
    key: GMAPS_KEY,
  });
  return `https://maps.googleapis.com/maps/api/staticmap?${params.toString()}`;
}

export function PartnerAdviceForm({ onSubmit, isLoading }: PartnerAdviceFormProps) {
  const t = useTranslations("chat.partnerAdviceForm");
  const [isLocating, setIsLocating] = useState(false);
  const [location, setLocation] = useState<{ lat: number; lng: number } | null>(null);

  const schema = z.object({
    request: z
      .string()
      .min(1, t("validation.requestRequired"))
      .max(500, t("validation.requestMaxLength")),
    urgency: z.string().max(100, t("validation.urgencyMaxLength")).optional(),
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
    watch,
    formState: { errors },
  } = useForm<FormValues>({ resolver: zodResolver(schema) });

  const requestValue = watch("request") ?? "";
  const urgencyValue = watch("urgency") ?? "";

  const handleLocate = () => {
    if (!navigator.geolocation) return;
    setIsLocating(true);
    navigator.geolocation.getCurrentPosition(
      (pos) => {
        const lat = parseFloat(pos.coords.latitude.toFixed(6));
        const lng = parseFloat(pos.coords.longitude.toFixed(6));
        setValue("lat", lat);
        setValue("lng", lng);
        setLocation({ lat, lng });
        setIsLocating(false);
      },
      () => setIsLocating(false)
    );
  };

  const handleReset = () => {
    setLocation(null);
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
          <div className="flex items-center justify-between">
            <Label htmlFor="request">{t("requestLabel")}</Label>
            <span className={`text-xs ${requestValue.length > 500 ? "text-destructive" : "text-muted-foreground"}`}>
              {requestValue.length}/500
            </span>
          </div>
          <textarea
            id="request"
            rows={3}
            placeholder={t("requestPlaceholder")}
            maxLength={500}
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
          <div className="flex items-center justify-between">
            <Label htmlFor="urgency">{t("urgencyLabel")}</Label>
            <span className={`text-xs ${urgencyValue.length > 100 ? "text-destructive" : "text-muted-foreground"}`}>
              {urgencyValue.length}/100
            </span>
          </div>
          <Input
            id="urgency"
            placeholder={t("urgencyPlaceholder")}
            maxLength={100}
            {...register("urgency")}
          />
          {errors.urgency && (
            <p className="text-xs text-destructive">{errors.urgency.message}</p>
          )}
        </div>

        {/* Location */}
        <div className="bg-muted/40 rounded-xl border border-border overflow-hidden">
          <div className="flex items-center justify-between px-4 py-3">
            <div className="text-sm font-medium text-foreground flex items-center gap-2">
              <Navigation className="w-4 h-4 text-muted-foreground" />
              {t("locationTitle")}
            </div>
            {location ? (
              <Button
                type="button"
                size="sm"
                variant="outline"
                onClick={handleReset}
                className="gap-1.5 h-7 text-xs"
              >
                <RefreshCw className="w-3 h-3" />
                {t("changeLocation")}
              </Button>
            ) : (
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
            )}
          </div>

          {location ? (
            <div className="relative w-full h-[200px]">
              {/* eslint-disable-next-line @next/next/no-img-element */}
              <img
                src={buildStaticMapUrl(location.lat, location.lng)}
                alt="Your location on map"
                className="w-full h-full object-cover"
              />
              <div className="absolute bottom-2 right-2 bg-background/80 backdrop-blur-sm rounded-lg px-2.5 py-1 text-xs text-muted-foreground font-mono">
                {location.lat.toFixed(4)}, {location.lng.toFixed(4)}
              </div>
            </div>
          ) : (
            <div className="px-4 pb-4">
              {(errors.lat || errors.lng) && (
                <p className="text-xs text-destructive mt-1">
                  {errors.lat?.message || errors.lng?.message}
                </p>
              )}
              <p className="text-xs text-muted-foreground mt-1">{t("locationHint")}</p>
            </div>
          )}

          {/* Hidden inputs to keep lat/lng in form state */}
          <input type="hidden" {...register("lat", { valueAsNumber: true })} />
          <input type="hidden" {...register("lng", { valueAsNumber: true })} />
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
