"use client";

import { useEffect, useState, useCallback } from "react";
import { useTranslations, useLocale } from "next-intl";
import { Loader2 } from "lucide-react";

import { ChatSidebar } from "@/components/chat/ChatSidebar";
import { ChatWindow } from "@/components/chat/ChatWindow";
import { chatService, ChatServiceError } from "@/services/chatService";
import { vehicleService } from "@/services/vehicleService";
import { subscriptionService } from "@/services/subscriptionService";
import type {
  ChatMode,
  ChatSummary,
  ChatMessage,
  SendMessageResponse,
  DiagnosticsInput,
  WorkClarificationInput,
  PartnerAdviceInput,
} from "@/types/chat";
import type { Vehicle } from "@/types/vehicle";
import type { SubscriptionInfo } from "@/types/client";

// ─── Page ─────────────────────────────────────────────────────────────────────

export default function ChatPage() {
  const t = useTranslations("chat");
  const locale = useLocale();

  // ─── State ──────────────────────────────────────────────────────────────────
  const [selectedMode, setSelectedMode] = useState<ChatMode>("FaultHelp");
  const [selectedVehicleId, setSelectedVehicleId] = useState<string | undefined>(undefined);
  const [vehicles, setVehicles] = useState<Vehicle[]>([]);
  const [chats, setChats] = useState<ChatSummary[]>([]);
  const [subscription, setSubscription] = useState<SubscriptionInfo | null>(null);

  const [activeChatId, setActiveChatId] = useState<string | undefined>(undefined);
  const [messages, setMessages] = useState<ChatMessage[]>([]);
  const [chatStatus, setChatStatus] = useState<string | null>(null);

  const [isLoadingInit, setIsLoadingInit] = useState(true);
  const [isLoadingMessages, setIsLoadingMessages] = useState(false);
  const [isSending, setIsSending] = useState(false);
  const [sidebarOpen, setSidebarOpen] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // ─── Initial load ─────────────────────────────────────────────────────────

  useEffect(() => {
    const init = async () => {
      try {
        const [vehicleList, chatList, sub] = await Promise.all([
          vehicleService.getMyVehicles().catch(() => [] as Vehicle[]),
          chatService.getMyChats().catch(() => [] as ChatSummary[]),
          subscriptionService.getMySubscription().catch(() => null),
        ]);
        setVehicles(vehicleList);
        setChats(chatList);
        setSubscription(sub);
        if (vehicleList.length > 0) {
          setSelectedVehicleId(vehicleList[0].id);
        }
      } finally {
        setIsLoadingInit(false);
      }
    };
    init();
  }, []);

  // ─── Load messages for active chat ───────────────────────────────────────

  const loadMessages = useCallback(async (chatId: string) => {
    setIsLoadingMessages(true);
    setError(null);
    try {
      const msgs = await chatService.getChatMessages(chatId);
      setMessages(msgs);
      const chat = chats.find((c) => c.id === chatId);
      if (chat) setChatStatus(chat.status);
    } catch {
      setError(t("errors.loadFailed"));
    } finally {
      setIsLoadingMessages(false);
    }
  }, [chats, t]);

  const handleSelectChat = useCallback(async (chatId: string) => {
    setActiveChatId(chatId);
    await loadMessages(chatId);
  }, [loadMessages]);

  // ─── Create new chat ─────────────────────────────────────────────────────

  const handleCreateChat = useCallback(
    async (
      mode: ChatMode,
      title: string,
      modeInput: DiagnosticsInput | WorkClarificationInput | PartnerAdviceInput
    ) => {
      setError(null);
      setIsSending(true);
      try {
        const payload = {
          mode,
          title,
          vehicleId: selectedVehicleId,
          locale,
          ...(mode === "FaultHelp" && { diagnosticsInput: modeInput as DiagnosticsInput }),
          ...(mode === "WorkClarification" && { workClarificationInput: modeInput as WorkClarificationInput }),
          ...(mode === "PartnerAdvice" && { partnerAdviceInput: modeInput as PartnerAdviceInput }),
        };
        const res = await chatService.createChat(payload);
        const newChatSummary: ChatSummary = {
          id: res.chatId,
          mode,
          status: "Active",
          title,
          vehicleId: selectedVehicleId,
          messageCount: res.initialAssistantReply ? 1 : 0,
          createdAt: new Date().toISOString(),
        };
        setChats((prev) => [newChatSummary, ...prev]);
        setActiveChatId(res.chatId);

        const initialMessages: ChatMessage[] = [];
        if (res.initialAssistantReply) {
          initialMessages.push({
            id: "initial",
            role: "Assistant",
            content: res.initialAssistantReply,
            isValid: true,
            createdAt: new Date().toISOString(),
          });
        }
        setMessages(initialMessages);
        setChatStatus("Active");
      } catch (err) {
        if (err instanceof ChatServiceError) {
          const key = err.code === "forbidden" ? "forbidden" : "createFailed";
          setError(t(`errors.${key}`));
        } else {
          setError(t("errors.unknown"));
        }
      } finally {
        setIsSending(false);
      }
    },
    [selectedVehicleId, locale, t]
  );

  // ─── Send message ────────────────────────────────────────────────────────

  const handleSendMessage = useCallback(
    async (content: string) => {
      if (!activeChatId || !content.trim()) return;
      setError(null);
      setIsSending(true);

      const optimisticUserMsg: ChatMessage = {
        id: `opt-${Date.now()}`,
        role: "User",
        content,
        isValid: true,
        createdAt: new Date().toISOString(),
      };
      setMessages((prev) => [...prev, optimisticUserMsg]);

      try {
        const res: SendMessageResponse = await chatService.sendMessage(activeChatId, {
          content,
          locale,
        });
        const assistantMsg: ChatMessage = {
          id: `resp-${Date.now()}`,
          role: "Assistant",
          content: res.assistantReply,
          isValid: res.wasValid,
          createdAt: new Date().toISOString(),
        };
        setMessages((prev) => [...prev, assistantMsg]);
        setChatStatus(res.chatStatus);
        setChats((prev) =>
          prev.map((c) =>
            c.id === activeChatId
              ? { ...c, status: res.chatStatus, messageCount: c.messageCount + 2 }
              : c
          )
        );
        if (subscription) {
          setSubscription((prev) =>
            prev ? { ...prev, aiRequestsRemaining: Math.max(0, prev.aiRequestsRemaining - 1) } : prev
          );
        }
      } catch (err) {
        // Remove optimistic message on failure
        setMessages((prev) => prev.filter((m) => m.id !== optimisticUserMsg.id));
        if (err instanceof ChatServiceError) {
          const key =
            err.code === "forbidden"
              ? "forbidden"
              : err.code === "conflict"
              ? "conflict"
              : "sendFailed";
          setError(t(`errors.${key}`));
        } else {
          setError(t("errors.unknown"));
        }
      } finally {
        setIsSending(false);
      }
    },
    [activeChatId, locale, t, subscription]
  );

  // ─── New chat (reset state) ──────────────────────────────────────────────

  const handleNewChat = useCallback(() => {
    setActiveChatId(undefined);
    setMessages([]);
    setChatStatus(null);
    setError(null);
    setSidebarOpen(false);
  }, []);

  // ─── Loading state ───────────────────────────────────────────────────────

  if (isLoadingInit) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-background">
        <Loader2 className="h-6 w-6 animate-spin text-muted-foreground" />
      </div>
    );
  }

  // ─── Render ──────────────────────────────────────────────────────────────

  return (
    <div className="flex h-screen bg-background overflow-hidden">
      <ChatSidebar
        vehicles={vehicles}
        chats={chats}
        subscription={subscription}
        selectedVehicleId={selectedVehicleId}
        selectedMode={selectedMode}
        activeChatId={activeChatId}
        isOpen={sidebarOpen}
        onClose={() => setSidebarOpen(false)}
        onVehicleSelect={setSelectedVehicleId}
        onModeChange={(mode) => {
          setSelectedMode(mode);
          handleNewChat();
        }}
        onChatSelect={handleSelectChat}
        onNewChat={handleNewChat}
      />

      <div className="flex-1 flex flex-col min-w-0">
        <ChatWindow
          selectedMode={selectedMode}
          activeChatId={activeChatId}
          messages={messages}
          chatStatus={chatStatus}
          isLoadingMessages={isLoadingMessages}
          isSending={isSending}
          error={error}
          onMenuClick={() => setSidebarOpen(true)}
          onCreateChat={handleCreateChat}
          onSendMessage={handleSendMessage}
          onNewChat={handleNewChat}
          onClearError={() => setError(null)}
        />
      </div>
    </div>
  );
}
