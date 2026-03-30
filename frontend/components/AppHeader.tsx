"use client";

import { AppLogo } from "@/components/AppLogo";
import { LocaleSwitcher } from "@/components/LocaleSwitcher";
import { cn } from "@/lib/utils";

interface AppHeaderProps {
  children?: React.ReactNode;
  className?: string;
}

/**
 * Sticky top header used in auth and dashboard pages.
 * Pass action buttons/nav as children.
 */
export function AppHeader({ children, className }: AppHeaderProps) {
  return (
    <header
      className={cn(
        "sticky top-0 z-50 w-full bg-card/90 backdrop-blur-sm border-b border-border shadow-sm",
        className
      )}
    >
      <div className="max-w-5xl mx-auto px-4 sm:px-6 h-16 flex items-center justify-between gap-4">
        <AppLogo href="/" />
        <div className="flex items-center gap-3">
          {children}
          <LocaleSwitcher />
        </div>
      </div>
    </header>
  );
}
