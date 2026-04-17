using InfluencerBackendAPI.Dtos;
using InfluencerBackendAPI.Models;

namespace InfluencerBackendAPI.Repositories
{
    public interface IUserRepository
    {
        Task<User> AuthenticateUser(string username, string password);
        Task<int> RegisterUser(RegisterRequest request);
        Task<List<User>> GetAllUsers();
    }
}
