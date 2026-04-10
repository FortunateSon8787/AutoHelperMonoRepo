"use client";

import { useEffect, useRef, useState } from "react";
import {
  Send,
  Menu,
  Sparkles,
  Loader2,
  AlertCircle,
  Plus,
  CheckCircle2,
  Clock,
  Stethoscope,
  FileCheck,
} from "lucide-react";
import { useTranslations } from "next-intl";

import { Button } from "@/components/ui/button";
import { DiagnosticsForm } from "@/components/chat/DiagnosticsForm";
import { WorkClarificationForm } from "@/components/chat/WorkClarificationForm";
import { PartnerAdviceForm } from "@/components/chat/PartnerAdviceForm";
import { DiagnosticResultCard } from "@/components/chat/DiagnosticResultCard";
import { WorkClarificationResultCard } from "@/components/chat/WorkClarificationResultCard";
import type {
  ChatMode,
  ChatMessage,
  DiagnosticsInput,
  WorkClarificationInput,
  PartnerAdviceInput,
  DiagnosticResult,
  WorkClarificationResult,
} from "@/types/chat";

// ─── Props ────────────────────────────────────────────────────────────────────

interface ChatWindowProps {
  selectedMode: ChatMode;
  activeChatId: string | undefined;
  messages: ChatMessage[];
  chatStatus: string | null;
  isLoadingMessages: boolean;
  isSending: boolean;
  error: string | null;
  onMenuClick: () => void;
  onCreateChat: (
    mode: ChatMode,
    title: string,
    modeInput: DiagnosticsInput | WorkClarificationInput | PartnerAdviceInput
  ) => Promise<void>;
  onSendMessage: (content: string) => Promise<void>;
  onNewChat: () => void;
  onClearError: () => void;
}

// ─── Component ────────────────────────────────────────────────────────────────

