using Common.Interfaces;
using Common.Models;
using Microsoft.AspNetCore.SignalR;
using System.Diagnostics;

namespace SignalRChatWithDiffClient.Hubs
{
    public class ChatHub : Hub<IChatClient>, IChatServer
    {
        private const string userNameKey = "username";

        public Task AddMessageToChat(ChatMessage message)
        {
            var httpCtx = Context.GetHttpContext();
            var headers = httpCtx.Request.Headers;
            string userNameFromHeaders = headers["username"];
            message.Caller = userNameFromHeaders;
            message.CreatedAt = DateTime.Now;
            return Clients.Others.SendClientMessageToChat(message);
        }

        public override async Task OnConnectedAsync()
        {
            var httpCtx = Context.GetHttpContext();
            var headers = httpCtx.Request.Headers;
            string userNameFromHeaders = headers["username"];
            ChatMessage message = new ChatMessage
            {
                Caller = (userNameFromHeaders != null) ? userNameFromHeaders : Context.ConnectionId,
                Text = "joined chat"
            };
            await AddMessageToChat(message);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            ChatMessage message = new ChatMessage
            {
                Text = "left chat"
            };
            await AddMessageToChat(message);
        }
    }
}
