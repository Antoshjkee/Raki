using Raki.TelegramBot.API.Services;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Raki.TelegramBot.API.Commands;

public class EveryoneCommand : BotCustomCommand
{
    private readonly StorageService _storageService;
    public override string Name => "everyone";

    public EveryoneCommand(StorageService storageService)
    {
        _storageService = storageService;
    }
    public override async Task<CommandResponse> ProcessAsync(Message message)
    {
        var commandResponse = new CommandResponse
        {
            Mode = ParseMode.Html

        };
        var players = (await _storageService.GetPlayersAsync(message.Chat.Id.ToString())).ToList();

        var playersWithUserName = players.Where(x => x.UserName != null).ToList();
        var playersWithName = players.Where(x => x.UserName == null).ToList();

        if (players.Any())
        {
            var resultList = playersWithUserName.Select(x => $"@{x.UserName}").ToList();
            resultList.AddRange(playersWithName.Select(x => $"<a href=\"tg://user?id={x.Id}\">{x.FirstName}</a>"));

            commandResponse.ResponseMessage = string.Join(' ', resultList);
        }
        else
        {
            commandResponse.ResponseMessage = "Юзеров нет в списке";
        }

        //add telegram buttons
        var keyboard = new InlineKeyboardMarkup(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("Плюс", "subscribe"),
                InlineKeyboardButton.WithCallbackData("Минус", "unsubscribe")
            },
        });

        commandResponse.Keyboard = keyboard;
    

        return commandResponse;
    }
}