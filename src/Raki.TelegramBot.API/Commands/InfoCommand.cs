using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Raki.TelegramBot.API.Commands
{
    public class InfoCommand : BotCustomCommand
    {
        public override string Name => "info";
        public override ParseMode Mode => ParseMode.MarkdownV2;
        public override Task<string> ProcessAsync(Message message)
        {
            return Task.FromResult("Хуинфо бля\\. Нет нихуя тут\\.");
        }

    }
}
