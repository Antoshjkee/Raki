using Raki.TelegramBot.API.Services;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Raki.TelegramBot.API.Commands;

public class EveryoneCommand : BotCustomCommand
{
    private readonly StorageService _storageService;
    public override string Name => "everyone";

    public EveryoneCommand(StorageService storageService)
    {
        _storageService = storageService;
    }
    public override async Task<string> ProcessAsync(Message message)
    {
        var players = (await _storageService.GetPlayersAsync(message.Chat.Id.ToString())).ToList();

        if (players.Any())
        {
            return string.Join(' ', players.Select(x => x.UserName));
        }

        return "В списке нету ни одного юзера";
    }
}