using InfluencerBackendAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace InfluencerBackendAPI.Data
{
    public class ApplicationDbContext: DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options): base(options){ }

        public DbSet<User> Users { get; set; }

        public DbSet<UserType> UserTypes { get; set; }

        public DbSet<ChatMessage> ChatMessages { get; set; }
    }
}
