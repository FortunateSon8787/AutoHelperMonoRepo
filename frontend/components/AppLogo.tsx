import Link from "next/link";
import { cn } from "@/lib/utils";

interface AppLogoProps {
  href?: string;
  className?: string;
}

export function AppLogo({ href = "/", className }: AppLogoProps) {
  const content = (
    <div className={cn("flex items-center gap-2.5", className)}>
      <div className="w-9 h-9 rounded-xl bg-primary flex items-center justify-center shadow-sm flex-shrink-0">
        <span className="text-primary-foreground font-bold text-base leading-none">A</span>
      </div>
      <span className="text-lg font-semibold text-foreground tracking-tight">AutoHelper</span>
    </div>
  );

  if (href) {
    return <Link href={href}>{content}</Link>;
  }

  return content;
}
