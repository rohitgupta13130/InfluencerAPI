using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InfluencerBackendAPI.Models
{
    [Table("Users", Schema = "influencer")]
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Full name is required")]
        [StringLength(100, MinimumLength = 3)]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Username is required")]
        [StringLength(100, MinimumLength = 3)]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [StringLength(255)] // store hashed password
        public string Password { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress]
        [StringLength(150)]
        public string Email { get; set; }

        [Required(ErrorMessage = "User type is required")]
        [StringLength(50)]
        public string UserTypeName { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Invalid UserTypeId")]
        public int UserTypeId { get; set; }

        public DateTime? LastSeen { get; set; }

        [Required]
        public bool IsOnline { get; set; } = false;

        public string ProfileImage { get; set; }
    }
}