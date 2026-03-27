"use client";

import { useEffect, useState, useCallback } from "react";
import Link from "next/link";
import { useParams } from "next/navigation";
import { Loader2, ArrowLeft, FileText, Eye, Download, ExternalLink } from "lucide-react";
import { useTranslations } from "next-intl";

import { Button } from "@/components/ui/button";
import { PdfPreviewModal } from "@/components/pdf-preview-modal";
import {
  serviceRecordService,
  ServiceRecordServiceError,
} from "@/services/serviceRecordService";
import type { ServiceRecord } from "@/types/serviceRecord";

export default function ServiceRecordDetailPage() {
  const t = useTranslations("serviceRecords.detail");
  const params = useParams<{ id: string; recordId: string }>();

  const [record, setRecord] = useState<ServiceRecord | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [isPdfOpen, setIsPdfOpen] = useState(false);

  // Authenticated blob URL — created once after PDF is fetched with Bearer token
  const [pdfBlobUrl, setPdfBlobUrl] = useState<string | null>(null);
  const [isPdfLoading, setIsPdfLoading] = useState(false);

  useEffect(() => {
    if (!params.recordId) return;

    serviceRecordService
      .getById(params.recordId)
      .then(setRecord)
      .catch((err: unknown) => {
        setError(
          err instanceof ServiceRecordServiceError && err.code === "notFound"
            ? t("notFound")
            : t("serverError")
        );
      })
      .finally(() => setIsLoading(false));
  }, [params.recordId, t]);

  // Revoke blob URL on unmount to free memory
  useEffect(() => {
    return () => {
      if (pdfBlobUrl) URL.revokeObjectURL(pdfBlobUrl);
    };
  }, [pdfBlobUrl]);

  // Fetches the PDF with Authorization header and returns a blob URL.
  // Returns the existing blob URL immediately if already loaded.
  const ensurePdfLoaded = useCallback(async (): Promise<string | null> => {
    if (pdfBlobUrl) return pdfBlobUrl;
    if (isPdfLoading || !params.recordId) return null;

    setIsPdfLoading(true);
    try {
      const token = localStorage.getItem("accessToken");
      const proxyUrl = serviceRecordService.getDocumentProxyUrl(params.recordId);
      const response = await fetch(proxyUrl, {
        headers: token ? { Authorization: `Bearer ${token}` } : {},
      });

      if (!response.ok) throw new Error("Failed to load PDF");

      const blob = await response.blob();
      const url = URL.createObjectURL(blob);
      setPdfBlobUrl(url);
      return url;
    } catch {
      return null;
    } finally {
      setIsPdfLoading(false);
    }
  }, [pdfBlobUrl, isPdfLoading, params.recordId]);

  const handleOpenPreview = useCallback(async () => {
    const url = await ensurePdfLoaded();
    if (url) setIsPdfOpen(true);
  }, [ensurePdfLoaded]);

  const handleDownload = useCallback(async () => {
    const url = await ensurePdfLoaded();
    if (!url || !record) return;
    const a = document.createElement("a");
    a.href = url;
    a.download = `${record.title}.pdf`;
    a.click();
  }, [ensurePdfLoaded, record]);

  const handleOpenInTab = useCallback(async () => {
    const url = await ensurePdfLoaded();
    if (!url) return;
    window.open(url, "_blank", "noopener,noreferrer");
  }, [ensurePdfLoaded]);

  if (isLoading) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-gray-50">
        <Loader2 className="h-6 w-6 animate-spin text-gray-400" />
      </div>
    );
  }

  if (error || !record) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-gray-50 px-4">
        <div className="bg-red-50 border border-red-200 text-red-700 rounded-lg px-6 py-4 text-sm">
          {error ?? t("notFound")}
        </div>
      </div>
    );
  }

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

        {/* Back link */}
        <Link
          href={`/dashboard/vehicles/${params.id}/service-records`}
          className="inline-flex items-center gap-1.5 text-sm text-gray-500 hover:text-gray-800 mb-6 transition-colors"
        >
          <ArrowLeft className="h-4 w-4" />
          {t("backButton")}
        </Link>

        {/* Title */}
        <h1 className="text-2xl font-bold text-gray-900 mb-6">{record.title}</h1>

        <div className="bg-white border border-gray-200 rounded-xl p-6 shadow-sm space-y-5">
          {/* Description */}
          <div>
            <p className="text-xs font-medium text-gray-400 uppercase tracking-wide mb-1">
              {t("descriptionLabel")}
            </p>
            <p className="text-sm text-gray-700 whitespace-pre-wrap">{record.description}</p>
          </div>

          <hr className="border-gray-100" />

          {/* Date + Cost grid */}
          <div className="grid grid-cols-2 gap-4">
            <div>
              <p className="text-xs font-medium text-gray-400 uppercase tracking-wide mb-1">
                {t("performedAtLabel")}
              </p>
              <p className="text-sm text-gray-700">
                {new Date(record.performedAt).toLocaleDateString()}
              </p>
            </div>
            <div>
              <p className="text-xs font-medium text-gray-400 uppercase tracking-wide mb-1">
                {t("costLabel")}
              </p>
              <p className="text-sm font-medium text-gray-700">
                {record.cost.toLocaleString()}
              </p>
            </div>
          </div>

          <hr className="border-gray-100" />

          {/* Executor */}
          <div className="grid grid-cols-2 gap-4">
            <div>
              <p className="text-xs font-medium text-gray-400 uppercase tracking-wide mb-1">
                {t("executorLabel")}
              </p>
              <p className="text-sm text-gray-700">{record.executorName}</p>
            </div>
            {record.executorContacts && (
              <div>
                <p className="text-xs font-medium text-gray-400 uppercase tracking-wide mb-1">
                  {t("executorContactsLabel")}
                </p>
                <p className="text-sm text-gray-700">{record.executorContacts}</p>
              </div>
            )}
          </div>

          <hr className="border-gray-100" />

          {/* Operations */}
          <div>
            <p className="text-xs font-medium text-gray-400 uppercase tracking-wide mb-2">
              {t("operationsLabel")}
            </p>
            <ul className="space-y-1">
              {record.operations.map((op, idx) => (
                <li key={idx} className="text-sm text-gray-700 flex items-start gap-2">
                  <span className="text-gray-300 mt-0.5">•</span>
                  {op}
                </li>
              ))}
            </ul>
          </div>

          <hr className="border-gray-100" />

          {/* Document */}
          <div>
            <p className="text-xs font-medium text-gray-400 uppercase tracking-wide mb-3">
              {t("documentLabel")}
            </p>

            {/* PDF Preview thumbnail */}
            <div
              className="relative w-full h-48 bg-gray-50 border border-gray-200 rounded-lg overflow-hidden cursor-pointer group mb-3"
              onClick={handleOpenPreview}
            >
              {pdfBlobUrl ? (
                <iframe
                  src={pdfBlobUrl}
                  className="w-full h-full pointer-events-none"
                  title={t("documentLabel")}
                  tabIndex={-1}
                />
              ) : (
                <div className="w-full h-full flex flex-col items-center justify-center gap-2 text-gray-400">
                  <FileText className="h-8 w-8" />
                  <span className="text-xs">{t("documentLabel")}</span>
                </div>
              )}
              <div className="absolute inset-0 bg-black/0 group-hover:bg-black/20 transition-colors flex items-center justify-center">
                <div className="opacity-0 group-hover:opacity-100 transition-opacity bg-white rounded-full p-3 shadow-lg">
                  {isPdfLoading
                    ? <Loader2 className="h-5 w-5 text-gray-700 animate-spin" />
                    : <Eye className="h-5 w-5 text-gray-700" />
                  }
                </div>
              </div>
            </div>

            {/* Action buttons */}
            <div className="flex items-center gap-2">
              <Button variant="outline" size="sm" onClick={handleOpenPreview} disabled={isPdfLoading}>
                {isPdfLoading
                  ? <Loader2 className="h-3.5 w-3.5 mr-1.5 animate-spin" />
                  : <FileText className="h-3.5 w-3.5 mr-1.5" />
                }
                {t("previewDocumentButton")}
              </Button>
              <Button variant="outline" size="sm" onClick={handleDownload} disabled={isPdfLoading}>
                <Download className="h-3.5 w-3.5 mr-1.5" />
                {t("downloadDocumentButton")}
              </Button>
              <Button variant="outline" size="sm" onClick={handleOpenInTab} disabled={isPdfLoading}>
                <ExternalLink className="h-3.5 w-3.5 mr-1.5" />
                {t("openDocumentButton")}
              </Button>
            </div>
          </div>
        </div>
      </div>

      {isPdfOpen && pdfBlobUrl && (
        <PdfPreviewModal
          url={pdfBlobUrl}
          filename={`${record.title}.pdf`}
          onClose={() => setIsPdfOpen(false)}
          labels={{
            download: t("downloadDocumentButton"),
            openFullscreen: t("openDocumentButton"),
          }}
        />
      )}
    </div>
  );
}
