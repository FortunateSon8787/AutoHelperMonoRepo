import { getTranslations } from "next-intl/server";
import { User } from "lucide-react";
import { vehicleService } from "@/services/vehicleService";

// ─── Page ─────────────────────────────────────────────────────────────────────

interface Props {
  params: Promise<{ vin: string }>;
}

export default async function VehicleOwnerPage({ params }: Props) {
  const { vin } = await params;
  const t = await getTranslations("vehicles.ownerCard");

  let owner: Awaited<ReturnType<typeof vehicleService.getOwnerByVin>> | null =
    null;
  let errorKey: string | null = null;

  try {
    owner = await vehicleService.getOwnerByVin(vin);
  } catch (err) {
    errorKey = err instanceof Error ? err.message : "server_error";
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

        {errorKey ? (
          <div className="bg-red-50 border border-red-200 text-red-700 rounded-lg px-6 py-4 text-sm">
            {errorKey === "not_found" ? t("notFound") : t("serverError")}
          </div>
        ) : (
          <div className="bg-white border border-gray-200 rounded-xl p-8 shadow-sm">
            {/* Title */}
            <div className="flex items-center gap-3 mb-6">
              {owner!.avatarUrl ? (
                // eslint-disable-next-line @next/next/no-img-element
                <img
                  src={owner!.avatarUrl}
                  alt={owner!.name}
                  className="w-12 h-12 rounded-full object-cover"
                />
              ) : (
                <div className="w-12 h-12 rounded-full bg-gray-100 flex items-center justify-center">
                  <User className="h-6 w-6 text-gray-500" />
                </div>
              )}
              <div>
                <h1 className="text-xl font-bold text-gray-900">
                  {t("title")}
                </h1>
                <p className="text-sm text-gray-500 font-mono uppercase">
                  {vin}
                </p>
              </div>
            </div>

            {/* Owner details */}
            <dl className="space-y-4">
              <div>
                <dt className="text-xs text-gray-400 uppercase tracking-wide mb-1">
                  {t("ownerLabel")}
                </dt>
                <dd className="text-base font-semibold text-gray-900">
                  {owner!.name}
                </dd>
              </div>

              <div>
                <dt className="text-xs text-gray-400 uppercase tracking-wide mb-1">
                  {t("contactsLabel")}
                </dt>
                <dd className="text-sm text-gray-700">
                  {owner!.contacts ?? (
                    <span className="text-gray-400 italic">
                      {t("noContacts")}
                    </span>
                  )}
                </dd>
              </div>
            </dl>
          </div>
        )}
      </div>
    </div>
  );
}
