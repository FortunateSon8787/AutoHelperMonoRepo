import { AxiosError } from "axios";
import { apiClient as api } from "@/lib/apiClient";
import type {
  ChatSummary,
  PagedResult,
  ChatMessage,
  CreateChatRequest,
  CreateChatResponse,
  SendMessageRequest,
  SendMessageResponse,
} from "@/types/chat";

// ─── Error Types ─────────────────────────────────────────────────────────────

export type ChatErrorCode =
  | "unauthorized"
  | "forbidden"
  | "notFound"
  | "conflict"
  | "badRequest"
  | "serverError"
  | "unknown";

export class ChatServiceError extends Error {
  constructor(
    public readonly code: ChatErrorCode,
    public readonly detail?: string
  ) {
    super(code);
    this.name = "ChatServiceError";
  }
}

function resolveErrorCode(error: unknown): ChatErrorCode {
  if (error instanceof AxiosError) {
    const status = error.response?.status;
    if (status === 401) return "unauthorized";
    if (status === 403) return "forbidden";
    if (status === 404) return "notFound";
    if (status === 409) return "conflict";
    if (status === 400) return "badRequest";
    if (status !== undefined && status >= 500) return "serverError";
  }
  return "unknown";
}

// ─── Chat Service ─────────────────────────────────────────────────────────────

export const chatService = {
  async getMyChats(page = 1, pageSize = 20): Promise<PagedResult<ChatSummary>> {
    try {
      const response = await api.get<PagedResult<ChatSummary>>("/api/chats", {
        params: { page, pageSize },
      });
      return response.data;
    } catch (error) {
      throw new ChatServiceError(resolveErrorCode(error));
    }
  },

  async deleteChat(chatId: string): Promise<void> {
    try {
      await api.delete(`/api/chats/${chatId}`);
    } catch (error) {
      throw new ChatServiceError(resolveErrorCode(error));
    }
  },

  async createChat(data: CreateChatRequest): Promise<CreateChatResponse> {
    try {
      const response = await api.post<CreateChatResponse>("/api/chats", data);
      return response.data;
    } catch (error) {
      throw new ChatServiceError(resolveErrorCode(error));
    }
  },

  async getChatMessages(chatId: string): Promise<ChatMessage[]> {
    try {
      const response = await api.get<ChatMessage[]>(`/api/chats/${chatId}/messages`);
      return response.data;
    } catch (error) {
      throw new ChatServiceError(resolveErrorCode(error));
    }
  },

  async sendMessage(chatId: string, data: SendMessageRequest): Promise<SendMessageResponse> {
    try {
      const response = await api.post<SendMessageResponse>(
        `/api/chats/${chatId}/messages`,
        data
      );
      return response.data;
    } catch (error) {
      throw new ChatServiceError(resolveErrorCode(error));
    }
  },
};
