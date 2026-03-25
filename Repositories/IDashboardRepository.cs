using InfluencerAPI.Dtos;

namespace InfluencerAPI.Repositories
{
    public interface IDashboardRepository
    {

        Task<DashboardRequest> GetDashboardByUserId(int userId);
    }
}
