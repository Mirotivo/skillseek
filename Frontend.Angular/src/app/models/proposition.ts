export interface Proposition {
    // id: number;
    date: string; // ISO 8601 date string
    duration: number; // "HH:mm:ss" format for TimeSpan
    price: number; // e.g., 30.00
    listingId: number;
    studentId: number | null;
  }
  