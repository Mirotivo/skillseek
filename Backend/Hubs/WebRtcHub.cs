using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

[MyCustom]
public class WebRtcHub : Hub
{
    private readonly ILogger<WebRtcHub> _logger;
    private readonly Dictionary<string, string> _users;

    public WebRtcHub(ILogger<WebRtcHub> logger, Dictionary<string, string> users)
    {
        _logger = logger;
        _users = users;
    }

    public async Task SendMessage(string user, string message)
    {
        await Clients.All.SendAsync("ReceiveMessage", user, message);
    }

    public async Task JoinRoom(string roomId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
        await Clients.Group(roomId).SendAsync("userJoined", Context.ConnectionId);
    }




    public async Task sendReady(object message)
    {
        await Clients.Others.SendAsync("ReceiveReady", message);
    }

    public async Task sendBye(object message)
    {
        await Clients.Others.SendAsync("ReceiveBye", message);
    }

    public async Task SendIceCandidate(object message)
    {
        await Clients.Others.SendAsync("ReceiveIceCandidate", message);
    }

    public async Task SendSdpOffer(object message)
    {
        await Clients.Others.SendAsync("ReceiveSdpOffer", message);
    }

    public async Task SendSdpAnswer(object message)
    {
        await Clients.Others.SendAsync("ReceiveSdpAnswer", message);
    }




    public async Task Login(string userName)
    {
        if (!_users.ContainsKey(userName))
        {
            _users.Add(userName, Context.ConnectionId);
        }
        else
        {
            _users[userName] = Context.ConnectionId;
        }
        await Clients.All.SendAsync("userLoggedIn", userName);
    }

    public async Task Logout(string userName)
    {
        _users.Remove(userName);
        Clients.All.SendAsync("userLoggedOut", userName);
    }

    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation($"Connecting : .....");
        try
        {
            var userName = Context.User.Identity.Name;
            userName = GetUserDisplayName(userName);
            if (userName != null)
            {
                await Login(userName);
            }
        }
        catch (System.Exception)
        {

        }
        finally
        {
            await base.OnConnectedAsync();
        }
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        _logger.LogInformation($"Disconnecting : .....");
        try
        {
            var userName = Context.User.Identity.Name;
            userName = GetUserDisplayName(userName);
            await Logout(userName);
        }
        catch (System.Exception)
        {

        }
        finally
        {
            await base.OnDisconnectedAsync(exception);
        }
    }

    public async Task GetConnectedUsers()
    {
        var otherUsers = _users.Keys.Where(value => value != Context.ConnectionId);
        await Clients.All.SendAsync("ConnectedUsers", otherUsers);
    }

    private string GetUserDisplayName(string userName)
    {
        // TODO: implement your logic to get the user's display name
        // For example, you could retrieve it from a database or compute it from the user name
        return userName;
    }

}
