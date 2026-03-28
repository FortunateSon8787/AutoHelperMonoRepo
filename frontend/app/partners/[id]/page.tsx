import { getTranslations } from "next-intl/server";
import { notFound } from "next/navigation";
import Link from "next/link";
import { Building2, MapPin, Clock, Phone, Globe, CheckCircle, ArrowLeft } from "lucide-react";
import type { PartnerProfile } from "@/types/partner";

// ─── SSR data fetcher ─────────────────────────────────────────────────────────

async function fetchPartner(id: string): Promise<PartnerProfile | null> {
  try {
    const res = await fetch(
      `${process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:8080"}/api/partners/${id}`,
      { next: { revalidate: 60 } }
    );
    if (res.status === 404) return null;
    if (!res.ok) throw new Error("Server error");
    return res.json() as Promise<PartnerProfile>;
  } catch {
    return null;
  }
}

// ─── Metadata ─────────────────────────────────────────────────────────────────

export async function generateMetadata({ params }: { params: Promise<{ id: string }> }) {
  const { id } = await params;
  const partner = await fetchPartner(id);
  if (!partner) return { title: "Партнёр не найден — AutoHelper" };
  return { title: `${partner.name} — AutoHelper` };
}

// ─── Page ─────────────────────────────────────────────────────────────────────

interface Props {
  params: Promise<{ id: string }>;
}

export default async function PartnerProfilePage({ params }: Props) {
  const { id } = await params;
  const partner = await fetchPartner(id);

  if (!partner) notFound();

  const t = await getTranslations("partners.profile");

  const typeLabels: Record<string, string> = {
    AutoService: t("types.AutoService"),
    CarWash: t("types.CarWash"),
    Towing: t("types.Towing"),
    AutoShop: t("types.AutoShop"),
    Other: t("types.Other"),
  };

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

        <Link href="/partners" className="inline-flex items-center gap-1.5 text-sm text-gray-500 hover:text-gray-800 mb-6 transition-colors">
          <ArrowLeft className="h-4 w-4" />
          {t("backToSearch")}
        </Link>

        <div className="bg-white border border-gray-200 rounded-xl p-8 shadow-sm space-y-6">
          {/* Title row */}
          <div className="flex items-start gap-4">
            <div className="w-12 h-12 rounded-full bg-gray-100 flex items-center justify-center flex-shrink-0">
              {partner.logoUrl ? (
                // eslint-disable-next-line @next/next/no-img-element
                <img src={partner.logoUrl} alt={partner.name} className="w-12 h-12 rounded-full object-cover" />
              ) : (
                <Building2 className="h-6 w-6 text-gray-400" />
              )}
            </div>
            <div className="min-w-0">
              <h1 className="text-xl font-bold text-gray-900">{partner.name}</h1>
              <p className="text-sm text-gray-400 mt-0.5">
                {typeLabels[partner.type] ?? partner.type}
              </p>
              {partner.isVerified && (
                <span className="inline-flex items-center gap-1 text-xs text-green-600 mt-1">
                  <CheckCircle className="h-3.5 w-3.5" />
                  {t("verified")}
                </span>
              )}
            </div>
          </div>

          {/* Description */}
          <div>
            <p className="text-sm text-gray-500 font-medium mb-1">{t("specializationLabel")}</p>
            <p className="text-sm text-gray-700">{partner.specialization}</p>
          </div>

          <div>
            <p className="text-sm text-gray-500 font-medium mb-1">{t("descriptionLabel")}</p>
            <p className="text-sm text-gray-700 whitespace-pre-wrap">{partner.description}</p>
          </div>

          {/* Address */}
          <div className="flex items-start gap-2 text-sm text-gray-700">
            <MapPin className="h-4 w-4 text-gray-400 flex-shrink-0 mt-0.5" />
            <span>{partner.address}</span>
          </div>

          {/* Working hours */}
          <div className="flex items-center gap-2 text-sm text-gray-700">
            <Clock className="h-4 w-4 text-gray-400 flex-shrink-0" />
            <span>
              {partner.workingOpenFrom} – {partner.workingOpenTo}{" "}
              <span className="text-gray-400">({partner.workingDays})</span>
            </span>
          </div>

          {/* Contacts */}
          <div className="space-y-2">
            <p className="text-sm text-gray-500 font-medium">{t("contactsLabel")}</p>
            <div className="flex items-center gap-2 text-sm text-gray-700">
              <Phone className="h-4 w-4 text-gray-400" />
              <span>{partner.contactsPhone}</span>
            </div>
            {partner.contactsWebsite && (
              <div className="flex items-center gap-2 text-sm text-gray-700">
                <Globe className="h-4 w-4 text-gray-400" />
                <a
                  href={partner.contactsWebsite}
                  target="_blank"
                  rel="noopener noreferrer"
                  className="text-gray-900 underline underline-offset-2 hover:text-gray-600"
                >
                  {partner.contactsWebsite}
                </a>
              </div>
            )}
          </div>
        </div>
      </div>
    </div>
  );
}
