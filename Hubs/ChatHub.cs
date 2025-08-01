using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.SignalR;

namespace LoginRegistrationMVC.Hubs
{
    public class ChatHub : Hub
    {
        private static readonly ConcurrentDictionary<string, string> _userConnections = new ConcurrentDictionary<string, string>();

        public override Task OnConnected()
        {
            string userName = HttpContext.Current?.User?.Identity?.Name ?? "Anonymous";
            if (!string.IsNullOrEmpty(userName))
            {
                _userConnections.AddOrUpdate(userName, Context.ConnectionId, (key, oldValue) => Context.ConnectionId);
                Clients.Caller.updateUserName(userName);
            }
            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            string userName = HttpContext.Current?.User?.Identity?.Name ?? "Anonymous";
            _userConnections.TryRemove(userName, out _);
            return base.OnDisconnected(stopCalled);
        }

        public void SendMessage(string message)
        {
            string userName = HttpContext.Current?.User?.Identity?.Name ?? "Anonymous";
            Clients.All.broadcastMessage(userName, message);
        }

        public void SendPrivateMessage(string toUserName, string message)
        {
            string fromUser = HttpContext.Current?.User?.Identity?.Name ?? "Anonymous";
            if (_userConnections.TryGetValue(toUserName, out string toConnectionId))
            {
                Clients.Client(toConnectionId).receivePrivateMessage(fromUser, message);
                Clients.Caller.receivePrivateMessage(fromUser, message); 
            }
            else
            {
                Clients.Caller.sendError("User " + toUserName + " is not connected.");
            }
        }
    }
}