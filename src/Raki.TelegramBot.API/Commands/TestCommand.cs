namespace Raki.TelegramBot.API.Commands;

using Newtonsoft.Json;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
public class TestCommand : BotCustomCommand
{
    public override string Name => "test";
    public override Task<CommandResponse> ProcessAsync(Message message) => Task.FromResult(new CommandResponse
    {
        Mode = ParseMode.Html,
        ResponseMessage = JsonConvert.SerializeObject(message.From)
    });
}