"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import { useParams } from "next/navigation";
import { Loader2, ArrowLeft, FileText, ExternalLink } from "lucide-react";
import { useTranslations } from "next-intl";

import { Button } from "@/components/ui/button";
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
            <p className="text-xs font-medium text-gray-400 uppercase tracking-wide mb-2">
              {t("documentLabel")}
            </p>
            <a href={record.documentUrl} target="_blank" rel="noopener noreferrer">
              <Button variant="outline" size="sm">
                <FileText className="h-3.5 w-3.5 mr-1.5" />
                {t("openDocumentButton")}
                <ExternalLink className="h-3 w-3 ml-1.5 text-gray-400" />
              </Button>
            </a>
          </div>
        </div>
      </div>
    </div>
  );
}
