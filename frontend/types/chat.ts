// ─── Chat Enums ────────────────────────────────────────────────────────────────

export type ChatMode = "FaultHelp" | "WorkClarification" | "PartnerAdvice";
export type ChatStatus = "Active" | "AwaitingUserAnswers" | "FinalAnswerSent" | "Completed";
export type MessageRole = "User" | "Assistant";

// ─── Request DTOs ─────────────────────────────────────────────────────────────

export interface DiagnosticsInput {
  symptoms: string;
  recentEvents?: string;
  previousIssues?: string;
}

export interface WorkClarificationInput {
  worksPerformed: string;
  workReason: string;
  laborCost: number;
  partsCost: number;
  guarantees?: string;
}

export interface PartnerAdviceInput {
  request: string;
  lat: number;
  lng: number;
  urgency?: string;
}

export interface CreateChatRequest {
  mode: ChatMode;
  title: string;
  vehicleId?: string;
  diagnosticsInput?: DiagnosticsInput;
  workClarificationInput?: WorkClarificationInput;
  partnerAdviceInput?: PartnerAdviceInput;
  locale?: string;
}

export interface SendMessageRequest {
  content: string;
  locale?: string;
}

// ─── Response DTOs ────────────────────────────────────────────────────────────

export interface CreateChatResponse {
  chatId: string;
  initialAssistantReply?: string;
}

export interface SendMessageResponse {
  assistantReply: string;
  wasValid: boolean;
  responseStage?: string;
  chatStatus: ChatStatus;
}

export interface ChatSummary {
  id: string;
  mode: ChatMode;
  status: ChatStatus;
  title: string;
  vehicleId?: string;
  messageCount: number;
  createdAt: string;
}

export interface ChatMessage {
  id: string;
  role: MessageRole;
  content: string;
  isValid: boolean;
  createdAt: string;
}
