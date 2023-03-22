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

        if (players.Any())
        {
            commandResponse.ResponseMessage = string.Join(' ', players.Select(x => x.UserName));
            return commandResponse;
        }

        var inlineKeyboard = new InlineKeyboardMarkup(new[]
        {
            new [] { InlineKeyboardButton.WithCallbackData("+", "plus") },
            new [] { InlineKeyboardButton.WithCallbackData("-", "minus") },
        });

        commandResponse.Keyboard = inlineKeyboard;

        return commandResponse;
    }
}