import { getTranslations } from "next-intl/server";
import { redirect } from "next/navigation";
import { Search } from "lucide-react";

// ─── Page ─────────────────────────────────────────────────────────────────────

interface Props {
  searchParams: Promise<{ vin?: string }>;
}

export default async function VehiclesSearchPage({ searchParams }: Props) {
  const vin = ((await searchParams).vin ?? "").trim().toUpperCase();

  if (vin) {
    redirect(`/vehicles/${encodeURIComponent(vin)}`);
  }

  const t = await getTranslations("vehicles.vinSearch");

  return (
    <div className="min-h-screen bg-gray-50 px-4 py-10">
      <div className="max-w-lg mx-auto">
        {/* Header */}
        <div className="flex items-center gap-3 mb-10">
          <div className="w-9 h-9 rounded-lg bg-gray-900 flex items-center justify-center text-white font-bold text-lg">
            A
          </div>
          <span className="text-xl font-bold text-gray-900">AutoHelper</span>
        </div>

        {/* Search Card */}
        <div className="bg-white border border-gray-200 rounded-xl p-8 shadow-sm">
          <div className="flex items-center gap-3 mb-2">
            <Search className="h-5 w-5 text-gray-500" />
            <h1 className="text-xl font-bold text-gray-900">{t("title")}</h1>
          </div>
          <p className="text-sm text-gray-500 mb-6">{t("subtitle")}</p>

          <form method="GET" action="/vehicles">
            <label
              htmlFor="vin"
              className="block text-xs font-medium text-gray-600 uppercase tracking-wide mb-1"
            >
              {t("vinLabel")}
            </label>
            <input
              id="vin"
              name="vin"
              type="text"
              placeholder={t("vinPlaceholder")}
              maxLength={17}
              autoComplete="off"
              className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm font-mono uppercase placeholder:normal-case placeholder:font-sans focus:outline-none focus:ring-2 focus:ring-gray-900 focus:border-transparent mb-4"
            />
            <button
              type="submit"
              className="w-full bg-gray-900 text-white rounded-lg py-2 text-sm font-medium hover:bg-gray-800 transition-colors"
            >
              {t("searchButton")}
            </button>
          </form>
        </div>
      </div>
    </div>
  );
}
