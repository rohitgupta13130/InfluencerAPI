namespace InfluencerBackendAPI.Models
{
    public class UserType
    {
        public int Id { get; set; }

        public string UserTypeName { get; set; }

        public ICollection<User> Users { get; set; }
    }
}
