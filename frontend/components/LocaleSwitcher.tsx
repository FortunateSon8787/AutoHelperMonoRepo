"use client";

import { useTransition } from "react";
import { useLocale } from "next-intl";
import { setLocaleCookie } from "@/app/actions/locale";
import { cn } from "@/lib/utils";

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
    <div className="flex gap-0.5 bg-secondary border border-border rounded-lg p-0.5">
      {Object.entries(localeLabels).map(([loc, label]) => (
        <button
          key={loc}
          onClick={() => handleChange(loc)}
          disabled={isPending || locale === loc}
          className={cn(
            "px-2.5 py-1 text-xs font-semibold rounded-md transition-all",
            locale === loc
              ? "bg-card text-foreground shadow-sm"
              : "text-muted-foreground hover:text-foreground"
          )}
        >
          {label}
        </button>
      ))}
    </div>
  );
}
