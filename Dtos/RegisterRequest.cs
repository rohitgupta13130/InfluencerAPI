using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace InfluencerBackendAPI.Dtos
{
    public class RegisterRequest
    {
        [Required(ErrorMessage = "Full name is required")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Full name must be between 3 and 100 characters")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(150)]
        public string Email { get; set; }

        [Required(ErrorMessage = "Phone number is required")]
        [RegularExpression(@"^[0-9]{10}$", ErrorMessage = "Phone number must be 10 digits")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
        public string Password { get; set; }

        [Required(ErrorMessage = "User type is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Invalid user type")]
        public int UserTypeId { get; set; }

        // ✅ FILE VALIDATION (basic)
        [Required(ErrorMessage = "Profile image is required")]
        public IFormFile ProfileImage { get; set; }
    }
}