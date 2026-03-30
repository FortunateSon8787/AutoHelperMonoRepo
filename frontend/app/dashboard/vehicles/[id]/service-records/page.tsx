"use client";

import { useEffect, useRef, useState } from "react";
import Link from "next/link";
import { useParams } from "next/navigation";
import { Loader2, ArrowLeft, Plus, FileText, X } from "lucide-react";
import { useTranslations } from "next-intl";

import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { AppHeader } from "@/components/AppHeader";
import { nativeTextareaCn } from "@/lib/form-styles";
import { cn } from "@/lib/utils";
import {
  serviceRecordService,
  ServiceRecordServiceError,
} from "@/services/serviceRecordService";
import type { ServiceRecord } from "@/types/serviceRecord";

export default function ServiceRecordsPage() {
  const t = useTranslations("serviceRecords.list");
  const tf = useTranslations("serviceRecords.form");
  const params = useParams<{ id: string }>();

  const [records, setRecords] = useState<ServiceRecord[]>([]);
  const [loadError, setLoadError] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [showForm, setShowForm] = useState(false);

  // ─── Form state ───────────────────────────────────────────────────────────
  const [formError, setFormError] = useState<string | null>(null);
  const [formSuccess, setFormSuccess] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [isUploading, setIsUploading] = useState(false);
  const [uploadedDocumentUrl, setUploadedDocumentUrl] = useState<string | null>(null);
  const [documentError, setDocumentError] = useState<string | null>(null);
  const fileInputRef = useRef<HTMLInputElement>(null);

  const [operations, setOperations] = useState<string[]>([""]);

  const [formValues, setFormValues] = useState({
    title: "",
    description: "",
    performedAt: "",
    cost: "",
    executorName: "",
    executorContacts: "",
  });

  // ─── Load records ─────────────────────────────────────────────────────────

  useEffect(() => {
    if (!params.id) return;

    serviceRecordService
      .getByVehicleId(params.id)
      .then(setRecords)
      .catch((err: unknown) => {
        setLoadError(
          err instanceof ServiceRecordServiceError
            ? tf(`errors.${err.code}`)
            : tf("errors.unknown")
        );
      })
      .finally(() => setIsLoading(false));
  }, [params.id, tf]);

  // ─── Handle PDF upload ────────────────────────────────────────────────────

  const handleFileChange = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;

    setDocumentError(null);
    setUploadedDocumentUrl(null);

    if (file.type !== "application/pdf") {
      setDocumentError(tf("validation.documentNotPdf"));
      return;
    }
    if (file.size > 10 * 1024 * 1024) {
      setDocumentError(tf("validation.documentTooLarge"));
      return;
    }

    setIsUploading(true);
    try {
      const url = await serviceRecordService.uploadDocument(file);
      setUploadedDocumentUrl(url);
    } catch {
      setDocumentError(tf("errors.serverError"));
    } finally {
      setIsUploading(false);
    }
  };

  // ─── Handle form submit ───────────────────────────────────────────────────

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setFormError(null);
    setFormSuccess(null);

    const validOps = operations.filter((op) => op.trim().length > 0);

    if (!formValues.title.trim()) { setFormError(tf("validation.titleRequired")); return; }
    if (!formValues.description.trim()) { setFormError(tf("validation.descriptionRequired")); return; }
    if (!formValues.performedAt) { setFormError(tf("validation.performedAtRequired")); return; }
    if (new Date(formValues.performedAt) > new Date()) { setFormError(tf("validation.performedAtFuture")); return; }
    if (Number(formValues.cost) < 0) { setFormError(tf("validation.costInvalid")); return; }
    if (!formValues.executorName.trim()) { setFormError(tf("validation.executorNameRequired")); return; }
    if (validOps.length === 0) { setFormError(tf("validation.operationsRequired")); return; }
    if (!uploadedDocumentUrl) { setFormError(tf("validation.documentRequired")); return; }

    setIsSubmitting(true);
    try {
      const created = await serviceRecordService.create(params.id, {
        title: formValues.title.trim(),
        description: formValues.description.trim(),
        performedAt: new Date(formValues.performedAt).toISOString(),
        cost: Number(formValues.cost),
        executorName: formValues.executorName.trim(),
        executorContacts: formValues.executorContacts.trim() || null,
        operations: validOps,
        documentUrl: uploadedDocumentUrl,
      });

      const newRecord = await serviceRecordService.getById(created.serviceRecordId);
      setRecords((prev) => [newRecord, ...prev]);

      setFormSuccess(tf("createSuccess"));
      setShowForm(false);
      resetForm();
    } catch (err) {
      const code = err instanceof ServiceRecordServiceError ? err.code : "unknown";
      setFormError(tf(`errors.${code}`));
    } finally {
      setIsSubmitting(false);
    }
  };

  const resetForm = () => {
    setFormValues({ title: "", description: "", performedAt: "", cost: "", executorName: "", executorContacts: "" });
    setOperations([""]);
    setUploadedDocumentUrl(null);
    setDocumentError(null);
    if (fileInputRef.current) fileInputRef.current.value = "";
    setFormError(null);
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
      <AppHeader />

      <div className="max-w-2xl mx-auto px-4 py-10">
        {/* Back link */}
        <Link
          href={`/dashboard/vehicles/${params.id}`}
          className="inline-flex items-center gap-1.5 text-sm text-muted-foreground hover:text-foreground mb-6 transition-colors"
        >
          <ArrowLeft className="h-4 w-4" />
          {tf("cancelButton")}
        </Link>

        {/* Title + Add button */}
        <div className="flex items-center justify-between mb-6">
          <h1 className="text-xl font-semibold text-foreground">{t("title")}</h1>
          {!showForm && (
            <Button
              size="sm"
              onClick={() => {
                setShowForm(true);
                setFormSuccess(null);
              }}
            >
              <Plus className="h-4 w-4" />
              {t("addButton")}
            </Button>
          )}
        </div>

        {formSuccess && (
          <div className="bg-success/5 border border-success/20 text-success rounded-xl px-4 py-3 text-sm mb-5">
            {formSuccess}
          </div>
        )}

        {/* ─── Add record form ───────────────────────────────────────────── */}
        {showForm && (
          <div className="bg-card border border-border rounded-2xl p-6 shadow-card mb-6">
            <div className="flex items-center justify-between mb-4">
              <h2 className="text-base font-semibold text-foreground">{tf("addTitle")}</h2>
              <button
                type="button"
                onClick={() => { setShowForm(false); resetForm(); }}
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

            <form onSubmit={handleSubmit} className="space-y-4">
              <div className="space-y-1.5">
                <Label htmlFor="title">{tf("titleLabel")}</Label>
                <Input
                  id="title"
                  placeholder={tf("titlePlaceholder")}
                  value={formValues.title}
                  onChange={(e) => setFormValues((v) => ({ ...v, title: e.target.value }))}
                />
              </div>

              <div className="space-y-1.5">
                <Label htmlFor="description">{tf("descriptionLabel")}</Label>
                <textarea
                  id="description"
                  rows={3}
                  placeholder={tf("descriptionPlaceholder")}
                  value={formValues.description}
                  onChange={(e) => setFormValues((v) => ({ ...v, description: e.target.value }))}
                  className={nativeTextareaCn}
                />
              </div>

              <div className="grid grid-cols-2 gap-4">
                <div className="space-y-1.5">
                  <Label htmlFor="performedAt">{tf("performedAtLabel")}</Label>
                  <Input
                    id="performedAt"
                    type="date"
                    value={formValues.performedAt}
                    max={new Date().toISOString().split("T")[0]}
                    onChange={(e) => setFormValues((v) => ({ ...v, performedAt: e.target.value }))}
                  />
                </div>

                <div className="space-y-1.5">
                  <Label htmlFor="cost">{tf("costLabel")}</Label>
                  <Input
                    id="cost"
                    type="number"
                    min={0}
                    step={0.01}
                    placeholder={tf("costPlaceholder")}
                    value={formValues.cost}
                    onChange={(e) => setFormValues((v) => ({ ...v, cost: e.target.value }))}
                  />
                </div>
              </div>

              <div className="grid grid-cols-2 gap-4">
                <div className="space-y-1.5">
                  <Label htmlFor="executorName">{tf("executorNameLabel")}</Label>
                  <Input
                    id="executorName"
                    placeholder={tf("executorNamePlaceholder")}
                    value={formValues.executorName}
                    onChange={(e) => setFormValues((v) => ({ ...v, executorName: e.target.value }))}
                  />
                </div>

                <div className="space-y-1.5">
                  <Label htmlFor="executorContacts">{tf("executorContactsLabel")}</Label>
                  <Input
                    id="executorContacts"
                    placeholder={tf("executorContactsPlaceholder")}
                    value={formValues.executorContacts}
                    onChange={(e) => setFormValues((v) => ({ ...v, executorContacts: e.target.value }))}
                  />
                </div>
              </div>

              {/* Operations list */}
              <div className="space-y-2">
                <Label>{tf("operationsLabel")}</Label>
                {operations.map((op, idx) => (
                  <div key={idx} className="flex gap-2">
                    <Input
                      placeholder={tf("operationsPlaceholder")}
                      value={op}
                      onChange={(e) => {
                        const updated = [...operations];
                        updated[idx] = e.target.value;
                        setOperations(updated);
                      }}
                    />
                    {operations.length > 1 && (
                      <button
                        type="button"
                        onClick={() => setOperations(operations.filter((_, i) => i !== idx))}
                        className="text-muted-foreground hover:text-destructive transition-colors"
                      >
                        <X className="h-4 w-4" />
                      </button>
                    )}
                  </div>
                ))}
                <button
                  type="button"
                  onClick={() => setOperations([...operations, ""])}
                  className="text-sm text-accent hover:text-accent/80 flex items-center gap-1 transition-colors"
                >
                  <Plus className="h-3.5 w-3.5" />
                  {tf("addOperationButton")}
                </button>
              </div>

              {/* PDF upload */}
              <div className="space-y-1.5">
                <Label htmlFor="document">{tf("documentLabel")}</Label>
                <input
                  ref={fileInputRef}
                  id="document"
                  type="file"
                  accept="application/pdf"
                  onChange={handleFileChange}
                  className="block w-full text-sm text-muted-foreground file:mr-4 file:py-1.5 file:px-3 file:rounded-lg file:border-0 file:text-sm file:font-medium file:bg-secondary file:text-foreground hover:file:bg-muted cursor-pointer"
                />
                <p className="text-xs text-muted-foreground">{tf("documentHint")}</p>
                {isUploading && (
                  <p className="text-xs text-accent flex items-center gap-1">
                    <Loader2 className="h-3 w-3 animate-spin" />
                    {tf("documentUploading")}
                  </p>
                )}
                {uploadedDocumentUrl && !isUploading && (
                  <p className="text-xs text-success">{tf("documentUploaded")}</p>
                )}
                {documentError && (
                  <p className="text-xs text-destructive">{documentError}</p>
                )}
              </div>

              <div className="flex gap-3 pt-2">
                <Button type="submit" disabled={isSubmitting || isUploading} className="flex-1">
                  {isSubmitting ? (
                    <>
                      <Loader2 className="h-4 w-4 animate-spin" />
                      {tf("submittingButton")}
                    </>
                  ) : (
                    tf("submitButton")
                  )}
                </Button>
                <Button
                  type="button"
                  variant="outline"
                  onClick={() => { setShowForm(false); resetForm(); }}
                >
                  {tf("cancelButton")}
                </Button>
              </div>
            </form>
          </div>
        )}

        {/* ─── Records list ──────────────────────────────────────────────── */}
        {records.length === 0 ? (
          <div className="bg-card border border-border rounded-2xl p-8 shadow-card text-center text-sm text-muted-foreground">
            {t("emptyState")}
          </div>
        ) : (
          <div className="space-y-3">
            {records.map((record) => (
              <div
                key={record.id}
                className="bg-card border border-border rounded-2xl p-5 shadow-card hover:shadow-card-hover transition-shadow"
              >
                <div className="flex items-start justify-between gap-4">
                  <div className="flex-1 min-w-0">
                    <p className="font-semibold text-foreground truncate">{record.title}</p>
                    <p className="text-sm text-muted-foreground mt-0.5">
                      {t("performedAtLabel")}:{" "}
                      {new Date(record.performedAt).toLocaleDateString()}
                      {" · "}
                      {t("executorLabel")}: {record.executorName}
                    </p>
                    <p className="text-sm font-medium text-foreground mt-1">
                      {t("costLabel")}: {record.cost.toLocaleString()}{" "}
                      {t("currency")}
                    </p>
                  </div>
                  <Link
                    href={`/dashboard/vehicles/${params.id}/service-records/${record.id}`}
                    className="shrink-0"
                  >
                    <Button variant="outline" size="sm">
                      <FileText className="h-3.5 w-3.5" />
                      {t("viewButton")}
                    </Button>
                  </Link>
                </div>
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  );
}
