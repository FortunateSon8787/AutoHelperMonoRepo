export interface ServiceRecord {
  id: string;
  vehicleId: string;
  title: string;
  description: string;
  performedAt: string; // ISO 8601
  cost: number;
  executorName: string;
  executorContacts: string | null;
  operations: string[];
  documentUrl: string;
}

export interface CreateServiceRecordRequest {
  title: string;
  description: string;
  performedAt: string; // ISO 8601 UTC
  cost: number;
  executorName: string;
  executorContacts: string | null;
  operations: string[];
  documentUrl: string;
}

export interface UpdateServiceRecordRequest {
  title: string;
  description: string;
  performedAt: string;
  cost: number;
  executorName: string;
  executorContacts: string | null;
  operations: string[];
}
