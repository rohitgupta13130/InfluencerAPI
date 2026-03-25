namespace InfluencerAPI.Dtos
{
    public class DashboardRequest
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public decimal Earning { get; set; }
        public int Followers { get; set; }
        public string Campaign { get; set; }
        public string Engagement { get; set; }

    }
}
