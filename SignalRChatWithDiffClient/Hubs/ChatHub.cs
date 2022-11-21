using Common.Interfaces;
using Common.Models;
using Microsoft.AspNetCore.SignalR;
using System.Diagnostics;

namespace Server.Hubs
{
    public class ChatHub : Hub<IChatClient>, IChatServer
    {

        private readonly string subscribersGroupName = "Subscribers";
        public Task AddMessageToChat(string message)
        {
            var caller = Context.UserIdentifier; // implemented in CustomUserProvider : IUserIdProvider
            var messageForClient = ChatMessage.Create(caller, message);
            Console.WriteLine(message);
            return Clients.Others.SendClientMessageToChat(messageForClient);
        }

        public override async Task OnConnectedAsync()
        {
            await AddMessageToChat("joined chat");
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await AddMessageToChat("left chat");
        }

        public Task Subscribe()
        {
            return Groups.AddToGroupAsync(Context.ConnectionId, subscribersGroupName);
        }

        public Task Unsubscribe()
        {
            return Groups.RemoveFromGroupAsync(Context.ConnectionId, subscribersGroupName);
        }
    }
}
