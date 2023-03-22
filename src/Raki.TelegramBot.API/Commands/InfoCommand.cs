using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Raki.TelegramBot.API.Commands
{
    public class InfoCommand : BotCustomCommand
    {
        public override string Name => "info";
        public override Task<CommandResponse> ProcessAsync(Message message)
        {
            var commandResponse = new CommandResponse
            {
                ResponseMessage = "Хуинфо бля. Нет нихуя тут."
            };

            return Task.FromResult(commandResponse);
        }

    }
}
