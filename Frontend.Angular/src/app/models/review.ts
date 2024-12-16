export interface Review {
    revieweeId: number;
    name: string;
    subject: string;
    feedback?: string; // For received or sent reviews
    avatar?: string | null; // Optional avatar URL
  }
  