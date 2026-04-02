"use client";

import { useEffect, useState } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { Loader2, LogOut, User } from "lucide-react";
import { useRouter } from "next/navigation";
import { useTranslations } from "next-intl";

import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { AppHeader } from "@/components/AppHeader";
import { authService } from "@/services/authService";
import { profileService, ProfileServiceError } from "@/services/profileService";
import type { ClientProfile } from "@/types/client";

// ─── Page ─────────────────────────────────────────────────────────────────────

export default function ProfilePage() {
  const t = useTranslations("profile");
  const tValidation = useTranslations("profile.validation");
  const tErrors = useTranslations("profile.errors");
  const router = useRouter();
  const [profile, setProfile] = useState<ClientProfile | null>(null);
  const [loadError, setLoadError] = useState<string | null>(null);
  const [serverError, setServerError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [isLoggingOut, setIsLoggingOut] = useState(false);

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
    let cancelled = false;

    profileService
      .getMyProfile()
      .then((data) => {
        if (cancelled) return;
        setProfile(data);
        reset({ name: data.name, contacts: data.contacts ?? "" });
      })
      .catch((err: unknown) => {
        if (cancelled) return;
        if (err instanceof ProfileServiceError) {
          setLoadError(tErrors(err.code));
        } else {
          setLoadError(tErrors("unknown"));
        }
      })
      .finally(() => {
        if (!cancelled) setIsLoading(false);
      });

    return () => {
      cancelled = true;
    };
  }, [reset]);

  const handleLogout = async () => {
    setIsLoggingOut(true);
    try {
      await authService.logout();
    } catch {
      // logout best-effort — redirect regardless
    }
    router.push("/auth/login");
  };

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
        <Button
          variant="ghost"
          size="sm"
          onClick={handleLogout}
          disabled={isLoggingOut}
          className="text-muted-foreground hover:text-foreground"
        >
          {isLoggingOut ? (
            <Loader2 className="h-4 w-4 animate-spin" />
          ) : (
            <LogOut className="h-4 w-4" />
          )}
          <span className="ml-1">{t("logoutButton")}</span>
        </Button>
      </AppHeader>

      <div className="max-w-lg mx-auto px-4 py-10">
        <div className="bg-card border border-border rounded-2xl p-8 shadow-card">
          {/* Title */}
          <div className="flex items-center gap-3 mb-6">
            <div className="w-11 h-11 rounded-xl bg-primary/10 flex items-center justify-center">
              <User className="h-5 w-5 text-primary" />
            </div>
            <div>
              <h1 className="text-xl font-semibold text-foreground">{t("title")}</h1>
              <p className="text-sm text-muted-foreground">{profile?.email}</p>
            </div>
          </div>

          {/* Meta info */}
          <div className="flex items-center gap-4 mb-6 pb-6 border-b border-border text-xs text-muted-foreground">
            <span>
              {t("subscription")}:{" "}
              <span className="text-foreground font-medium">{profile?.subscriptionStatus}</span>
            </span>
            <span>
              {t("provider")}:{" "}
              <span className="text-foreground font-medium">{profile?.authProvider}</span>
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
                className={errors.name ? "border-destructive focus-visible:ring-destructive" : ""}
              />
              {errors.name && (
                <p className="text-xs text-destructive">{errors.name.message}</p>
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
              />
              <p className="text-xs text-muted-foreground">{t("emailReadOnly")}</p>
            </div>

            {/* Contacts */}
            <div className="space-y-1.5">
              <Label htmlFor="contacts">{t("contactsLabel")}</Label>
              <Input
                id="contacts"
                type="text"
                placeholder={t("contactsPlaceholder")}
                {...register("contacts")}
                className={errors.contacts ? "border-destructive focus-visible:ring-destructive" : ""}
              />
              {errors.contacts && (
                <p className="text-xs text-destructive">{errors.contacts.message}</p>
              )}
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
      </div>
    </div>
  );
}
