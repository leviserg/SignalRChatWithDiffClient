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
            string userNameFromQuery = httpCtx.Request.Query["username"];
            string userNameFromHeaders = headers["username"];
            message.Caller = (!string.IsNullOrEmpty(userNameFromHeaders)) ? userNameFromHeaders : ((!string.IsNullOrEmpty(userNameFromQuery)) ? userNameFromQuery : "Anonymous");
            message.CreatedAt = DateTime.Now;
            return Clients.Others.SendClientMessageToChat(message);
        }

        public override async Task OnConnectedAsync()
        {
            var httpCtx = Context.GetHttpContext();
            var headers = httpCtx.Request.Headers;
            string userNameFromQuery = httpCtx.Request.Query["username"];
            string userNameFromHeaders = headers["username"];

            ChatMessage message = new ChatMessage
            {
                Caller = (!string.IsNullOrEmpty(userNameFromHeaders)) ? userNameFromHeaders : ((!string.IsNullOrEmpty(userNameFromQuery)) ? userNameFromQuery : "Anonymous"),//Context.ConnectionId,
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
