using Telegram.Bot.Types;

namespace Raki.TelegramBot.API.Commands
{
    public class MinusCommand : BotCustomCommand
    {
        public override string Name => "minus";

        public override Task<CommandResponse> ProcessAsync(Message message)
        {
            throw new NotImplementedException();
        }
    }
}
