using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InfluencerBackendAPI.Models
{
    [Table("ChatMessages", Schema = "influencer")]
    public class ChatMessage
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "SenderId must be greater than 0")]
        public int SenderId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "ReceiverId must be greater than 0")]
        public int ReceiverId { get; set; }

        [Required(ErrorMessage = "Message is required")]
        [StringLength(500, ErrorMessage = "Message cannot exceed 500 characters")]
        public string Message { get; set; }

        [Required]
        public DateTime SentAt { get; set; } = DateTime.UtcNow; // ✅ default

        [Required]
        public bool IsRead { get; set; } = false; // ✅ default

        // 🔹 Foreign Keys
        [ForeignKey("SenderId")]
        public User Sender { get; set; }

        [ForeignKey("ReceiverId")]
        public User Receiver { get; set; }
    }
}