export function ChatWindow({
  selectedMode,
  activeChatId,
  messages,
  chatStatus,
  isLoadingMessages,
  isSending,
  error,
  onMenuClick,
  onCreateChat,
  onSendMessage,
  onNewChat,
  onClearError,
}: ChatWindowProps) {
  const tWindow = useTranslations("chat.window");
  const tModes = useTranslations("chat.modes");
  const tSubtitles = useTranslations("chat.modeSubtitles");

  const [inputValue, setInputValue] = useState("");
  const messagesEndRef = useRef<HTMLDivElement>(null);
  const textareaRef = useRef<HTMLTextAreaElement>(null);

  // Auto-scroll to bottom
  useEffect(() => {
    messagesEndRef.current?.scrollIntoView({ behavior: "smooth" });
  }, [messages]);

  // Auto-resize textarea
  const handleTextareaInput = (e: React.ChangeEvent<HTMLTextAreaElement>) => {
    setInputValue(e.target.value);
    const el = e.target;
    el.style.height = "auto";
    el.style.height = `${Math.min(el.scrollHeight, 160)}px`;
    onClearError();
  };

  const handleSend = async () => {
    const content = inputValue.trim();
    if (!content || isSending) return;
    setInputValue("");
    if (textareaRef.current) {
      textareaRef.current.style.height = "auto";
    }
    await onSendMessage(content);
  };

  const handleKeyDown = (e: React.KeyboardEvent<HTMLTextAreaElement>) => {
    if (e.key === "Enter" && !e.shiftKey) {
      e.preventDefault();
      handleSend();
    }
  };

  // ─── Handle mode form submit ───────────────────────────────────────────────

  const handleDiagnosticsSubmit = async (data: DiagnosticsInput, title: string) => {
    await onCreateChat("FaultHelp", title, data);
  };

  const handleWorkClarificationSubmit = async (data: WorkClarificationInput, title: string) => {
    await onCreateChat("WorkClarification", title, data);
  };

  const handlePartnerAdviceSubmit = async (data: PartnerAdviceInput, title: string) => {
    await onCreateChat("PartnerAdvice", title, data);
  };

  // ─── Status badge ──────────────────────────────────────────────────────────

  const isCompleted = chatStatus === "Completed" || chatStatus === "FinalAnswerSent";
  const isAwaiting = chatStatus === "AwaitingUserAnswers";

  // ─── Render ──────────────────────────────────────────────────────────────

  return (
    <div className="flex flex-col h-full">
      {/* Top bar */}
      <div className="bg-card border-b border-border px-4 lg:px-6 py-4 flex-shrink-0">
        <div className="flex items-center gap-3">
          <button
            onClick={onMenuClick}
            className="lg:hidden p-2 hover:bg-secondary rounded-lg transition-colors"
          >
            <Menu className="w-5 h-5" />
          </button>
          <div className="flex-1 min-w-0">
            <div className="flex items-center gap-2">
              <h1 className="text-lg font-semibold text-foreground truncate">
                {tModes(selectedMode)}
              </h1>
              {isCompleted && (
                <span className="flex items-center gap-1 px-2 py-0.5 bg-success/10 text-success text-xs rounded-full">
                  <CheckCircle2 className="w-3 h-3" />
                  {tWindow("completedBadge")}
                </span>
              )}
              {isAwaiting && (
                <span className="flex items-center gap-1 px-2 py-0.5 bg-warning/10 text-warning text-xs rounded-full">
                  <Clock className="w-3 h-3" />
                  {tWindow("awaitingBadge")}
                </span>
              )}
            </div>
            <p className="text-xs text-muted-foreground mt-0.5">{tSubtitles(selectedMode)}</p>
          </div>
          {activeChatId && (
            <Button
              size="sm"
              variant="outline"
              onClick={onNewChat}
              className="gap-1.5 flex-shrink-0"
            >
              <Plus className="w-3.5 h-3.5" />
              {tWindow("newChatButton")}
            </Button>
          )}
        </div>
      </div>

      {/* Error banner */}
      {error && (
        <div className="px-4 lg:px-6 pt-3 flex-shrink-0">
          <div className="flex items-center gap-2 bg-destructive/5 border border-destructive/20 text-destructive rounded-xl px-4 py-3 text-sm">
            <AlertCircle className="w-4 h-4 flex-shrink-0" />
            {error}
          </div>
        </div>
      )}

      {/* Messages / Forms */}
      <div className="flex-1 overflow-y-auto px-4 lg:px-6 py-6">
        {isLoadingMessages ? (
          <div className="flex items-center justify-center h-full">
            <Loader2 className="w-6 h-6 animate-spin text-muted-foreground" />
          </div>
        ) : !activeChatId ? (
          /* Welcome + mode form */
          <div className="flex flex-col items-center gap-6 max-w-2xl mx-auto">
            <div className="text-center">
              <h2 className="text-xl font-semibold text-foreground mb-1">
                {tWindow("welcomeTitle")}
              </h2>
              <p className="text-sm text-muted-foreground">{tWindow("welcomeSubtitle")}</p>
            </div>

            {selectedMode === "FaultHelp" && (
              <DiagnosticsForm onSubmit={handleDiagnosticsSubmit} isLoading={isSending} />
            )}
            {selectedMode === "WorkClarification" && (
              <WorkClarificationForm onSubmit={handleWorkClarificationSubmit} isLoading={isSending} />
            )}
            {selectedMode === "PartnerAdvice" && (
              <PartnerAdviceForm onSubmit={handlePartnerAdviceSubmit} isLoading={isSending} />
            )}
          </div>
        ) : (
          /* Messages */
          <div className="space-y-5 max-w-4xl mx-auto">
            {messages.map((msg, idx) => (
              <ChatMessageBubble key={msg.id ?? idx} message={msg} aiLabel={tWindow("aiLabel")} invalidBadge={tWindow("invalidBadge")} />
            ))}
            {isSending && (
              <div className="flex justify-start">
                <div className="bg-secondary border border-border rounded-2xl px-5 py-4 max-w-lg">
                  <div className="flex items-center gap-2 mb-2">
                    <div className="flex items-center gap-1.5 px-2 py-1 bg-accent/10 rounded-md">
                      <Sparkles className="w-3 h-3 text-accent" />
                      <span className="text-xs font-medium text-accent">{tWindow("aiLabel")}</span>
                    </div>
                  </div>
                  <div className="flex gap-1">
                    <span className="w-2 h-2 bg-muted-foreground/50 rounded-full animate-bounce [animation-delay:0ms]" />
                    <span className="w-2 h-2 bg-muted-foreground/50 rounded-full animate-bounce [animation-delay:150ms]" />
                    <span className="w-2 h-2 bg-muted-foreground/50 rounded-full animate-bounce [animation-delay:300ms]" />
                  </div>
                </div>
              </div>
            )}
            <div ref={messagesEndRef} />
          </div>
        )}
      </div>

      {/* Input area — only shown when chat is active and not completed */}
      {activeChatId && !isCompleted && (
        <div className="bg-card border-t border-border px-4 lg:px-6 py-4 flex-shrink-0">
          <div className="max-w-4xl mx-auto">
            <div className="flex items-end gap-3">
              <div className="flex-1 bg-input border border-border rounded-xl overflow-hidden focus-within:ring-2 focus-within:ring-ring transition-all">
                <textarea
                  ref={textareaRef}
                  value={inputValue}
                  onChange={handleTextareaInput}
                  onKeyDown={handleKeyDown}
                  placeholder={tWindow("inputPlaceholder")}
                  rows={1}
                  disabled={isSending}
                  className="w-full px-4 py-3 text-sm text-foreground placeholder:text-muted-foreground resize-none focus:outline-none bg-transparent disabled:opacity-60"
                  style={{ minHeight: "44px", maxHeight: "160px" }}
                />
              </div>
              <button
                onClick={handleSend}
                disabled={!inputValue.trim() || isSending}
                className="p-3 bg-primary text-primary-foreground rounded-xl hover:opacity-90 transition-opacity shadow-sm disabled:opacity-40 disabled:cursor-not-allowed flex-shrink-0"
              >
                {isSending ? (
                  <Loader2 className="w-5 h-5 animate-spin" />
                ) : (
                  <Send className="w-5 h-5" />
                )}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}

// ─── Message Bubble ───────────────────────────────────────────────────────────

function DiagnosticsInputReadonly({
  input,
}: {
  input: NonNullable<ChatMessage["diagnosticsInput"]>;
}) {
  const t = useTranslations("chat.diagnosticsForm");

  return (
    <div className="flex justify-end">
      <div className="max-w-3xl w-full bg-card border border-border rounded-2xl px-5 py-4 shadow-sm">
        <div className="flex items-center gap-2 mb-4">
          <div className="w-7 h-7 bg-gradient-to-br from-sky-400 to-cyan-500 rounded-lg flex items-center justify-center">
            <Stethoscope className="w-3.5 h-3.5 text-white" />
          </div>
          <span className="text-xs font-medium text-muted-foreground">{t("title")}</span>
        </div>
        <div className="space-y-3">
          <div className="space-y-1">
            <p className="text-xs font-medium text-muted-foreground">{t("symptomsLabel")}</p>
            <div className="px-3 py-2 bg-secondary border border-border rounded-xl text-sm text-foreground leading-relaxed whitespace-pre-wrap">
              {input.symptoms}
            </div>
          </div>
          {input.recentEvents && (
            <div className="space-y-1">
              <p className="text-xs font-medium text-muted-foreground">{t("recentEventsLabel")}</p>
              <div className="px-3 py-2 bg-secondary border border-border rounded-xl text-sm text-foreground leading-relaxed whitespace-pre-wrap">
                {input.recentEvents}
              </div>
            </div>
          )}
          {input.previousIssues && (
            <div className="space-y-1">
              <p className="text-xs font-medium text-muted-foreground">{t("previousIssuesLabel")}</p>
              <div className="px-3 py-2 bg-secondary border border-border rounded-xl text-sm text-foreground leading-relaxed whitespace-pre-wrap">
                {input.previousIssues}
              </div>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}

function WorkClarificationInputReadonly({
  input,
}: {
  input: NonNullable<ChatMessage["workClarificationInput"]>;
}) {
  const t = useTranslations("chat.workClarificationForm");

  return (
    <div className="flex justify-end">
      <div className="max-w-3xl w-full bg-card border border-border rounded-2xl px-5 py-4 shadow-sm">
        <div className="flex items-center gap-2 mb-4">
          <div className="w-7 h-7 bg-gradient-to-br from-primary to-blue-600 rounded-lg flex items-center justify-center">
            <FileCheck className="w-3.5 h-3.5 text-white" />
          </div>
          <span className="text-xs font-medium text-muted-foreground">{t("title")}</span>
        </div>
        <div className="space-y-3">
          <div className="space-y-1">
            <p className="text-xs font-medium text-muted-foreground">{t("worksPerformedLabel")}</p>
            <div className="px-3 py-2 bg-secondary border border-border rounded-xl text-sm text-foreground leading-relaxed whitespace-pre-wrap">
              {input.worksPerformed}
            </div>
          </div>
          {input.workReason && (
            <div className="space-y-1">
              <p className="text-xs font-medium text-muted-foreground">{t("workReasonLabel")}</p>
              <div className="px-3 py-2 bg-secondary border border-border rounded-xl text-sm text-foreground leading-relaxed whitespace-pre-wrap">
                {input.workReason}
              </div>
            </div>
          )}
          {(input.laborCost > 0 || input.partsCost > 0) && (
            <div className="grid grid-cols-2 gap-3">
              {input.laborCost > 0 && (
                <div className="space-y-1">
                  <p className="text-xs font-medium text-muted-foreground">{t("laborCostLabel")}</p>
                  <div className="px-3 py-2 bg-secondary border border-border rounded-xl text-sm text-foreground">
                    {input.laborCost}
                  </div>
                </div>
              )}
              {input.partsCost > 0 && (
                <div className="space-y-1">
                  <p className="text-xs font-medium text-muted-foreground">{t("partsCostLabel")}</p>
                  <div className="px-3 py-2 bg-secondary border border-border rounded-xl text-sm text-foreground">
                    {input.partsCost}
                  </div>
                </div>
              )}
            </div>
          )}
          {input.guarantees && (
            <div className="space-y-1">
              <p className="text-xs font-medium text-muted-foreground">{t("guaranteesLabel")}</p>
              <div className="px-3 py-2 bg-secondary border border-border rounded-xl text-sm text-foreground leading-relaxed whitespace-pre-wrap">
                {input.guarantees}
              </div>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}

function ChatMessageBubble({
  message,
  aiLabel,
  invalidBadge,
}: {
  message: ChatMessage;
  aiLabel: string;
  invalidBadge: string;
}) {
  const isUser = message.role === "User";

  if (isUser) {
    if (message.diagnosticsInput) {
      return <DiagnosticsInputReadonly input={message.diagnosticsInput} />;
    }
    if (message.workClarificationInput) {
      return <WorkClarificationInputReadonly input={message.workClarificationInput} />;
    }
    return (
      <div className="flex justify-end">
        <div className="max-w-3xl bg-card border border-border rounded-2xl px-5 py-4 shadow-sm">
          <p className="text-sm text-foreground leading-relaxed whitespace-pre-wrap">
            {message.content}
          </p>
        </div>
      </div>
    );
  }

  const diagnosticResult: DiagnosticResult | null = (() => {
    if (!message.diagnosticResultJson) return null;
    try {
      return JSON.parse(message.diagnosticResultJson) as DiagnosticResult;
    } catch {
      return null;
    }
  })();

  const workClarificationResult: WorkClarificationResult | null = (() => {
    if (!message.workClarificationResultJson) return null;
    try {
      return JSON.parse(message.workClarificationResultJson) as WorkClarificationResult;
    } catch {
      return null;
    }
  })();

  return (
    <div className="flex justify-start">
      <div className="max-w-4xl w-full bg-secondary border border-border rounded-2xl overflow-hidden shadow-sm">
        <div className="px-5 py-4 space-y-4">
          <div className="flex items-center gap-2">
            <div className="flex items-center gap-1.5 px-2 py-1 bg-accent/10 rounded-md">
              <Sparkles className="w-3 h-3 text-accent" />
              <span className="text-xs font-medium text-accent">{aiLabel}</span>
            </div>
            {!message.isValid && (
              <span className="text-xs text-destructive bg-destructive/10 px-2 py-0.5 rounded-md">
                {invalidBadge}
              </span>
            )}
          </div>
          {diagnosticResult ? (
            <DiagnosticResultCard result={diagnosticResult} />
          ) : workClarificationResult ? (
            <WorkClarificationResultCard result={workClarificationResult} />
          ) : (
            <div className="text-sm text-foreground leading-relaxed whitespace-pre-wrap">
              {message.content}
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
