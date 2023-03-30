using Telegram.Bot.Types;

namespace Raki.TelegramBot.API.Commands;

public class SessionCommand : BotCustomCommand
{
    public override string Name => "session";

    public override Task<CommandResponse> ProcessAsync(Message message)
    {
        // start - ping everyone that has given PLUS to this session.
        throw new NotImplementedException();
    }
}
