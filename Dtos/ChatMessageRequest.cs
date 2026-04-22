namespace InfluencerAPI.Dtos
{
    public class ChatMessageRequest
    {
        public int SenderId { get; set; }
        public int ReceiverId { get; set; }
        public string Message { get; set; }
    }
}
