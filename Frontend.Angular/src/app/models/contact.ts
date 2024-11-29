import { Message } from "./message";

export interface Contact {
    id: number;
    name: string;
    lastMessage: string;
    timestamp: string;
    details: string;
    messages: Message[];
    requestDetails: string;
    lessons: { date: string; time: string; price: string }[];
  }
  