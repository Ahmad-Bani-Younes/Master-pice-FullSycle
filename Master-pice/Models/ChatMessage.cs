namespace Master_pice.Models
{
    public class ChatMessage
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }

        public string Message { get; set; } // للنصوص
        public string? ImagePath { get; set; } // 🔥 صورة الشات

        public string SenderType { get; set; } // Admin أو User
        public DateTime SentAt { get; set; } = DateTime.Now;
    }

}
