"use client";

import Link from "next/link";
import {
  Car,
  Stethoscope,
  FileCheck,
  MapPin,
  Clock,
  X,
  TrendingUp,
  Plus,
  MessageSquare,
  Trash2,
  ChevronDown,
  Loader2,
} from "lucide-react";
import { useState } from "react";
import { useTranslations } from "next-intl";

import { Button } from "@/components/ui/button";
import { ConfirmDialog } from "@/components/ui/confirm-dialog";
import type { ChatMode, ChatSummary } from "@/types/chat";
import type { Vehicle } from "@/types/vehicle";
import type { SubscriptionInfo } from "@/types/client";

// ─── Props ────────────────────────────────────────────────────────────────────

interface ChatSidebarProps {
  vehicles: Vehicle[];
  chats: ChatSummary[];
  hasNextPage: boolean;
  isLoadingMore: boolean;
  subscription: SubscriptionInfo | null;
  selectedVehicleId: string | undefined;
  selectedMode: ChatMode;
  activeChatId: string | undefined;
  isOpen: boolean;
  onClose: () => void;
  onVehicleSelect: (vehicleId: string | undefined) => void;
  onModeChange: (mode: ChatMode) => void;
  onChatSelect: (chatId: string) => void;
  onNewChat: () => void;
  onLoadMore: () => void;
  onDeleteChat: (chatId: string) => Promise<void>;
}

// ─── Mode config ──────────────────────────────────────────────────────────────

const MODES: { id: ChatMode; icon: React.ElementType; gradient: string }[] = [
  { id: "FaultHelp", icon: Stethoscope, gradient: "from-sky-400 to-cyan-500" },
  { id: "WorkClarification", icon: FileCheck, gradient: "from-primary to-blue-600" },
  { id: "PartnerAdvice", icon: MapPin, gradient: "from-emerald-500 to-green-600" },
];

// ─── Component ────────────────────────────────────────────────────────────────

