using InfluencerAPI.Dtos;
using InfluencerBackendAPI.Data;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace InfluencerAPI.Repositories
{
    public class DashboardRepository : IDashboardRepository
    {
      
           private readonly ApplicationDbContext _context;

        public DashboardRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<DashboardRequest> GetDashboardByUserId(int userId)
        {
            DashboardRequest dashboard = null;

            using (var conn = _context.Database.GetDbConnection())
            {
                await conn.OpenAsync();

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "influencer.GetDashboardByUserId";
                    cmd.CommandType = CommandType.StoredProcedure;

                    var param = cmd.CreateParameter();
                    param.ParameterName = "@UserId";
                    param.Value = userId;
                    cmd.Parameters.Add(param);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            dashboard = new DashboardRequest
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                UserId = Convert.ToInt32(reader["UserId"]),
                                Earning = Convert.ToDecimal(reader["Earning"]),
                                Followers = Convert.ToInt32(reader["Followers"]),
                                Campaign = reader["Campaign"].ToString(),
                                Engagement = reader["Engagement"].ToString()
                            };
                        }
                    }
                }
            }

            return dashboard;
        }
    
    }
}
