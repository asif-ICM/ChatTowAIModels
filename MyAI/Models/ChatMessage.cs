namespace MyAI.Models
{
    
    public class MyChatMessage
    {
        public string Message { get; set; } = string.Empty;
        public bool IsUserMessage { get; set; }
        public DateTime Timestamp { get; set; }
    }
}

