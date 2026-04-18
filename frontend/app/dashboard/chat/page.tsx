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

const PAGE_SIZE = 20;

// ─── Helpers ──────────────────────────────────────────────────────────────────

function parseDiagnosticsContent(content: string): DiagnosticsInput {
  const lines = content.split("\n");
  let symptoms = "";
  let recentEvents: string | undefined;
  let previousIssues: string | undefined;

  for (const line of lines) {
    if (line.startsWith("Symptoms: ")) {
      symptoms = line.slice("Symptoms: ".length);
    } else if (line.startsWith("Recent events: ")) {
      recentEvents = line.slice("Recent events: ".length);
    } else if (line.startsWith("Previous issues: ")) {
      previousIssues = line.slice("Previous issues: ".length);
    }
  }

  return { symptoms, recentEvents, previousIssues };
}

function parsePartnerAdviceContent(content: string): PartnerAdviceInput {
  const lines = content.split("\n");
  let request = "";
  let urgency: PartnerAdviceInput["urgency"] = "NotSpecified";
  let lat = 0;
  let lng = 0;
  let hasStructuredFormat = false;

  for (const line of lines) {
    if (line.startsWith("Request: ")) {
      request = line.slice("Request: ".length);
      hasStructuredFormat = true;
    } else if (line.startsWith("Urgency: ")) {
      const raw = line.slice("Urgency: ".length).trim();
      if (raw === "NotUrgent" || raw === "Urgent") urgency = raw;
    } else if (line.startsWith("Location: ")) {
      const coords = line.slice("Location: ".length).split(",");
      if (coords.length === 2) {
        lat = parseFloat(coords[0]) || 0;
        lng = parseFloat(coords[1]) || 0;
      }
    }
  }

  // For invalid messages, content is stored as raw user text (no structured format)
  if (!hasStructuredFormat) {
    request = content;
  }

  return { request, urgency, lat, lng };
}

function parseWorkClarificationContent(content: string): WorkClarificationInput {
  const lines = content.split("\n");
  let worksPerformed = "";
  let workReason = "";
  let laborCost = 0;
  let partsCost = 0;
  let guarantees: string | undefined;

  for (const line of lines) {
    if (line.startsWith("Works performed: ")) {
      worksPerformed = line.slice("Works performed: ".length);
    } else if (line.startsWith("Stated reason: ")) {
      workReason = line.slice("Stated reason: ".length);
    } else if (line.startsWith("Labor cost: ")) {
      laborCost = parseFloat(line.slice("Labor cost: ".length)) || 0;
    } else if (line.startsWith("Parts cost: ")) {
      partsCost = parseFloat(line.slice("Parts cost: ".length)) || 0;
    } else if (line.startsWith("Guarantees: ")) {
      guarantees = line.slice("Guarantees: ".length);
    }
  }

  return { worksPerformed, workReason, laborCost, partsCost, guarantees };
}

// ─── Page ─────────────────────────────────────────────────────────────────────

