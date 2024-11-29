export interface Review {
    name: string;
    subject: string;
    message?: string; // For pending reviews
    feedback?: string; // For received or sent reviews
    avatar?: string | null; // Optional avatar URL
  }
  