using System.ComponentModel.DataAnnotations;

namespace InfluencerBackendAPI.Dtos
{
    public class LoginRequest
    {
        [Required(ErrorMessage = "Username is required")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 100 characters")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 5, ErrorMessage = "Password must be at least 5 characters")]
        public string Password { get; set; }
    }
}