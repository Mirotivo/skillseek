export interface User {
  firstName: string;
  lastName: string;
  address: string;
  dob: string;
  email: string;
  phone: string;
  skypeId: string;
  hangoutId: string;
  profileVerified: string[]; // An array of verification methods like Email, Mobile
  lessonsCompleted: string; // Could be a duration string like '77h'
  evaluations: number; // The number of evaluations
  profileImage: string; // Path or URL to the profile image
  recommendationToken: string;
  paymentDetailsAvailable: boolean;
}
