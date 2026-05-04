using System.ComponentModel.DataAnnotations;

namespace InfluencerAPI.Dtos
{
    public class ChatMessageRequest
    {
        [Required(ErrorMessage = "SenderId is required")]
        [Range(1, int.MaxValue, ErrorMessage = "SenderId must be greater than 0")]
        public int SenderId { get; set; }

        [Required(ErrorMessage = "ReceiverId is required")]
        [Range(1, int.MaxValue, ErrorMessage = "ReceiverId must be greater than 0")]
        public int ReceiverId { get; set; }

        [Required(ErrorMessage = "Message is required")]
        [StringLength(500, MinimumLength = 1, ErrorMessage = "Message must be between 1 and 500 characters")]
        public string Message { get; set; }
    }
}