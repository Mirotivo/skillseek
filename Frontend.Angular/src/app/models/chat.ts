import { Message } from "./message";


export interface Chat {
    id: number;
    listingId: number;
    studentId: number;
    recipientId: number;
    name: string;
    lastMessage: string;
    timestamp: string;
    details: string;
    messages: Message[];
    requestDetails: string;
    myRole: string;
  }
  