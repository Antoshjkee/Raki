namespace Raki.TelegramBot.API.Commands;

using Raki.TelegramBot.API.Services;
using System.Collections.Concurrent;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

public class AllCommand : BotCustomCommand
{
    private readonly MessageConstructor _messageConstructor;
    private readonly StorageService _storageService;

    public AllCommand(TelegramBot telegramBot, MessageConstructor messageConstructor, StorageService storageService) : base(telegramBot)
    {
        _messageConstructor = messageConstructor;
        _storageService = storageService;
    }
    
    public override string Name => "all";

    public override async Task<CommandResponse> ProcessAsync(Message message)
    {
        var commandResponse = new CommandResponse
        {
            Mode = ParseMode.Html
        };

        var partitionKey = message.Chat.Id.ToString();

        var players = (await _storageService.GetPlayersAsync(partitionKey)).ToList();
        var userTags = _messageConstructor.GetUserTags(players);

        await TelegramBot.Client.SendTextMessageAsync(partitionKey,
           userTags, ParseMode.Html);

        return commandResponse;
    }
}
