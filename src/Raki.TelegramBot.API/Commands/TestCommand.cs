using Newtonsoft.Json;
using Raki.TelegramBot.API.Commands;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

public class TestCommand : BotCustomCommand
{
    public override string Name => "test";
    public override  Task<CommandResponse> ProcessAsync(Message message)
    {
      return Task.FromResult(new CommandResponse
      {
          Mode = ParseMode.Html,
          ResponseMessage = JsonConvert.SerializeObject(message.From)
      });
    }
}