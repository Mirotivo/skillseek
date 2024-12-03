export interface Proposition {
    id: number;
    date: string; // ISO date string
    duration: string; // e.g., "1 hour"
    price: number; // e.g., 30.00
    status: string; // e.g., "Pending", "Accepted"
  }
  