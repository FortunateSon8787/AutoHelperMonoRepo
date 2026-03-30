"use client";

import { useEffect, useState } from "react";
import { useTranslations } from "next-intl";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import Link from "next/link";
import { Loader2, ArrowLeft, Plus, X } from "lucide-react";
import {
  adCampaignService,
  AdCampaignServiceError,
} from "@/services/adCampaignService";
import type { AdCampaign } from "@/types/adCampaign";
import { AD_TYPES, TARGET_CATEGORIES } from "@/types/adCampaign";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { AppHeader } from "@/components/AppHeader";
import { nativeSelectCn, nativeTextareaCn } from "@/lib/form-styles";
import { cn } from "@/lib/utils";

// ─── Form schema ──────────────────────────────────────────────────────────────

function buildSchema(t: ReturnType<typeof useTranslations<"adCampaigns">>) {
  return z
    .object({
      type: z.string().min(1, t("validation.typeRequired")),
      targetCategory: z.string().min(1, t("validation.targetCategoryRequired")),
      content: z
        .string()
        .min(1, t("validation.contentRequired"))
        .max(2048, t("validation.contentMaxLength")),
      startsAt: z.string().min(1, t("validation.startsAtRequired")),
      endsAt: z.string().min(1, t("validation.endsAtRequired")),
      showToAnonymous: z.boolean(),
    })
    .refine((data) => new Date(data.endsAt) > new Date(data.startsAt), {
      message: t("validation.endsAtAfterStartsAt"),
      path: ["endsAt"],
    });
}

type FormValues = {
  type: string;
  targetCategory: string;
  content: string;
  startsAt: string;
  endsAt: string;
  showToAnonymous: boolean;
};

// ─── Component ────────────────────────────────────────────────────────────────

