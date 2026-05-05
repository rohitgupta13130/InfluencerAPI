using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InfluencerBackendAPI.Models
{
    [Table("UserTypes", Schema = "influencer")]
    public class UserType
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "User type name is required")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Must be between 3 and 50 characters")]
        public string UserTypeName { get; set; }

        // Navigation property
        public ICollection<User> Users { get; set; } = new List<User>();
    }
}