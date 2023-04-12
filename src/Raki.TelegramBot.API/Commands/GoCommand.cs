namespace Raki.TelegramBot.API.Commands;

using ChatGPT.Net;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Raki.TegramBot.Core.Services;
using Raki.TelegramBot.API.Entities;
using Raki.TelegramBot.API.Models;
using Raki.TelegramBot.API.Services;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

public class GoCommand : BotCustomCommand
{
    private readonly StorageService _storageService;
    private readonly MessageConstructor _messageConstructor;
    private readonly IOptions<AppConfigOptions> _appConfigOptions;
    private readonly ChatGpt _chatGpt;

    public override string Name => "go";

    public GoCommand(StorageService storageService, MessageConstructor messageConstructor, TelegramBot telegramBot,
        IOptions<AppConfigOptions> appConfigOptions, ChatGpt chatGpt) : base(telegramBot)
    {
        _storageService = storageService;
        _messageConstructor = messageConstructor;
        _appConfigOptions = appConfigOptions;
        _chatGpt = chatGpt;
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

        DateTime? suggestedTime = default;
        var sessionLtTime = DateTime.UtcNow.AddHours(2);
        var midnight = new DateTime(sessionLtTime.Year, sessionLtTime.Month, sessionLtTime.Day, 23, 59, 59, DateTimeKind.Utc);

        var chatGptResponse = await _chatGpt.Ask($@"
                Here is a format of your response : 
                {{
                    ""Time"" : ""2023-04-12T15:30:00"",
                    ""IsSuccess"" : true
                }}
        You should try to extact the time from the message I am going to provide you. 
        If its looks like incorrect time or message does not contain time at all, you should mark ""IsSuccess"" as false and ""Time"" field should be null. 
        Here is the message : {message.Text}");

        var charGtpObjectReponse = JsonConvert.DeserializeObject<ChatGptTimeResponse>(chatGptResponse);

        if (charGtpObjectReponse != null && charGtpObjectReponse.IsSuccess)
        {
            // TODO : Consider multiple times?
            var responseTime = charGtpObjectReponse.Time;

            if (responseTime != null)
            {
                suggestedTime = new DateTime(sessionLtTime.Year, sessionLtTime.Month, sessionLtTime.Day,
                    responseTime.Value.Hour, responseTime.Value.Minute, responseTime.Value.Second, DateTimeKind.Utc);
            }
        }

        var newSession = new SessionRecordEntity
        {
            RowKey = Guid.NewGuid().ToString(),
            PartitionKey = partitionKey,
            SessionEnd = midnight,
            SessionStart = suggestedTime,
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

        await TelegramBot.Client.PinChatMessageAsync(message.Chat.Id, sessionMessage.MessageId, disableNotification: true);
        await TelegramBot.Client.DeleteMessageAsync(message.Chat.Id, sessionMessage.MessageId + 1);

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
