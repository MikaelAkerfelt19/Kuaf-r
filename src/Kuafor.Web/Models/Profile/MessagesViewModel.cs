namespace Kuafor.Web.Models.Profile
{
    public class MessagesViewModel
    {
        public List<MessageItemViewModel> Messages { get; set; } = new();
    }

    public class MessageItemViewModel
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime? SentAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
