using Common.Models;

namespace Common.Interfaces
{
    public interface IChatServer
    {
        Task AddMessageToChat(ChatMessage message);
    }
}
