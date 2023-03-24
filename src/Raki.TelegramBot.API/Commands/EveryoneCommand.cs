using Microsoft.Extensions.Options;
using Raki.TelegramBot.API.Entities;
using Raki.TelegramBot.API.Models;
using Raki.TelegramBot.API.Services;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Raki.TelegramBot.API.Commands;

public class EveryoneCommand : BotCustomCommand
{
    private readonly StorageService _storageService;
    private readonly IOptions<TimezoneOptions> _timeZoneOptions;
    private readonly MessageConstructor _messageConstructor;

    public override string Name => "everyone";

    public EveryoneCommand(StorageService storageService, IOptions<TimezoneOptions> timeZoneOptions, MessageConstructor messageConstructor)
    {
        _storageService = storageService;
        _timeZoneOptions = timeZoneOptions;
        _messageConstructor = messageConstructor;
    }
    public override async Task<CommandResponse> ProcessAsync(Message message)
    {
        var commandResponse = new CommandResponse
        {
            Mode = ParseMode.Html
        };

        var partitionKey = message.Chat.Id.ToString();
        var players = (await _storageService.GetPlayersAsync(partitionKey)).ToList();

        var playersWithUserName = players.Where(x => x.UserName != null).ToList();
        var playersWithName = players.Where(x => x.UserName == null).ToList();

        if (players.Any())
        {
            var session = default(SessionRecordEntity);

            var currentSession = await _storageService.GetCurrentSessionAsync(message.Chat.Id.ToString());

            if (currentSession != null)
            {
                session = currentSession;
            }
            else
            {
                var sessionLtTime = DateTime.UtcNow.AddHours(2);
                var newSession = new SessionRecordEntity
                {
                    RowKey = Guid.NewGuid().ToString(),
                    PartitionKey = partitionKey,
                    SessionStart = sessionLtTime,
                    SessionEnd = sessionLtTime.AddHours(1),
                    SessionId = message.MessageId,
                };

                await _storageService.CreateSessionAsync(newSession);
                session = newSession;
            }

            var replyMessage = await _messageConstructor.ConstructEveryoneMessageAsync(partitionKey, session);
            commandResponse.ResponseMessage = replyMessage;

            var keyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Плюс", $"plus-{session.SessionId}"),
                    InlineKeyboardButton.WithCallbackData("Минус", $"minus-{session.SessionId}")
                },
            }); ;

            commandResponse.Keyboard = keyboard;
        }
        else
        {
            commandResponse.ResponseMessage = "Юзеров нет в списке";
        }

        return commandResponse;
    }
}