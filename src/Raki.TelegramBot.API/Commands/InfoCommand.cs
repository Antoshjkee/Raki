namespace Raki.TelegramBot.API.Commands;

using Raki.TelegramBot.API.Services;
using Telegram.Bot.Types;

public class InfoCommand : BotCustomCommand
{
    public InfoCommand(TelegramBot telegramBot) : base(telegramBot)
    {

    }

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
