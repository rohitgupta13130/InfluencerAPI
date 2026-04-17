using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InfluencerBackendAPI.Models
{
    [Table("ChatMessages", Schema = "influencer")]
    public class ChatMessage
    {
        [Key]
        public int Id { get; set; }

        public int SenderId { get; set; }

        public int ReceiverId { get; set; }

        public string Message { get; set; }

        public DateTime SentAt { get; set; }

        public bool IsRead { get; set; }

        // 🔹 Navigation Properties (optional but good)
        public User Sender { get; set; }
        public User Receiver { get; set; }
    }
}