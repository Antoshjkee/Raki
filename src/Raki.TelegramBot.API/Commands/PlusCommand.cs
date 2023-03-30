using Telegram.Bot.Types;

namespace Raki.TelegramBot.API.Commands
{
    public class PlusCommand : BotCustomCommand
    {
        public PlusCommand()
        {
        }

        public override string Name => "plus";

        public override Task<CommandResponse> ProcessAsync(Message message)
        {
            throw new NotImplementedException();
        }
    }
  
}
