"use client";

import { useEffect } from "react";
import { MapContainer, TileLayer, Marker, Popup, Circle, useMap } from "react-leaflet";
import L from "leaflet";
import type { PartnerNearbyResult } from "@/types/partner";

// Fix Leaflet default icon paths broken by webpack/Next.js bundling
delete (L.Icon.Default.prototype as { _getIconUrl?: unknown })._getIconUrl;
L.Icon.Default.mergeOptions({
  iconRetinaUrl: "https://unpkg.com/leaflet@1.9.4/dist/images/marker-icon-2x.png",
  iconUrl: "https://unpkg.com/leaflet@1.9.4/dist/images/marker-icon.png",
  shadowUrl: "https://unpkg.com/leaflet@1.9.4/dist/images/marker-shadow.png",
});

const userIcon = new L.Icon({
  iconUrl: "https://unpkg.com/leaflet@1.9.4/dist/images/marker-icon.png",
  iconRetinaUrl: "https://unpkg.com/leaflet@1.9.4/dist/images/marker-icon-2x.png",
  shadowUrl: "https://unpkg.com/leaflet@1.9.4/dist/images/marker-shadow.png",
  iconSize: [25, 41],
  iconAnchor: [12, 41],
  className: "leaflet-marker-user",
});

interface Props {
  partners: PartnerNearbyResult[];
  userLat: number;
  userLng: number;
  radiusKm: number;
  selectedId: string | null;
  onSelectPartner: (id: string) => void;
  yourLocationLabel: string;
  openLabel: string;
  closedLabel: string;
  kmLabel: string;
}

function RecenterView({ lat, lng }: { lat: number; lng: number }) {
  const map = useMap();
  useEffect(() => {
    map.setView([lat, lng]);
  }, [lat, lng, map]);
  return null;
}

export default function PartnersMap({
  partners,
  userLat,
  userLng,
  radiusKm,
  selectedId,
  onSelectPartner,
  yourLocationLabel,
  openLabel,
  closedLabel,
  kmLabel,
}: Props) {
  return (
    <MapContainer
      center={[userLat, userLng]}
      zoom={12}
      style={{ height: "100%", width: "100%" }}
      className="rounded-xl"
    >
      <TileLayer
        attribution='&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a>'
        url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
      />

      <RecenterView lat={userLat} lng={userLng} />

      {/* Search radius circle */}
      <Circle
        center={[userLat, userLng]}
        radius={radiusKm * 1000}
        pathOptions={{ color: "#374151", fillColor: "#374151", fillOpacity: 0.04, weight: 1.5 }}
      />

      {/* User position */}
      <Marker position={[userLat, userLng]} icon={userIcon}>
        <Popup>{yourLocationLabel}</Popup>
      </Marker>

      {/* Partner markers */}
      {partners.map((p) => (
        <Marker
          key={p.id}
          position={[p.locationLat, p.locationLng]}
          eventHandlers={{ click: () => onSelectPartner(p.id) }}
        >
          <Popup>
            <div className="text-sm">
              <p className="font-semibold">{p.name}</p>
              <p className="text-gray-500 text-xs">{p.address}</p>
              <p className="text-xs mt-1">{p.distanceKm} {kmLabel}</p>
              {p.isOpenNow ? (
                <span className="text-green-600 text-xs">● {openLabel}</span>
              ) : (
                <span className="text-red-500 text-xs">● {closedLabel}</span>
              )}
            </div>
          </Popup>
        </Marker>
      ))}
    </MapContainer>
  );
}
