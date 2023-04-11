namespace Raki.TelegramBot.API.Commands;

using Microsoft.Extensions.Options;
using Raki.TegramBot.Core.Services;
using Raki.TelegramBot.API.Entities;
using Raki.TelegramBot.API.Models;
using Raki.TelegramBot.API.Services;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

public class EveryoneCommand : BotCustomCommand
{
    private readonly StorageService _storageService;
    private readonly MessageConstructor _messageConstructor;
    private readonly IOptions<AppConfigOptions> _appConfigOptions;

    public override string Name => "everyone";

    public EveryoneCommand(StorageService storageService, MessageConstructor messageConstructor, TelegramBot telegramBot,
        IOptions<AppConfigOptions> appConfigOptions) : base(telegramBot)
    {
        _storageService = storageService;
        _messageConstructor = messageConstructor;
        _appConfigOptions = appConfigOptions;
    }

    public override async Task<CommandResponse> ProcessAsync(Message message)
    {
        var commandResponse = new CommandResponse
        {
            Mode = ParseMode.Html
        };

        var partitionKey = message.Chat.Id.ToString();

        var activeSessions = await _storageService.GetActiveSessionsAsync(partitionKey);

        if (activeSessions.Count() >= _appConfigOptions.Value.ActiveSessionCap)
        {
            await TelegramBot.Client.SendTextMessageAsync(message.Chat.Id, "Максимальное количество активных сессий превышенно.",
                parseMode: commandResponse.Mode, replyMarkup:
                commandResponse.Keyboard,
                replyToMessageId: commandResponse.ReplyToId);

            return commandResponse;
        }

        var players = (await _storageService.GetPlayersAsync(partitionKey)).ToList();

        var playersWithUserName = players.Where(x => x.UserName != null).ToList();
        var playersWithName = players.Where(x => x.UserName == null).ToList();

        if (players.Any())
        {
            var sessionLtTime = DateTime.UtcNow.AddHours(2);
            var midnight = new DateTime(sessionLtTime.Year, sessionLtTime.Month, sessionLtTime.Day, 23, 59, 59, DateTimeKind.Utc);

            var newSession = new SessionRecordEntity
            {
                RowKey = Guid.NewGuid().ToString(),
                PartitionKey = partitionKey,
                SessionEnd = midnight,
                SessionStart = sessionLtTime,
                UniqueLetter = GetNextAvailableLetter(activeSessions.ToList()),
                SessionId = message.MessageId,
            };

            await _storageService.CreateSessionAsync(newSession);

            var replyMessage = await _messageConstructor.ConstructEveryoneMessageAsync(partitionKey, newSession);
            commandResponse.ResponseMessage = replyMessage;
            commandResponse.Keyboard = _messageConstructor.GetKeyboardMarkup(newSession.SessionId);

            var sessionMessage = await TelegramBot.Client.SendTextMessageAsync(message.Chat.Id, commandResponse.ResponseMessage,
                    parseMode: commandResponse.Mode, replyMarkup:
                    commandResponse.Keyboard,
                    replyToMessageId: commandResponse.ReplyToId);
        }
        else
        {
            commandResponse.ResponseMessage = "Юзеров нет в списке";
            await TelegramBot.Client.SendTextMessageAsync(message.Chat.Id, commandResponse.ResponseMessage,
                parseMode: commandResponse.Mode, replyMarkup:
                commandResponse.Keyboard,
                replyToMessageId: commandResponse.ReplyToId);
            return commandResponse;
        }

        return commandResponse;
    }

    private static string? GetNextAvailableLetter(List<SessionRecordEntity> sessions)
    {
        var englishAlphabet = Helper.GetEnglishAlphabet();
        var availableLetters = englishAlphabet.ToList();

        foreach (var session in sessions)
        {
            availableLetters.Remove(session.UniqueLetter);
        }

        return availableLetters.FirstOrDefault();
    }
}
