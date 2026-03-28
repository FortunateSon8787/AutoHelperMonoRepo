"use client";

import dynamic from "next/dynamic";
import { useEffect, useRef, useState } from "react";
import { Loader2, MapPin, Search, Filter, Phone, Globe, Clock } from "lucide-react";
import { useTranslations } from "next-intl";
import Link from "next/link";
import { partnerService, PartnerServiceError } from "@/services/partnerService";
import type { PartnerNearbyResult } from "@/types/partner";
import { PARTNER_TYPES } from "@/types/partner";
import "leaflet/dist/leaflet.css";

// Dynamic import to prevent SSR (Leaflet requires window)
const PartnersMap = dynamic(() => import("@/components/partners/PartnersMap"), {
  ssr: false,
  loading: () => (
    <div className="w-full h-full flex items-center justify-center bg-gray-100 rounded-xl">
      <Loader2 className="h-6 w-6 animate-spin text-gray-400" />
    </div>
  ),
});

const DEFAULT_LAT = 55.7558;
const DEFAULT_LNG = 37.6173;
const DEFAULT_RADIUS = 10;

export default function PartnersSearchPage() {
  const t = useTranslations("partners.search");

  const [partners, setPartners] = useState<PartnerNearbyResult[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [selectedId, setSelectedId] = useState<string | null>(null);

  const [lat, setLat] = useState(DEFAULT_LAT);
  const [lng, setLng] = useState(DEFAULT_LNG);
  const [radiusKm, setRadiusKm] = useState(DEFAULT_RADIUS);
  const [type, setType] = useState("");
  const [isOpenNow, setIsOpenNow] = useState(false);
  const [locationStatus, setLocationStatus] = useState<"idle" | "locating" | "done" | "error">("idle");

  const selectedRef = useRef<HTMLDivElement | null>(null);

  const doSearch = (searchLat: number, searchLng: number) => {
    setIsLoading(true);
    setError(null);
    setSelectedId(null);
    partnerService
      .searchNearby({
        lat: searchLat,
        lng: searchLng,
        radiusKm,
        type: type || undefined,
        isOpenNow,
      })
      .then(setPartners)
      .catch((err: unknown) => {
        if (err instanceof PartnerServiceError) {
          setError(t("errors.serverError"));
        } else {
          setError(t("errors.unknown"));
        }
      })
      .finally(() => setIsLoading(false));
  };

  const handleLocate = () => {
    if (!navigator.geolocation) {
      setError(t("errors.geolocationNotSupported"));
      return;
    }
    setLocationStatus("locating");
    navigator.geolocation.getCurrentPosition(
      (pos) => {
        const newLat = pos.coords.latitude;
        const newLng = pos.coords.longitude;
        setLat(newLat);
        setLng(newLng);
        setLocationStatus("done");
        doSearch(newLat, newLng);
      },
      () => {
        setLocationStatus("error");
        setError(t("errors.geolocationDenied"));
      }
    );
  };

  const handleSearch = () => doSearch(lat, lng);

  useEffect(() => {
    doSearch(lat, lng);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  useEffect(() => {
    if (selectedId && selectedRef.current) {
      selectedRef.current.scrollIntoView({ behavior: "smooth", block: "nearest" });
    }
  }, [selectedId]);

  const selectedPartner = partners.find((p) => p.id === selectedId) ?? null;

  return (
    <div className="min-h-screen bg-gray-50 flex flex-col">
      {/* Header */}
      <header className="bg-white border-b border-gray-200 px-4 py-3 flex items-center gap-3">
        <div className="w-8 h-8 rounded-lg bg-gray-900 flex items-center justify-center text-white font-bold">
          A
        </div>
        <span className="text-lg font-bold text-gray-900">AutoHelper</span>
        <span className="text-gray-400 mx-2">/</span>
        <span className="text-gray-700 text-sm font-medium">{t("title")}</span>
      </header>

      <div className="flex flex-1 overflow-hidden" style={{ height: "calc(100vh - 57px)" }}>
        {/* Sidebar */}
        <aside className="w-80 flex-shrink-0 bg-white border-r border-gray-200 flex flex-col overflow-hidden">
          {/* Filters */}
          <div className="p-4 border-b border-gray-100 space-y-3">
            <div className="flex gap-2">
              <div className="flex-1 space-y-1">
                <label className="text-xs font-medium text-gray-500 uppercase tracking-wide">Lat</label>
                <input
                  type="number"
                  step="any"
                  value={lat}
                  onChange={(e) => setLat(parseFloat(e.target.value) || DEFAULT_LAT)}
                  className="w-full border border-gray-300 rounded-lg px-2 py-1.5 text-sm focus:outline-none focus:ring-2 focus:ring-gray-900"
                />
              </div>
              <div className="flex-1 space-y-1">
                <label className="text-xs font-medium text-gray-500 uppercase tracking-wide">Lng</label>
                <input
                  type="number"
                  step="any"
                  value={lng}
                  onChange={(e) => setLng(parseFloat(e.target.value) || DEFAULT_LNG)}
                  className="w-full border border-gray-300 rounded-lg px-2 py-1.5 text-sm focus:outline-none focus:ring-2 focus:ring-gray-900"
                />
              </div>
            </div>

            <div className="flex gap-2 items-end">
              <div className="flex-1 space-y-1">
                <label className="text-xs font-medium text-gray-500 uppercase tracking-wide">
                  {t("radiusLabel")}
                </label>
                <input
                  type="number"
                  min={1}
                  max={100}
                  value={radiusKm}
                  onChange={(e) => setRadiusKm(Math.min(100, Math.max(1, parseInt(e.target.value) || DEFAULT_RADIUS)))}
                  className="w-full border border-gray-300 rounded-lg px-2 py-1.5 text-sm focus:outline-none focus:ring-2 focus:ring-gray-900"
                />
              </div>
              <button
                onClick={handleLocate}
                disabled={locationStatus === "locating"}
                title={t("locateMeButton")}
                className="h-9 px-3 rounded-lg border border-gray-300 text-gray-600 hover:bg-gray-50 disabled:opacity-50 flex items-center gap-1.5 text-sm"
              >
                {locationStatus === "locating" ? (
                  <Loader2 className="h-4 w-4 animate-spin" />
                ) : (
                  <MapPin className="h-4 w-4" />
                )}
              </button>
            </div>

            <div className="space-y-1">
              <label className="text-xs font-medium text-gray-500 uppercase tracking-wide">
                <Filter className="h-3 w-3 inline mr-1" />
                {t("typeLabel")}
              </label>
              <select
                value={type}
                onChange={(e) => setType(e.target.value)}
                className="w-full border border-gray-300 rounded-lg px-2 py-1.5 text-sm focus:outline-none focus:ring-2 focus:ring-gray-900 bg-white"
              >
                <option value="">{t("allTypes")}</option>
                {PARTNER_TYPES.map((pt) => (
                  <option key={pt} value={pt}>
                    {t(`types.${pt}`)}
                  </option>
                ))}
              </select>
            </div>

            <label className="flex items-center gap-2 text-sm text-gray-700 cursor-pointer select-none">
              <input
                type="checkbox"
                checked={isOpenNow}
                onChange={(e) => setIsOpenNow(e.target.checked)}
                className="rounded"
              />
              {t("isOpenNowLabel")}
            </label>

            <button
              onClick={handleSearch}
              disabled={isLoading}
              className="w-full bg-gray-900 text-white rounded-lg py-2 text-sm font-medium hover:bg-gray-800 transition-colors disabled:opacity-60 flex items-center justify-center gap-2"
            >
              {isLoading ? <Loader2 className="h-4 w-4 animate-spin" /> : <Search className="h-4 w-4" />}
              {t("searchButton")}
            </button>
          </div>

          {/* Error */}
          {error && (
            <div className="mx-4 mt-3 bg-red-50 border border-red-200 text-red-700 rounded-lg px-3 py-2 text-xs">
              {error}
            </div>
          )}

          {/* Results list */}
          <div className="flex-1 overflow-y-auto">
            {isLoading ? (
              <div className="flex items-center justify-center py-12">
                <Loader2 className="h-5 w-5 animate-spin text-gray-400" />
              </div>
            ) : partners.length === 0 && !error ? (
              <p className="text-center text-sm text-gray-400 py-10">{t("emptyState")}</p>
            ) : (
              <ul className="divide-y divide-gray-100">
                {partners.map((p) => (
                  <li key={p.id}>
                    <div
                      ref={p.id === selectedId ? selectedRef : null}
                      onClick={() => setSelectedId(p.id)}
                      className={`px-4 py-3 cursor-pointer transition-colors hover:bg-gray-50 ${
                        p.id === selectedId ? "bg-gray-50 border-l-2 border-gray-900" : ""
                      }`}
                    >
                      <div className="flex items-start justify-between gap-2">
                        <div className="min-w-0">
                          <p className="text-sm font-semibold text-gray-900 truncate">{p.name}</p>
                          <p className="text-xs text-gray-400 truncate">{p.address}</p>
                        </div>
                        <div className="text-right flex-shrink-0">
                          <p className="text-xs text-gray-500">{p.distanceKm} {t("kmLabel")}</p>
                          {p.isOpenNow ? (
                            <span className="text-xs text-green-600">{t("openLabel")}</span>
                          ) : (
                            <span className="text-xs text-red-400">{t("closedLabel")}</span>
                          )}
                        </div>
                      </div>
                      <Link
                        href={`/partners/${p.id}`}
                        onClick={(e) => e.stopPropagation()}
                        className="text-xs text-gray-500 underline underline-offset-2 hover:text-gray-900 mt-1 inline-block"
                      >
                        {t("viewProfileLink")}
                      </Link>
                    </div>
                  </li>
                ))}
              </ul>
            )}
          </div>

          {/* Selected partner quick-view */}
          {selectedPartner && (
            <div className="border-t border-gray-200 p-4 bg-gray-50 text-sm space-y-1.5">
              <p className="font-semibold text-gray-900">{selectedPartner.name}</p>
              <p className="text-gray-500 text-xs">{selectedPartner.specialization}</p>
              <div className="flex items-center gap-1.5 text-gray-600 text-xs">
                <Clock className="h-3 w-3" />
                {selectedPartner.workingOpenFrom} – {selectedPartner.workingOpenTo} ({selectedPartner.workingDays})
              </div>
              <div className="flex items-center gap-1.5 text-gray-600 text-xs">
                <Phone className="h-3 w-3" />
                {selectedPartner.contactsPhone}
              </div>
              {selectedPartner.contactsWebsite && (
                <div className="flex items-center gap-1.5 text-gray-600 text-xs">
                  <Globe className="h-3 w-3" />
                  <a href={selectedPartner.contactsWebsite} target="_blank" rel="noopener noreferrer" className="underline hover:text-gray-900">
                    {selectedPartner.contactsWebsite}
                  </a>
                </div>
              )}
              <Link
                href={`/partners/${selectedPartner.id}`}
                className="inline-block mt-1 text-xs bg-gray-900 text-white px-3 py-1 rounded-lg hover:bg-gray-800 transition-colors"
              >
                {t("viewProfileLink")}
              </Link>
            </div>
          )}
        </aside>

        {/* Map */}
        <main className="flex-1 p-4">
          <PartnersMap
            partners={partners}
            userLat={lat}
            userLng={lng}
            radiusKm={radiusKm}
            selectedId={selectedId}
            onSelectPartner={setSelectedId}
          />
        </main>
      </div>
    </div>
  );
}
