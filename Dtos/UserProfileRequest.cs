using System.ComponentModel.DataAnnotations;

namespace InfluencerAPI.Dtos
{
    public class UserProfileRequest
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Id must be greater than 0")]
        public int Id { get; set; }

        [Required(ErrorMessage = "Full name is required")]
        [StringLength(100, MinimumLength = 3)]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Username is required")]
        [StringLength(100, MinimumLength = 3)]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "User type is required")]
        [StringLength(50)]
        public string UserTypeName { get; set; }

        public DateTime? LastSeen { get; set; }

        [Required]
        public bool IsOnline { get; set; }
    }
}