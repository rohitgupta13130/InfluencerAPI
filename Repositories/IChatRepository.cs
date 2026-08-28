using InfluencerAPI.Dtos;
using InfluencerAPI.Models;

namespace InfluencerAPI.Repositories
{
    public interface IChatRepository
    {
       
            Task SendMessage(ChatMessageRequest model);
            Task<List<ChatResponseRequest>> GetChat(int user1, int user2);
            Task MarkAsRead(int id);

    }
}
