using Common.Models;

namespace Common.Interfaces
{
    public interface IChatServer
    {
        Task AddMessageToChat(string message);
        Task Subscribe();
        Task Unsubscribe();
    }
}
