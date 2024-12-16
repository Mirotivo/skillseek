public interface INotificationService
{
    Task SendNotificationAsync(string userId, NotificationEvent eventName, string message, object data = null);
}