export default function ChatPage() {
  const t = useTranslations("chat");
  const locale = useLocale();

  // ─── State ──────────────────────────────────────────────────────────────────
  const [selectedMode, setSelectedMode] = useState<ChatMode>("FaultHelp");
  const [selectedVehicleId, setSelectedVehicleId] = useState<string | undefined>(undefined);
  const [vehicles, setVehicles] = useState<Vehicle[]>([]);
  const [chats, setChats] = useState<ChatSummary[]>([]);
  const [chatsPage, setChatsPage] = useState(1);
  const [hasNextPage, setHasNextPage] = useState(false);
  const [subscription, setSubscription] = useState<SubscriptionInfo | null>(null);

  const [activeChatId, setActiveChatId] = useState<string | undefined>(undefined);
  const [messages, setMessages] = useState<ChatMessage[]>([]);
  const [chatStatus, setChatStatus] = useState<string | null>(null);

  const [isLoadingInit, setIsLoadingInit] = useState(true);
  const [isLoadingMore, setIsLoadingMore] = useState(false);
  const [isLoadingMessages, setIsLoadingMessages] = useState(false);
  const [isSending, setIsSending] = useState(false);
  const [sidebarOpen, setSidebarOpen] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // ─── Initial load ─────────────────────────────────────────────────────────

  useEffect(() => {
    const init = async () => {
      try {
        const [vehicleList, chatsPaged, sub] = await Promise.all([
          vehicleService.getMyVehicles().catch(() => [] as Vehicle[]),
          chatService.getMyChats(1, PAGE_SIZE).catch(() => null),
          subscriptionService.getMySubscription().catch(() => null),
        ]);
        setVehicles(vehicleList);
        if (chatsPaged) {
          setChats(chatsPaged.items);
          setHasNextPage(chatsPaged.hasNextPage);
          setChatsPage(1);
        }
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

  // ─── Load more chats ──────────────────────────────────────────────────────

  const handleLoadMore = useCallback(async () => {
    if (isLoadingMore || !hasNextPage) return;
    setIsLoadingMore(true);
    try {
      const nextPage = chatsPage + 1;
      const result = await chatService.getMyChats(nextPage, PAGE_SIZE);
      setChats((prev) => [...prev, ...result.items]);
      setHasNextPage(result.hasNextPage);
      setChatsPage(nextPage);
    } finally {
      setIsLoadingMore(false);
    }
  }, [isLoadingMore, hasNextPage, chatsPage]);

  // ─── Load messages for active chat ───────────────────────────────────────

  const loadMessages = useCallback(async (chatId: string) => {
    setIsLoadingMessages(true);
    setError(null);
    try {
      const msgs = await chatService.getChatMessages(chatId);
      const chat = chats.find((c) => c.id === chatId);
      const enriched = msgs.map((msg, idx) => {
        if (msg.role === "User" && idx === 0) {
          if (chat?.mode === "FaultHelp") {
            return { ...msg, diagnosticsInput: parseDiagnosticsContent(msg.content) };
          }
          if (chat?.mode === "WorkClarification") {
            return { ...msg, workClarificationInput: parseWorkClarificationContent(msg.content) };
          }
          if (chat?.mode === "PartnerAdvice") {
            return { ...msg, partnerAdviceInput: parsePartnerAdviceContent(msg.content) };
          }
        }
        return msg;
      });
      setMessages(enriched);
      if (chat) setChatStatus(chat.status);
    } catch {
      setError(t("errors.loadFailed"));
    } finally {
      setIsLoadingMessages(false);
    }
  }, [chats, t]);

  const handleSelectChat = useCallback(async (chatId: string) => {
    setActiveChatId(chatId);
    const chat = chats.find((c) => c.id === chatId);
    if (chat) setSelectedMode(chat.mode);
    await loadMessages(chatId);
  }, [chats, loadMessages]);

  // ─── Delete chat ──────────────────────────────────────────────────────────

  const handleDeleteChat = useCallback(async (chatId: string) => {
    try {
      await chatService.deleteChat(chatId);
      setChats((prev) => prev.filter((c) => c.id !== chatId));
      if (activeChatId === chatId) {
        setActiveChatId(undefined);
        setMessages([]);
        setChatStatus(null);
      }
    } catch {
      setError(t("errors.unknown"));
    }
  }, [activeChatId, t]);

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
        const resolvedStatus = res.chatStatus ?? "Active";
        const newChatSummary: ChatSummary = {
          id: res.chatId,
          mode,
          status: resolvedStatus,
          title,
          vehicleId: selectedVehicleId,
          messageCount: res.initialAssistantReply ? 1 : 0,
          createdAt: new Date().toISOString(),
        };
        setChats((prev) => [newChatSummary, ...prev]);
        setActiveChatId(res.chatId);

        const initialMessages: ChatMessage[] = [];
        if (mode === "FaultHelp") {
          initialMessages.push({
            id: "user-initial",
            role: "User",
            content: "",
            isValid: true,
            createdAt: new Date().toISOString(),
            diagnosticsInput: modeInput as DiagnosticsInput,
          });
        }
        if (mode === "WorkClarification") {
          initialMessages.push({
            id: "user-initial",
            role: "User",
            content: "",
            isValid: true,
            createdAt: new Date().toISOString(),
            workClarificationInput: modeInput as WorkClarificationInput,
          });
        }
        if (mode === "PartnerAdvice") {
          initialMessages.push({
            id: "user-initial",
            role: "User",
            content: "",
            isValid: true,
            createdAt: new Date().toISOString(),
            partnerAdviceInput: modeInput as PartnerAdviceInput,
          });
        }
        if (res.initialAssistantReply) {
          initialMessages.push({
            id: "initial",
            role: "Assistant",
            content: res.initialAssistantReply,
            isValid: res.wasValid !== false,
            createdAt: new Date().toISOString(),
            diagnosticResultJson: res.diagnosticResultJson,
            workClarificationResultJson: res.workClarificationResultJson,
            partnerAdviceResultJson: res.partnerAdviceResultJson,
          });
        }
        setMessages(initialMessages);
        setChatStatus(resolvedStatus);
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
          diagnosticResultJson: res.diagnosticResultJson,
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
        hasNextPage={hasNextPage}
        isLoadingMore={isLoadingMore}
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
        onLoadMore={handleLoadMore}
        onDeleteChat={handleDeleteChat}
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
