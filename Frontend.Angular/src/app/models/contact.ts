import { Message } from "./message";

export interface Contact {
    id: number;
    studentId: number;
    name: string;
    lastMessage: string;
    timestamp: string;
    details: string;
    messages: Message[];
    requestDetails: string;
  }
  