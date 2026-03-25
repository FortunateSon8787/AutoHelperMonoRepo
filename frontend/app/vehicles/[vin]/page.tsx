import { getTranslations } from "next-intl/server";
import { User, Car } from "lucide-react";
import { vehicleService } from "@/services/vehicleService";
import type { PublicVehicle, VehicleOwner } from "@/types/vehicle";

// ─── Status badge color map ────────────────────────────────────────────────────

const STATUS_COLORS: Record<string, string> = {
  Active: "bg-green-100 text-green-700",
  ForSale: "bg-blue-100 text-blue-700",
  InRepair: "bg-yellow-100 text-yellow-700",
  Recycled: "bg-gray-100 text-gray-600",
  Dismantled: "bg-red-100 text-red-700",
};

// ─── Page ─────────────────────────────────────────────────────────────────────

interface Props {
  params: Promise<{ vin: string }>;
}

export default async function VehiclePublicPage({ params }: Props) {
  const { vin } = await params;

  const [tCard, tOwner] = await Promise.all([
    getTranslations("vehicles.publicCard"),
    getTranslations("vehicles.ownerCard"),
  ]);

  // Fetch vehicle details and owner in parallel
  const [vehicleResult, ownerResult] = await Promise.allSettled([
    vehicleService.getByVin(vin),
    vehicleService.getOwnerByVin(vin),
  ]);

  const vehicle: PublicVehicle | null =
    vehicleResult.status === "fulfilled" ? vehicleResult.value : null;
  const owner: VehicleOwner | null =
    ownerResult.status === "fulfilled" ? ownerResult.value : null;

  const vehicleError =
    vehicleResult.status === "rejected"
      ? (vehicleResult.reason instanceof Error
          ? vehicleResult.reason.message
          : "server_error")
      : null;

  return (
    <div className="min-h-screen bg-gray-50 px-4 py-10">
      <div className="max-w-lg mx-auto space-y-4">
        {/* Header */}
        <div className="flex items-center gap-3 mb-8">
          <div className="w-9 h-9 rounded-lg bg-gray-900 flex items-center justify-center text-white font-bold text-lg">
            A
          </div>
          <span className="text-xl font-bold text-gray-900">AutoHelper</span>
        </div>

        {/* Vehicle not found */}
        {vehicleError === "not_found" ? (
          <div className="bg-red-50 border border-red-200 text-red-700 rounded-lg px-6 py-4 text-sm">
            {tCard("notFound")}
          </div>
        ) : vehicleError ? (
          <div className="bg-red-50 border border-red-200 text-red-700 rounded-lg px-6 py-4 text-sm">
            {tCard("serverError")}
          </div>
        ) : (
          /* ── Vehicle Details Card ── */
          <div className="bg-white border border-gray-200 rounded-xl p-8 shadow-sm">
            <div className="flex items-center gap-3 mb-6">
              <div className="w-10 h-10 rounded-full bg-gray-100 flex items-center justify-center">
                <Car className="h-5 w-5 text-gray-500" />
              </div>
              <div>
                <h1 className="text-xl font-bold text-gray-900">
                  {tCard("vehicleTitle")}
                </h1>
                <p className="text-sm text-gray-500 font-mono uppercase">
                  {vin}
                </p>
              </div>
            </div>

            <dl className="grid grid-cols-2 gap-x-6 gap-y-4">
              <div>
                <dt className="text-xs text-gray-400 uppercase tracking-wide mb-1">
                  {tCard("brandLabel")}
                </dt>
                <dd className="text-sm font-semibold text-gray-900">
                  {vehicle!.brand}
                </dd>
              </div>

              <div>
                <dt className="text-xs text-gray-400 uppercase tracking-wide mb-1">
                  {tCard("modelLabel")}
                </dt>
                <dd className="text-sm font-semibold text-gray-900">
                  {vehicle!.model}
                </dd>
              </div>

              <div>
                <dt className="text-xs text-gray-400 uppercase tracking-wide mb-1">
                  {tCard("yearLabel")}
                </dt>
                <dd className="text-sm font-semibold text-gray-900">
                  {vehicle!.year}
                </dd>
              </div>

              <div>
                <dt className="text-xs text-gray-400 uppercase tracking-wide mb-1">
                  {tCard("colorLabel")}
                </dt>
                <dd className="text-sm font-semibold text-gray-900">
                  {vehicle!.color ?? (
                    <span className="text-gray-400 italic">
                      {tCard("noColor")}
                    </span>
                  )}
                </dd>
              </div>

              <div>
                <dt className="text-xs text-gray-400 uppercase tracking-wide mb-1">
                  {tCard("mileageLabel")}
                </dt>
                <dd className="text-sm font-semibold text-gray-900">
                  {vehicle!.mileage.toLocaleString()} {tCard("km")}
                </dd>
              </div>

              <div>
                <dt className="text-xs text-gray-400 uppercase tracking-wide mb-1">
                  {tCard("statusLabel")}
                </dt>
                <dd>
                  <span
                    className={`inline-block text-xs font-medium px-2 py-0.5 rounded-full ${STATUS_COLORS[vehicle!.status] ?? "bg-gray-100 text-gray-600"}`}
                  >
                    {vehicle!.status}
                  </span>
                </dd>
              </div>
            </dl>

            {vehicle!.partnerName && (
              <div className="mt-4 pt-4 border-t border-gray-100">
                <span className="text-xs text-gray-400 uppercase tracking-wide">
                  {tCard("partnerLabel")}
                </span>{" "}
                <span className="text-sm font-semibold text-gray-900">
                  {vehicle!.partnerName}
                </span>
              </div>
            )}
          </div>
        )}

        {/* ── Owner Card ── */}
        {owner && (
          <div className="bg-white border border-gray-200 rounded-xl p-8 shadow-sm">
            <div className="flex items-center gap-3 mb-6">
              {owner.avatarUrl ? (
                // eslint-disable-next-line @next/next/no-img-element
                <img
                  src={owner.avatarUrl}
                  alt={owner.name}
                  className="w-12 h-12 rounded-full object-cover"
                />
              ) : (
                <div className="w-12 h-12 rounded-full bg-gray-100 flex items-center justify-center">
                  <User className="h-6 w-6 text-gray-500" />
                </div>
              )}
              <h2 className="text-lg font-bold text-gray-900">
                {tOwner("title")}
              </h2>
            </div>

            <dl className="space-y-4">
              <div>
                <dt className="text-xs text-gray-400 uppercase tracking-wide mb-1">
                  {tOwner("ownerLabel")}
                </dt>
                <dd className="text-base font-semibold text-gray-900">
                  {owner.name}
                </dd>
              </div>

              <div>
                <dt className="text-xs text-gray-400 uppercase tracking-wide mb-1">
                  {tOwner("contactsLabel")}
                </dt>
                <dd className="text-sm text-gray-700">
                  {owner.contacts ?? (
                    <span className="text-gray-400 italic">
                      {tOwner("noContacts")}
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
