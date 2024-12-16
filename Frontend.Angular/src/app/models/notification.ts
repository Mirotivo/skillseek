export enum NotificationEvent {
  NewMessage,
  ChatRequest,
  UserJoined,
  UserLeft,
  SystemAlert,
}
export interface Notification {
  eventName: NotificationEvent,
  message: string,
  data: any
}
