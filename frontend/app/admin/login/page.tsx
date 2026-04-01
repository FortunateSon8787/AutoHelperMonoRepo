"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { Loader2, ShieldCheck } from "lucide-react";
import { useTranslations } from "next-intl";

import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { AppLogo } from "@/components/AppLogo";
import { LocaleSwitcher } from "@/components/LocaleSwitcher";
import {
  adminAuthService,
  AdminAuthServiceError,
} from "@/services/adminAuthService";

// ─── Page ─────────────────────────────────────────────────────────────────────

export default function AdminLoginPage() {
  const t = useTranslations("admin.login");
  const router = useRouter();
  const [serverError, setServerError] = useState<string | null>(null);

  const loginSchema = z.object({
    email: z
      .string()
      .min(1, t("errors.emailRequired"))
      .email(t("errors.emailInvalid")),
    password: z.string().min(1, t("errors.passwordRequired")),
  });

  type LoginFormValues = z.infer<typeof loginSchema>;

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<LoginFormValues>({
    resolver: zodResolver(loginSchema),
  });

  const onSubmit = async (values: LoginFormValues) => {
    setServerError(null);
    try {
      await adminAuthService.login(values.email, values.password);
      router.push("/admin/customers");
    } catch (error) {
      if (error instanceof AdminAuthServiceError) {
        setServerError(t(`errors.${error.code}` as Parameters<typeof t>[0]));
      } else {
        setServerError(t("errors.unknown"));
      }
    }
  };

  return (
    <div className="min-h-screen flex flex-col bg-background">
      {/* Top bar */}
      <div className="flex items-center justify-between px-6 py-4 border-b border-border bg-card">
        <AppLogo />
        <LocaleSwitcher />
      </div>

      {/* Center content */}
      <div className="flex-1 flex items-center justify-center px-4 py-12">
        <div className="w-full max-w-md">
          <div className="bg-card border border-border rounded-2xl p-8 shadow-card">
            {/* Icon + Title */}
            <div className="flex items-center gap-3 mb-6">
              <div className="w-10 h-10 rounded-xl bg-primary/10 flex items-center justify-center flex-shrink-0">
                <ShieldCheck className="h-5 w-5 text-primary" />
              </div>
              <div>
                <h1 className="text-xl font-semibold text-foreground leading-tight">
                  {t("title")}
                </h1>
                <p className="text-sm text-muted-foreground">{t("subtitle")}</p>
              </div>
            </div>

            {/* Server error alert */}
            {serverError && (
              <div className="bg-destructive/5 border border-destructive/20 text-destructive rounded-xl px-4 py-3 text-sm mb-5">
                {serverError}
              </div>
            )}

            <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
              {/* Email */}
              <div className="space-y-1.5">
                <Label htmlFor="email">{t("emailLabel")}</Label>
                <Input
                  id="email"
                  type="email"
                  placeholder={t("emailPlaceholder")}
                  autoComplete="email"
                  {...register("email")}
                  className={
                    errors.email
                      ? "border-destructive focus-visible:ring-destructive"
                      : ""
                  }
                />
                {errors.email && (
                  <p className="text-xs text-destructive">
                    {errors.email.message}
                  </p>
                )}
              </div>

              {/* Password */}
              <div className="space-y-1.5">
                <Label htmlFor="password">{t("passwordLabel")}</Label>
                <Input
                  id="password"
                  type="password"
                  placeholder={t("passwordPlaceholder")}
                  autoComplete="current-password"
                  {...register("password")}
                  className={
                    errors.password
                      ? "border-destructive focus-visible:ring-destructive"
                      : ""
                  }
                />
                {errors.password && (
                  <p className="text-xs text-destructive">
                    {errors.password.message}
                  </p>
                )}
              </div>

              <Button
                type="submit"
                className="w-full mt-2"
                size="lg"
                disabled={isSubmitting}
              >
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
    </div>
  );
}
