"use client";

import { useEffect, useState } from "react";
import { useTranslations } from "next-intl";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import {
  adCampaignService,
  AdCampaignServiceError,
} from "@/services/adCampaignService";
import type { AdCampaign } from "@/types/adCampaign";
import { AD_TYPES, TARGET_CATEGORIES } from "@/types/adCampaign";
import Link from "next/link";

// ─── Form schema ──────────────────────────────────────────────────────────────

function buildSchema(t: ReturnType<typeof useTranslations<"adCampaigns">>) {
  return z
    .object({
      type: z.string().min(1, t("validation.typeRequired")),
      targetCategory: z
        .string()
        .min(1, t("validation.targetCategoryRequired")),
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
          prev.map((c) =>
            c.id === editingId ? { ...c, ...payload } : c
          )
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
    <main className="min-h-screen bg-gray-50 py-8 px-4">
      <div className="max-w-4xl mx-auto">
        {/* Header */}
        <div className="flex items-center justify-between mb-6">
          <div>
            <Link
              href="/partner/cabinet"
              className="text-sm text-blue-600 hover:underline mb-1 block"
            >
              ← Партнёрский кабинет
            </Link>
            <h1 className="text-2xl font-bold text-gray-900">{t("title")}</h1>
            <p className="text-gray-500 text-sm mt-1">{t("subtitle")}</p>
          </div>
          <button
            onClick={openCreate}
            className="bg-blue-600 text-white px-4 py-2 rounded-lg hover:bg-blue-700 transition-colors text-sm font-medium"
          >
            + {t("createButton")}
          </button>
        </div>

        {/* Success message */}
        {successMessage && (
          <div className="mb-4 p-3 bg-green-50 text-green-700 rounded-lg text-sm">
            {successMessage}
          </div>
        )}

        {/* Load error */}
        {loadError && (
          <div className="mb-4 p-3 bg-red-50 text-red-600 rounded-lg text-sm">
            {loadError}
          </div>
        )}

        {/* Loading */}
        {isLoading && (
          <div className="text-center py-12 text-gray-500">
            {tCommon("loading")}
          </div>
        )}

        {/* Empty state */}
        {!isLoading && !loadError && campaigns.length === 0 && (
          <div className="text-center py-12 text-gray-500 bg-white rounded-xl border border-gray-200">
            {t("emptyState")}
          </div>
        )}

        {/* Campaigns list */}
        {!isLoading && campaigns.length > 0 && (
          <div className="space-y-4">
            {campaigns.map((campaign) => (
              <div
                key={campaign.id}
                className="bg-white rounded-xl border border-gray-200 p-4 shadow-sm"
              >
                <div className="flex items-start justify-between gap-4">
                  <div className="flex-1 min-w-0">
                    <div className="flex items-center gap-2 mb-1">
                      <span
                        className={`inline-block px-2 py-0.5 rounded text-xs font-medium ${
                          campaign.isActive
                            ? "bg-green-100 text-green-700"
                            : "bg-gray-100 text-gray-600"
                        }`}
                      >
                        {campaign.isActive ? t("activeLabel") : t("inactiveLabel")}
                      </span>
                      <span className="text-xs text-gray-500">
                        {t(`types.${campaign.type as "OfferBlock" | "Banner"}`)}
                      </span>
                      <span className="text-xs text-gray-400">·</span>
                      <span className="text-xs text-gray-500">
                        {t(`categories.${campaign.targetCategory as "AutoService" | "CarWash" | "Towing" | "AutoShop" | "Other"}`)}
                      </span>
                    </div>

                    <p className="text-sm text-gray-800 truncate">{campaign.content}</p>

                    <div className="mt-2 flex flex-wrap gap-3 text-xs text-gray-500">
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
                    <button
                      onClick={() => openEdit(campaign)}
                      className="text-sm text-blue-600 hover:underline"
                    >
                      {t("editButton")}
                    </button>
                    <button
                      onClick={() => handleDelete(campaign.id)}
                      className="text-sm text-red-500 hover:underline"
                    >
                      {t("deleteButton")}
                    </button>
                  </div>
                </div>
              </div>
            ))}
          </div>
        )}

        {/* Create/Edit modal */}
        {isFormOpen && (
          <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
            <div className="bg-white rounded-xl w-full max-w-lg shadow-xl">
              <div className="p-6">
                <h2 className="text-lg font-semibold text-gray-900 mb-4">
                  {editingId ? t("editButton") : t("createButton")}
                </h2>

                {formError && (
                  <div className="mb-4 p-3 bg-red-50 text-red-600 rounded-lg text-sm">
                    {formError}
                  </div>
                )}

                <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
                  {/* Type */}
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      {t("typeLabel")}
                    </label>
                    <select
                      {...register("type")}
                      className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                    >
                      <option value="">—</option>
                      {AD_TYPES.map((type) => (
                        <option key={type} value={type}>
                          {t(`types.${type}`)}
                        </option>
                      ))}
                    </select>
                    {errors.type && (
                      <p className="mt-1 text-xs text-red-500">{errors.type.message}</p>
                    )}
                  </div>

                  {/* Target category */}
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      {t("targetCategoryLabel")}
                    </label>
                    <select
                      {...register("targetCategory")}
                      className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                    >
                      <option value="">—</option>
                      {TARGET_CATEGORIES.map((cat) => (
                        <option key={cat} value={cat}>
                          {t(`categories.${cat}`)}
                        </option>
                      ))}
                    </select>
                    {errors.targetCategory && (
                      <p className="mt-1 text-xs text-red-500">{errors.targetCategory.message}</p>
                    )}
                  </div>

                  {/* Content */}
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      {t("contentLabel")}
                    </label>
                    <textarea
                      {...register("content")}
                      rows={3}
                      className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                    />
                    {errors.content && (
                      <p className="mt-1 text-xs text-red-500">{errors.content.message}</p>
                    )}
                  </div>

                  {/* Dates */}
                  <div className="grid grid-cols-2 gap-3">
                    <div>
                      <label className="block text-sm font-medium text-gray-700 mb-1">
                        {t("startsAtLabel")}
                      </label>
                      <input
                        type="datetime-local"
                        {...register("startsAt")}
                        className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                      />
                      {errors.startsAt && (
                        <p className="mt-1 text-xs text-red-500">{errors.startsAt.message}</p>
                      )}
                    </div>
                    <div>
                      <label className="block text-sm font-medium text-gray-700 mb-1">
                        {t("endsAtLabel")}
                      </label>
                      <input
                        type="datetime-local"
                        {...register("endsAt")}
                        className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                      />
                      {errors.endsAt && (
                        <p className="mt-1 text-xs text-red-500">{errors.endsAt.message}</p>
                      )}
                    </div>
                  </div>

                  {/* Show to anonymous */}
                  <div className="flex items-center gap-2">
                    <input
                      type="checkbox"
                      id="showToAnonymous"
                      {...register("showToAnonymous")}
                      className="h-4 w-4 rounded border-gray-300 text-blue-600 focus:ring-blue-500"
                    />
                    <label htmlFor="showToAnonymous" className="text-sm text-gray-700">
                      {t("showToAnonymousLabel")}
                    </label>
                  </div>

                  {/* Actions */}
                  <div className="flex gap-3 pt-2">
                    <button
                      type="submit"
                      disabled={isSaving}
                      className="flex-1 bg-blue-600 text-white py-2 rounded-lg text-sm font-medium hover:bg-blue-700 disabled:opacity-50 transition-colors"
                    >
                      {isSaving ? t("savingButton") : t("saveButton")}
                    </button>
                    <button
                      type="button"
                      onClick={() => setIsFormOpen(false)}
                      className="flex-1 border border-gray-300 text-gray-700 py-2 rounded-lg text-sm font-medium hover:bg-gray-50 transition-colors"
                    >
                      {t("cancelButton")}
                    </button>
                  </div>
                </form>
              </div>
            </div>
          </div>
        )}
      </div>
    </main>
  );
}
