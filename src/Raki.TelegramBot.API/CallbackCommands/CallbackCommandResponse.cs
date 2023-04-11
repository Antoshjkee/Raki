namespace Raki.TelegramBot.API.CallbackCommands;
using Telegram.Bot.Types.Enums;

public class CallbackCommandResponse
{
    public ParseMode Mode { get; set; }
    public string? ResponseMessage { get; set; }
    public int? ReplyToId { get; set; }
    public int? MessageId { get; set; }

    public CallbackCommandResponse() => Mode = ParseMode.Html;
}

