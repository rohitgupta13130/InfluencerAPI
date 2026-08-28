using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InfluencerAPI.Models
{
    [Table("InfluencerDashboard", Schema = "influencer")]
    public class InfluencerDashboard
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "UserId must be greater than 0")]
        public int UserId { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Earning cannot be negative")]
        public decimal Earning { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Followers cannot be negative")]
        public int Followers { get; set; }

        [Required(ErrorMessage = "Campaign is required")]
        [StringLength(100, ErrorMessage = "Campaign cannot exceed 100 characters")]
        public string Campaign { get; set; }

        [Required(ErrorMessage = "Engagement is required")]
        [StringLength(50, ErrorMessage = "Engagement cannot exceed 50 characters")]
        public string Engagement { get; set; }
    }
}