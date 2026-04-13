import type { NextConfig } from "next";
import createNextIntlPlugin from "next-intl/plugin";

const withNextIntl = createNextIntlPlugin("./i18n/request.ts");

const securityHeaders = [
  // Запрещает встраивание страницы в <iframe> с других доменов (clickjacking)
  { key: "X-Frame-Options", value: "SAMEORIGIN" },
  // Отключает MIME-sniffing — браузер не будет угадывать тип контента
  { key: "X-Content-Type-Options", value: "nosniff" },
  // Управление реферером: отправляем только origin при cross-origin запросах
  { key: "Referrer-Policy", value: "strict-origin-when-cross-origin" },
  // Разрешаем геолокацию только self (используется на странице /partners)
  {
    key: "Permissions-Policy",
    value: "geolocation=(self), camera=(), microphone=()",
  },
  // Базовый CSP: разрешаем ресурсы только с нашего домена и доверенных CDN
  // unsafe-inline нужен для next.js inline scripts/styles; tile.openstreetmap.org — для Leaflet карты
  {
    key: "Content-Security-Policy",
    value: [
      "default-src 'self'",
      "script-src 'self' 'unsafe-inline' 'unsafe-eval'",
      "style-src 'self' 'unsafe-inline'",
      "img-src 'self' data: blob: https://*.tile.openstreetmap.org https://maps.googleapis.com",
      "font-src 'self'",
      "connect-src 'self' http://localhost:8080",
      "frame-ancestors 'self'",
    ].join("; "),
  },
];

const nextConfig: NextConfig = {
  // Produces a self-contained output in .next/standalone — required for the
  // multi-stage Docker build (copies only the minimal set of files needed).
  output: "standalone",

  async headers() {
    return [
      {
        source: "/(.*)",
        headers: securityHeaders,
      },
    ];
  },
};

export default withNextIntl(nextConfig);
