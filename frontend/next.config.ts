import type { NextConfig } from "next";
import createNextIntlPlugin from "next-intl/plugin";

const withNextIntl = createNextIntlPlugin("./i18n/request.ts");

const nextConfig: NextConfig = {
  // Produces a self-contained output in .next/standalone — required for the
  // multi-stage Docker build (copies only the minimal set of files needed).
  output: "standalone",
};

export default withNextIntl(nextConfig);
