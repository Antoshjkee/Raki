namespace Raki.TelegramBot.API.Models;

public class ChatGptTimeResponse
{
    public ChatGptTimeResponse() => Time = Enumerable.Empty<DateTime>();
    public IEnumerable<DateTime> Time { get; set; }
    public bool IsSuccess { get; set; }
    public bool IsMultipleDates { get; set; }
}