export function ChatSidebar({
  vehicles,
  chats,
  hasNextPage,
  isLoadingMore,
  subscription,
  selectedVehicleId,
  selectedMode,
  activeChatId,
  isOpen,
  onClose,
  onVehicleSelect,
  onModeChange,
  onChatSelect,
  onNewChat,
  onLoadMore,
  onDeleteChat,
}: ChatSidebarProps) {
  const t = useTranslations("chat.sidebar");
  const tModes = useTranslations("chat.modes");

  const [deletingChatId, setDeletingChatId] = useState<string | null>(null);
  const [confirmChatId, setConfirmChatId] = useState<string | null>(null);

  const selectedVehicle = vehicles.find((v) => v.id === selectedVehicleId);

  const filteredChats = selectedVehicleId
    ? chats.filter((c) => c.vehicleId === selectedVehicleId)
    : chats;

  const formatDate = (iso: string) => {
    const date = new Date(iso);
    const now = new Date();
    const diffH = Math.floor((now.getTime() - date.getTime()) / 3600000);
    if (diffH < 1) return "< 1h ago";
    if (diffH < 24) return `${diffH}h ago`;
    const diffD = Math.floor(diffH / 24);
    if (diffD < 7) return `${diffD}d ago`;
    return date.toLocaleDateString();
  };

  const renewsOn = subscription?.endDate
    ? new Date(subscription.endDate).toLocaleDateString()
    : null;

  const handleDeleteRequest = (e: React.MouseEvent, chatId: string) => {
    e.stopPropagation();
    setConfirmChatId(chatId);
  };

  const handleDeleteConfirm = async () => {
    if (!confirmChatId) return;
    const chatId = confirmChatId;
    setConfirmChatId(null);
    setDeletingChatId(chatId);
    try {
      await onDeleteChat(chatId);
    } finally {
      setDeletingChatId(null);
    }
  };

  return (
    <>
      {/* Mobile overlay */}
      {isOpen && (
        <div
          className="fixed inset-0 bg-black/20 z-40 lg:hidden"
          onClick={onClose}
        />
      )}

      <aside
        className={`fixed lg:static inset-y-0 left-0 z-50 w-80 bg-card border-r border-border flex flex-col transition-transform lg:translate-x-0 ${
          isOpen ? "translate-x-0" : "-translate-x-full"
        }`}
      >
        {/* Header */}
        <div className="flex items-center justify-between px-4 h-16 border-b border-border flex-shrink-0">
          <span className="text-lg font-semibold text-primary">AutoHelper</span>
          <div className="flex items-center gap-2">
            <Button size="sm" variant="outline" onClick={onNewChat} className="gap-1.5 h-8">
              <Plus className="w-3.5 h-3.5" />
              {t("newChat")}
            </Button>
            <button
              onClick={onClose}
              className="lg:hidden p-1.5 hover:bg-background rounded-lg"
            >
              <X className="w-4 h-4" />
            </button>
          </div>
        </div>

        <div className="flex-1 overflow-y-auto p-4 space-y-5">
          {/* Vehicle selector */}
          <div className="bg-gradient-to-br from-secondary to-muted/30 rounded-xl p-4 border border-border space-y-3">
            {selectedVehicle ? (
              <>
                <div className="flex items-center justify-between">
                  <div className="text-xs text-muted-foreground">{t("selectedVehicle")}</div>
                  <div className="flex items-center gap-1 px-2 py-0.5 bg-success/10 rounded-md">
                    <div className="w-1.5 h-1.5 rounded-full bg-success" />
                    <span className="text-xs text-success">{t("vehicleStatus")}</span>
                  </div>
                </div>
                <div className="flex items-center gap-3">
                  <div className="w-10 h-10 bg-card rounded-lg flex items-center justify-center shadow-sm border border-border">
                    <Car className="w-5 h-5 text-primary" />
                  </div>
                  <div>
                    <div className="font-medium text-foreground text-sm">
                      {selectedVehicle.brand} {selectedVehicle.model}
                    </div>
                    <div className="text-xs text-muted-foreground">{selectedVehicle.year}</div>
                  </div>
                </div>
                <div className="grid grid-cols-2 gap-2 pt-2 border-t border-border">
                  <div>
                    <div className="text-xs text-muted-foreground">{t("mileageLabel")}</div>
                    <div className="text-xs font-medium text-foreground">
                      {selectedVehicle.mileage.toLocaleString()} km
                    </div>
                  </div>
                  <div>
                    <div className="text-xs text-muted-foreground">{t("vinLabel")}</div>
                    <div className="text-xs font-medium text-foreground">
                      ...{selectedVehicle.vin.slice(-4)}
                    </div>
                  </div>
                </div>
                {vehicles.length > 1 && (
                  <select
                    className="w-full mt-1 text-xs border border-border rounded-lg px-2 py-1.5 bg-card text-foreground focus:outline-none focus:ring-2 focus:ring-ring"
                    value={selectedVehicleId ?? ""}
                    onChange={(e) => onVehicleSelect(e.target.value || undefined)}
                  >
                    {vehicles.map((v) => (
                      <option key={v.id} value={v.id}>
                        {v.brand} {v.model} ({v.year})
                      </option>
                    ))}
                  </select>
                )}
              </>
            ) : (
              <div className="text-center py-2">
                <Car className="w-8 h-8 text-muted-foreground mx-auto mb-2" />
                <p className="text-xs text-muted-foreground">{t("noVehicle")}</p>
                <Link
                  href="/dashboard/vehicles"
                  className="text-xs text-primary hover:underline mt-1 inline-block"
                >
                  {t("selectVehicle")}
                </Link>
              </div>
            )}
          </div>

          {/* Mode selector */}
          <div className="space-y-2">
            <div className="text-xs font-medium text-foreground uppercase tracking-wide px-1">
              {t("modesTitle")}
            </div>
            {MODES.map((mode) => (
              <button
                key={mode.id}
                onClick={() => {
                  onModeChange(mode.id);
                  onClose();
                }}
                className={`w-full text-left p-3 rounded-xl border transition-all ${
                  selectedMode === mode.id && !activeChatId
                    ? "bg-secondary border-ring/40 shadow-sm"
                    : "bg-card border-border hover:border-border hover:shadow-sm hover:bg-secondary/50"
                }`}
              >
                <div className="flex items-start gap-3">
                  <div
                    className={`w-9 h-9 bg-gradient-to-br ${mode.gradient} rounded-lg flex items-center justify-center flex-shrink-0 shadow-sm`}
                  >
                    <mode.icon className="w-4.5 h-4.5 text-white" />
                  </div>
                  <div className="flex-1 min-w-0">
                    <div className="text-sm font-medium text-foreground">
                      {tModes(mode.id)}
                    </div>
                    <div className="text-xs text-muted-foreground leading-relaxed">
                      <ModeDesc modeId={mode.id} />
                    </div>
                  </div>
                </div>
              </button>
            ))}
          </div>

          {/* Recent chats */}
          {filteredChats.length > 0 && (
            <div className="space-y-2">
              <div className="text-xs font-medium text-foreground uppercase tracking-wide px-1">
                {t("recentChats")}
              </div>
              <div className="space-y-1">
                {filteredChats.map((chat) => (
                  <div
                    key={chat.id}
                    className={`group relative flex items-start gap-2 px-3 py-2.5 rounded-lg transition-colors cursor-pointer ${
                      activeChatId === chat.id
                        ? "bg-secondary border border-border"
                        : "hover:bg-secondary/60"
                    }`}
                    onClick={() => onChatSelect(chat.id)}
                  >
                    <MessageSquare className="w-3.5 h-3.5 text-muted-foreground mt-0.5 flex-shrink-0" />
                    <div className="flex-1 min-w-0">
                      <div className="text-xs text-foreground truncate font-medium pr-5">
                        {chat.title}
                      </div>
                      <div className="text-xs text-muted-foreground flex items-center gap-1 mt-0.5">
                        <Clock className="w-2.5 h-2.5" />
                        {formatDate(chat.createdAt)}
                      </div>
                    </div>

                    {/* Delete button — visible on hover */}
                    <button
                      onClick={(e) => handleDeleteRequest(e, chat.id)}
                      disabled={deletingChatId === chat.id}
                      className="absolute right-2 top-2.5 p-1 rounded-md opacity-0 group-hover:opacity-100 hover:bg-destructive/10 hover:text-destructive text-muted-foreground transition-all disabled:pointer-events-none"
                      aria-label={t("deleteChat")}
                    >
                      {deletingChatId === chat.id ? (
                        <Loader2 className="w-3 h-3 animate-spin" />
                      ) : (
                        <Trash2 className="w-3 h-3" />
                      )}
                    </button>
                  </div>
                ))}
              </div>

              {/* Load more */}
              {hasNextPage && (
                <Button
                  variant="ghost"
                  size="sm"
                  className="w-full gap-1.5 text-xs text-muted-foreground"
                  onClick={onLoadMore}
                  disabled={isLoadingMore}
                >
                  {isLoadingMore ? (
                    <Loader2 className="w-3 h-3 animate-spin" />
                  ) : (
                    <ChevronDown className="w-3 h-3" />
                  )}
                  {t("loadMore")}
                </Button>
              )}
            </div>
          )}

          {/* Subscription usage */}
          <div className="bg-gradient-to-br from-primary/5 to-accent/5 rounded-xl p-4 border border-primary/20 space-y-2">
            <div className="flex items-center gap-2 text-sm font-medium text-foreground">
              <TrendingUp className="w-4 h-4 text-primary" />
              {t("subscription")}
            </div>
            {subscription ? (
              <>
                <div>
                  <div className="text-2xl font-semibold text-primary">
                    {subscription.aiRequestsRemaining}
                  </div>
                  <div className="text-xs text-muted-foreground">{t("requestsRemaining")}</div>
                </div>
                {renewsOn && (
                  <div className="pt-2 border-t border-primary/10">
                    <div className="text-xs text-muted-foreground">
                      {t("renewsOn")}: {renewsOn}
                    </div>
                  </div>
                )}
              </>
            ) : (
              <div className="text-xs text-muted-foreground">
                {t("noSubscription")}{" "}
                <Link href="/subscription" className="text-primary hover:underline">
                  {t("upgradeLink")}
                </Link>
              </div>
            )}
          </div>
        </div>
      </aside>

      {/* Delete confirm dialog */}
      <ConfirmDialog
        open={confirmChatId !== null}
        title={t("deleteChatTitle")}
        description={t("deleteChatDescription")}
        confirmLabel={t("deleteChatConfirm")}
        cancelLabel={t("deleteChatCancel")}
        isDestructive
        onConfirm={handleDeleteConfirm}
        onCancel={() => setConfirmChatId(null)}
      />
    </>
  );
}

// ─── Mode description helper ──────────────────────────────────────────────────

function ModeDesc({ modeId }: { modeId: ChatMode }) {
  const t = useTranslations("chat.sidebar");
  const map: Record<ChatMode, string> = {
    FaultHelp: t("diagnosticsDesc"),
    WorkClarification: t("reviewDesc"),
    PartnerAdvice: t("partnersDesc"),
  };
  return <>{map[modeId]}</>;
}
