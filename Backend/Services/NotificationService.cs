using Microsoft.AspNetCore.SignalR;

public enum NotificationEvent
{
    NewMessage,
    ChatRequest,
    UserJoined,
    UserLeft,
    SystemAlert
}

public class NotificationService : INotificationService
{
    private readonly IHubContext<NotificationHub> _hubContext;

    public NotificationService(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task SendNotificationAsync(string userId, NotificationEvent eventName, string message, object data = null)
    {
        var connectionId = NotificationHub.GetConnectionId(userId);

        if (!string.IsNullOrEmpty(connectionId))
        {
            var notification = new Notification
            {
                EventName = eventName,
                Message = message,
                Data = data
            };

            // Send notification to the specific user
            await _hubContext.Clients.Client(connectionId).SendAsync("ReceiveNotification", notification);
        }
        else
        {
            Console.WriteLine($"User {userId} is not connected.");
        }
    }

}
