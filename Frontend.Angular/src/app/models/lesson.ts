export enum LessonStatus
{
    Proposed,
    Booked,
    Completed,
    Canceled
}
export interface Lesson {
    id: number;
    topic: string; // e.g., "Math Basics"
    date: string; // ISO date string
    duration: string; // e.g., "1 hour"
    status: LessonStatus; // e.g., "Completed", "Upcoming"
  }
  