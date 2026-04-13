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
  diagnosticResultJson?: string | null;
  workClarificationResultJson?: string | null;
  partnerAdviceResultJson?: string | null;
}

export interface SendMessageResponse {
  assistantReply: string;
  wasValid: boolean;
  responseStage?: string;
  chatStatus: ChatStatus;
  diagnosticResultJson?: string | null;
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

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  hasNextPage: boolean;
}

export interface ChatMessage {
  id: string;
  role: MessageRole;
  content: string;
  isValid: boolean;
  createdAt: string;
  diagnosticResultJson?: string | null;
  workClarificationResultJson?: string | null;
  partnerAdviceResultJson?: string | null;
  /** Structured input for the first FaultHelp user message — used to render a readonly form */
  diagnosticsInput?: DiagnosticsInput | null;
  /** Structured input for the first WorkClarification user message — used to render a readonly form */
  workClarificationInput?: WorkClarificationInput | null;
  /** Structured input for the first PartnerAdvice user message — used to render a readonly form */
  partnerAdviceInput?: PartnerAdviceInput | null;
}

// ─── Diagnostic Result (FaultHelp mode) ──────────────────────────────────────

export interface DiagnosticProblem {
  name: string;
  probability: number;
  possible_causes?: string | null;
  recommended_actions?: string | null;
}

export interface DiagnosticResult {
  response_stage: string;
  potential_problems?: DiagnosticProblem[] | null;
  urgency?: string | null;
  current_risks?: string | null;
  safe_to_drive?: boolean | null;
  suggested_partner_category?: string | null;
  disclaimer?: string | null;
}

// ─── Partner Advice Result (PartnerAdvice mode) ───────────────────────────────

export interface PartnerAdviceEntry {
  name: string;
  address?: string | null;
  distance_km: number;
  rating?: number | null;
  reviews_count?: number | null;
  is_open_now?: boolean | null;
  phone?: string | null;
  website?: string | null;
  services?: string | null;
  is_priority: boolean;
  has_warning: boolean;
}

export interface PartnerAdviceResult {
  /** Optional short summary / advice text shown above the partner list. */
  summary?: string | null;
  partners?: PartnerAdviceEntry[] | null;
}

// ─── Work Clarification Result (WorkClarification mode) ───────────────────────

export interface WorkClarificationResult {
  /** "low" | "medium" | "high" | "unclear" */
  work_reason_relevance: string;
  work_reason_explanation: string;
  /** "below_market" | "near_market" | "above_market" | "unknown" */
  labor_price_assessment: string;
  labor_price_explanation: string;
  /** "below_market" | "near_market" | "above_market" | "unknown" */
  parts_price_assessment: string;
  parts_price_explanation: string;
  /** "weak" | "normal" | "strong" | "unclear" */
  guarantee_assessment: string;
  guarantee_explanation: string;
  /** "poor" | "mixed" | "fair" | "good" | "unknown" */
  overall_honesty: string;
  overall_explanation: string;
  future_expectations?: string | null;
  disclaimer?: string | null;
}