export default function AdCampaignsPage() {
  const t = useTranslations("adCampaigns");
  const tErrors = useTranslations("adCampaigns.errors");
  const tCommon = useTranslations("common");

  const [campaigns, setCampaigns] = useState<AdCampaign[]>([]);
  const [loadError, setLoadError] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  const [isFormOpen, setIsFormOpen] = useState(false);
  const [editingId, setEditingId] = useState<string | null>(null);
  const [isSaving, setIsSaving] = useState(false);
  const [formError, setFormError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);

  const schema = buildSchema(t);
  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: { showToAnonymous: false },
  });

  // ─── Load campaigns ───────────────────────────────────────────────────────

  useEffect(() => {
    adCampaignService
      .getMyCampaigns()
      .then(setCampaigns)
      .catch((err) => {
        if (err instanceof AdCampaignServiceError) {
          setLoadError(tErrors(err.code));
        } else {
          setLoadError(tErrors("unknown"));
        }
      })
      .finally(() => setIsLoading(false));
  }, [tErrors]);

  // ─── Open form ────────────────────────────────────────────────────────────

  function openCreate() {
    setEditingId(null);
    reset({ showToAnonymous: false, type: "", targetCategory: "", content: "", startsAt: "", endsAt: "" });
    setFormError(null);
    setSuccessMessage(null);
    setIsFormOpen(true);
  }

  function openEdit(campaign: AdCampaign) {
    setEditingId(campaign.id);
    reset({
      type: campaign.type,
      targetCategory: campaign.targetCategory,
      content: campaign.content,
      startsAt: campaign.startsAt.substring(0, 16),
      endsAt: campaign.endsAt.substring(0, 16),
      showToAnonymous: campaign.showToAnonymous,
    });
    setFormError(null);
    setSuccessMessage(null);
    setIsFormOpen(true);
  }

  // ─── Submit ───────────────────────────────────────────────────────────────

  const onSubmit = async (values: FormValues) => {
    setIsSaving(true);
    setFormError(null);
    setSuccessMessage(null);

    try {
      const payload = {
        ...values,
        startsAt: new Date(values.startsAt).toISOString(),
        endsAt: new Date(values.endsAt).toISOString(),
      };

      if (editingId) {
        await adCampaignService.updateCampaign(editingId, payload);
        setSuccessMessage(t("updateSuccess"));
        setCampaigns((prev) =>
          prev.map((c) => (c.id === editingId ? { ...c, ...payload } : c))
        );
      } else {
        const { campaignId } = await adCampaignService.createCampaign(payload);
        setSuccessMessage(t("createSuccess"));
        const newCampaign: AdCampaign = {
          id: campaignId,
          partnerId: "",
          ...payload,
          isActive: false,
          statsImpressions: 0,
          statsClicks: 0,
        };
        setCampaigns((prev) => [newCampaign, ...prev]);
      }

      setIsFormOpen(false);
    } catch (err) {
      if (err instanceof AdCampaignServiceError) {
        setFormError(tErrors(err.code));
      } else {
        setFormError(tErrors("unknown"));
      }
    } finally {
      setIsSaving(false);
    }
  };

  // ─── Delete ───────────────────────────────────────────────────────────────

  const handleDelete = async (id: string) => {
    if (!window.confirm(t("deleteConfirm"))) return;

    try {
      await adCampaignService.deleteCampaign(id);
      setCampaigns((prev) => prev.filter((c) => c.id !== id));
      setSuccessMessage(t("deleteSuccess"));
    } catch (err) {
      if (err instanceof AdCampaignServiceError) {
        setLoadError(tErrors(err.code));
      }
    }
  };

  // ─── Render ───────────────────────────────────────────────────────────────

  return (
    <div className="min-h-screen bg-background">
      <AppHeader />

      <div className="max-w-4xl mx-auto px-4 py-10">
        {/* Page header */}
        <div className="flex items-center justify-between mb-6">
          <div>
            <Link
              href="/partner/cabinet"
              className="inline-flex items-center gap-1 text-sm text-muted-foreground hover:text-foreground mb-1 transition-colors"
            >
              <ArrowLeft className="h-3.5 w-3.5" />
              {t("backToPartnerCabinet")}
            </Link>
            <h1 className="text-xl font-semibold text-foreground">{t("title")}</h1>
            <p className="text-sm text-muted-foreground mt-0.5">{t("subtitle")}</p>
          </div>
          <Button onClick={openCreate}>
            <Plus className="h-4 w-4" />
            {t("createButton")}
          </Button>
        </div>

        {/* Success message */}
        {successMessage && (
          <div className="bg-success/5 border border-success/20 text-success rounded-xl px-4 py-3 text-sm mb-5">
            {successMessage}
          </div>
        )}

        {/* Load error */}
        {loadError && (
          <div className="bg-destructive/5 border border-destructive/20 text-destructive rounded-xl px-4 py-3 text-sm mb-5">
            {loadError}
          </div>
        )}

        {/* Loading */}
        {isLoading && (
          <div className="flex items-center justify-center py-12 text-muted-foreground gap-2">
            <Loader2 className="h-5 w-5 animate-spin" />
            {tCommon("loading")}
          </div>
        )}

        {/* Empty state */}
        {!isLoading && !loadError && campaigns.length === 0 && (
          <div className="bg-card border border-border rounded-2xl p-10 text-center text-sm text-muted-foreground shadow-card">
            {t("emptyState")}
          </div>
        )}

        {/* Campaigns list */}
        {!isLoading && campaigns.length > 0 && (
          <div className="space-y-3">
            {campaigns.map((campaign) => (
              <div
                key={campaign.id}
                className="bg-card border border-border rounded-2xl p-5 shadow-card hover:shadow-card-hover transition-shadow"
              >
                <div className="flex items-start justify-between gap-4">
                  <div className="flex-1 min-w-0">
                    <div className="flex items-center gap-2 mb-1">
                      <span
                        className={cn(
                          "inline-block px-2 py-0.5 rounded-lg text-xs font-medium",
                          campaign.isActive
                            ? "bg-success/10 text-success"
                            : "bg-secondary text-muted-foreground"
                        )}
                      >
                        {campaign.isActive ? t("activeLabel") : t("inactiveLabel")}
                      </span>
                      <span className="text-xs text-muted-foreground">
                        {t(`types.${campaign.type as "OfferBlock" | "Banner"}`)}
                      </span>
                      <span className="text-xs text-muted-foreground">·</span>
                      <span className="text-xs text-muted-foreground">
                        {t(`categories.${campaign.targetCategory as "AutoService" | "CarWash" | "Towing" | "AutoShop" | "Other"}`)}
                      </span>
                    </div>

                    <p className="text-sm text-foreground truncate">{campaign.content}</p>

                    <div className="mt-2 flex flex-wrap gap-3 text-xs text-muted-foreground">
                      <span>
                        {t("startsAtLabel")}: {new Date(campaign.startsAt).toLocaleDateString()}
                      </span>
                      <span>
                        {t("endsAtLabel")}: {new Date(campaign.endsAt).toLocaleDateString()}
                      </span>
                      <span>
                        {t("impressionsLabel")}: {campaign.statsImpressions}
                      </span>
                      <span>
                        {t("clicksLabel")}: {campaign.statsClicks}
                      </span>
                    </div>
                  </div>

                  <div className="flex gap-2 flex-shrink-0">
                    <Button variant="outline" size="sm" onClick={() => openEdit(campaign)}>
                      {t("editButton")}
                    </Button>
                    <Button variant="ghost" size="sm" onClick={() => handleDelete(campaign.id)} className="text-destructive hover:text-destructive hover:bg-destructive/5">
                      {t("deleteButton")}
                    </Button>
                  </div>
                </div>
              </div>
            ))}
          </div>
        )}

        {/* Create/Edit modal */}
        {isFormOpen && (
          <div className="fixed inset-0 bg-foreground/50 flex items-center justify-center z-50 p-4">
            <div className="bg-card border border-border rounded-2xl w-full max-w-lg shadow-2xl">
              <div className="p-6">
                <div className="flex items-center justify-between mb-4">
                  <h2 className="text-lg font-semibold text-foreground">
                    {editingId ? t("editButton") : t("createButton")}
                  </h2>
                  <button
                    type="button"
                    onClick={() => setIsFormOpen(false)}
                    className="text-muted-foreground hover:text-foreground transition-colors"
                  >
                    <X className="h-5 w-5" />
                  </button>
                </div>

                {formError && (
                  <div className="bg-destructive/5 border border-destructive/20 text-destructive rounded-xl px-4 py-3 text-sm mb-4">
                    {formError}
                  </div>
                )}

                <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
                  {/* Type */}
                  <div className="space-y-1.5">
                    <Label htmlFor="type">{t("typeLabel")}</Label>
                    <select
                      id="type"
                      {...register("type")}
                      className={errors.type ? `${nativeSelectCn} border-destructive` : nativeSelectCn}
                    >
                      <option value="">—</option>
                      {AD_TYPES.map((type) => (
                        <option key={type} value={type}>
                          {t(`types.${type}`)}
                        </option>
                      ))}
                    </select>
                    {errors.type && (
                      <p className="text-xs text-destructive">{errors.type.message}</p>
                    )}
                  </div>

                  {/* Target category */}
                  <div className="space-y-1.5">
                    <Label htmlFor="targetCategory">{t("targetCategoryLabel")}</Label>
                    <select
                      id="targetCategory"
                      {...register("targetCategory")}
                      className={errors.targetCategory ? `${nativeSelectCn} border-destructive` : nativeSelectCn}
                    >
                      <option value="">—</option>
                      {TARGET_CATEGORIES.map((cat) => (
                        <option key={cat} value={cat}>
                          {t(`categories.${cat}`)}
                        </option>
                      ))}
                    </select>
                    {errors.targetCategory && (
                      <p className="text-xs text-destructive">{errors.targetCategory.message}</p>
                    )}
                  </div>

                  {/* Content */}
                  <div className="space-y-1.5">
                    <Label htmlFor="content">{t("contentLabel")}</Label>
                    <textarea
                      id="content"
                      {...register("content")}
                      rows={3}
                      className={errors.content ? `${nativeTextareaCn} border-destructive` : nativeTextareaCn}
                    />
                    {errors.content && (
                      <p className="text-xs text-destructive">{errors.content.message}</p>
                    )}
                  </div>

                  {/* Dates */}
                  <div className="grid grid-cols-2 gap-3">
                    <div className="space-y-1.5">
                      <Label htmlFor="startsAt">{t("startsAtLabel")}</Label>
                      <Input
                        id="startsAt"
                        type="datetime-local"
                        {...register("startsAt")}
                        className={errors.startsAt ? "border-destructive" : ""}
                      />
                      {errors.startsAt && (
                        <p className="text-xs text-destructive">{errors.startsAt.message}</p>
                      )}
                    </div>
                    <div className="space-y-1.5">
                      <Label htmlFor="endsAt">{t("endsAtLabel")}</Label>
                      <Input
                        id="endsAt"
                        type="datetime-local"
                        {...register("endsAt")}
                        className={errors.endsAt ? "border-destructive" : ""}
                      />
                      {errors.endsAt && (
                        <p className="text-xs text-destructive">{errors.endsAt.message}</p>
                      )}
                    </div>
                  </div>

                  {/* Show to anonymous */}
                  <div className="flex items-center gap-2">
                    <input
                      type="checkbox"
                      id="showToAnonymous"
                      {...register("showToAnonymous")}
                      className="h-4 w-4 rounded border-border text-accent focus:ring-ring"
                    />
                    <Label htmlFor="showToAnonymous" className="font-normal cursor-pointer">
                      {t("showToAnonymousLabel")}
                    </Label>
                  </div>

                  {/* Actions */}
                  <div className="flex gap-3 pt-2">
                    <Button type="submit" disabled={isSaving} className="flex-1" size="lg">
                      {isSaving ? (
                        <>
                          <Loader2 className="h-4 w-4 animate-spin" />
                          {t("savingButton")}
                        </>
                      ) : (
                        t("saveButton")
                      )}
                    </Button>
                    <Button
                      type="button"
                      variant="outline"
                      size="lg"
                      className="flex-1"
                      onClick={() => setIsFormOpen(false)}
                    >
                      {t("cancelButton")}
                    </Button>
                  </div>
                </form>
              </div>
            </div>
          </div>
        )}
      </div>
    </div>
  );
}
