namespace Raki.TelegramBot.API.Models
{
    public class ChatGptTimeResponse
    {
        public DateTime? Time { get; set; }
        public bool IsSuccess { get; set; }
        public bool IsMultipleDates { get; set; }
    }
}
