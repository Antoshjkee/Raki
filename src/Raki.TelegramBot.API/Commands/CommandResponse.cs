using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Raki.TelegramBot.API.Commands;

public class CommandResponse
{
    public string? ResponseMessage { get; set; }
    public ParseMode Mode { get; set; }
    public InlineKeyboardMarkup? Keyboard { get; set; }

    public CommandResponse()
    {
        Mode = ParseMode.Html;
    }
}