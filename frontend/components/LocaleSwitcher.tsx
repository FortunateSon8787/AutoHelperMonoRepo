"use client";

import { useTransition } from "react";
import { useLocale } from "next-intl";
import { setLocaleCookie } from "@/app/actions/locale";

const localeLabels: Record<string, string> = {
  ru: "RU",
  en: "EN",
};

export function LocaleSwitcher() {
  const locale = useLocale();
  const [isPending, startTransition] = useTransition();

  const handleChange = (newLocale: string) => {
    startTransition(async () => {
      await setLocaleCookie(newLocale);
    });
  };

  return (
    <div className="fixed top-4 right-4 z-50 flex gap-1 bg-white border border-gray-200 rounded-lg p-1 shadow-sm">
      {Object.entries(localeLabels).map(([loc, label]) => (
        <button
          key={loc}
          onClick={() => handleChange(loc)}
          disabled={isPending || locale === loc}
          className={`px-3 py-1 text-xs font-semibold rounded-md transition-colors ${
            locale === loc
              ? "bg-gray-900 text-white"
              : "text-gray-500 hover:text-gray-900"
          }`}
        >
          {label}
        </button>
      ))}
    </div>
  );
}
