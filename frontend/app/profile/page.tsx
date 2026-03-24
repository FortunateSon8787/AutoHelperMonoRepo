"use client";

import { useEffect, useState } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { Loader2, User } from "lucide-react";
import { useTranslations } from "next-intl";

import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { profileService, ProfileServiceError } from "@/services/profileService";
import type { ClientProfile } from "@/types/client";

// ─── Page ─────────────────────────────────────────────────────────────────────

export default function ProfilePage() {
  const t = useTranslations("profile");
  const tValidation = useTranslations("profile.validation");
  const tErrors = useTranslations("profile.errors");
  const [profile, setProfile] = useState<ClientProfile | null>(null);
  const [loadError, setLoadError] = useState<string | null>(null);
  const [serverError, setServerError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  // ─── Schema (uses translations — must live inside component) ──────────────

  const profileSchema = z.object({
    name: z
      .string()
      .min(1, tValidation("nameRequired"))
      .max(256, tValidation("nameTooLong")),
    contacts: z
      .string()
      .max(512, tValidation("contactsTooLong"))
      .nullable()
      .optional()
      .transform((v: string | null | undefined) => v || null),
  });

  type ProfileFormValues = z.infer<typeof profileSchema>;

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors, isSubmitting },
  } = useForm<ProfileFormValues>({
    resolver: zodResolver(profileSchema),
  });

  useEffect(() => {
    profileService
      .getMyProfile()
      .then((data) => {
        setProfile(data);
        reset({ name: data.name, contacts: data.contacts ?? "" });
      })
      .catch((err: unknown) => {
        if (err instanceof ProfileServiceError) {
          setLoadError(tErrors(err.code));
        } else {
          setLoadError(tErrors("unknown"));
        }
      })
      .finally(() => setIsLoading(false));
  }, [reset]);

  const onSubmit = async (values: ProfileFormValues) => {
    setServerError(null);
    setSuccessMessage(null);
    try {
      await profileService.updateMyProfile({
        name: values.name,
        contacts: values.contacts ?? null,
      });
      setSuccessMessage(t("saveSuccess"));
      setProfile((prev) =>
        prev ? { ...prev, name: values.name, contacts: values.contacts ?? null } : prev
      );
    } catch (error) {
      if (error instanceof ProfileServiceError) {
        setServerError(tErrors(error.code));
      }
    }
  };

  if (isLoading) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-gray-50">
        <Loader2 className="h-6 w-6 animate-spin text-gray-400" />
      </div>
    );
  }

  if (loadError) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-gray-50 px-4">
        <div className="bg-red-50 border border-red-200 text-red-700 rounded-lg px-6 py-4 text-sm">
          {loadError}
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-50 px-4 py-10">
      <div className="max-w-lg mx-auto">
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
              <User className="h-5 w-5 text-gray-500" />
            </div>
            <div>
              <h1 className="text-xl font-bold text-gray-900">{t("title")}</h1>
              <p className="text-sm text-gray-500">{profile?.email}</p>
            </div>
          </div>

          {/* Meta info */}
          <div className="flex items-center gap-4 mb-6 pb-6 border-b border-gray-100 text-xs text-gray-400">
            <span>{t("subscription")}: <span className="text-gray-600 font-medium">{profile?.subscriptionStatus}</span></span>
            <span>{t("provider")}: <span className="text-gray-600 font-medium">{profile?.authProvider}</span></span>
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

          {/* Form */}
          <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
            {/* Name */}
            <div className="space-y-1.5">
              <Label htmlFor="name">{t("nameLabel")}</Label>
              <Input
                id="name"
                type="text"
                placeholder={t("namePlaceholder")}
                autoComplete="name"
                {...register("name")}
                className={errors.name ? "border-red-500" : ""}
              />
              {errors.name && (
                <p className="text-xs text-red-500">{errors.name.message}</p>
              )}
            </div>

            {/* Email — read-only */}
            <div className="space-y-1.5">
              <Label htmlFor="email">{t("emailLabel")}</Label>
              <Input
                id="email"
                type="email"
                value={profile?.email ?? ""}
                readOnly
                disabled
                className="bg-gray-50 text-gray-500 cursor-not-allowed"
              />
              <p className="text-xs text-gray-400">{t("emailReadOnly")}</p>
            </div>

            {/* Contacts */}
            <div className="space-y-1.5">
              <Label htmlFor="contacts">{t("contactsLabel")}</Label>
              <Input
                id="contacts"
                type="text"
                placeholder={t("contactsPlaceholder")}
                {...register("contacts")}
                className={errors.contacts ? "border-red-500" : ""}
              />
              {errors.contacts && (
                <p className="text-xs text-red-500">{errors.contacts.message}</p>
              )}
            </div>

            <Button type="submit" className="w-full mt-2" disabled={isSubmitting}>
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
      </div>
    </div>
  );
}
