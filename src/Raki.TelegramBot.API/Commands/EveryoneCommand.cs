namespace Raki.TelegramBot.API.Commands;

using Raki.TelegramBot.API.Entities;
using Raki.TelegramBot.API.Services;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

public class EveryoneCommand : BotCustomCommand
{
    private readonly StorageService _storageService;
    private readonly MessageConstructor _messageConstructor;
    private readonly TelegramBot _telegramBot;

    public override string Name => "everyone";

    public EveryoneCommand(StorageService storageService, MessageConstructor messageConstructor, TelegramBot telegramBot)
    {
        _storageService = storageService;
        _messageConstructor = messageConstructor;
        _telegramBot = telegramBot;
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

                var userTags = _messageConstructor.GetUserTags(players);
                commandResponse.ReplyToId = currentSession.SessionId + 1;
                commandResponse.ResponseMessage = "Сессия уже создана." + "\n" + userTags;
                commandResponse.ResponseMessage += await AddRespondedPlayersAsync(partitionKey, session.SessionId.ToString());
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

                var replyMessage = await _messageConstructor.ConstructEveryoneMessageAsync(partitionKey, session);
                commandResponse.ResponseMessage = replyMessage;

                var keyboard = new InlineKeyboardMarkup(new[]
                {
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("Плюс", $"plus-{session.SessionId}"),
                        InlineKeyboardButton.WithCallbackData("Минус", $"minus-{session.SessionId}")
                    },
                });

                commandResponse.Keyboard = keyboard;
            }
        }
        else
        {
            commandResponse.ResponseMessage = "Юзеров нет в списке";
        }


        await _telegramBot.Client.SendTextMessageAsync(message.Chat.Id, commandResponse.ResponseMessage,
            parseMode: commandResponse.Mode, replyMarkup:
            commandResponse.Keyboard,
            replyToMessageId: commandResponse.ReplyToId);

        return commandResponse;
    }

    private async Task<string> AddRespondedPlayersAsync(string partitionKey, string sessionId)
    {
        var result = string.Empty;

        var plusPlayersUserTags = await _messageConstructor.GetRespondedPlayersTagsAsync(partitionKey, sessionId, true);
        var minusPlayersUserTags = await _messageConstructor.GetRespondedPlayersTagsAsync(partitionKey, sessionId, false);

        if (!string.IsNullOrEmpty(plusPlayersUserTags))
        {
            //👍
            result += "\n\n" +
                $"Плюс {char.ConvertFromUtf32(0x1F44D)}" + "\n" +
                $"{plusPlayersUserTags}";
        }

        if (!string.IsNullOrEmpty(minusPlayersUserTags))
        {
            //👎
            result += "\n\n" +
                $"Минус {char.ConvertFromUtf32(0x1F44E)}" + "\n" +
                $"{minusPlayersUserTags}";
        }

        return result;
    }
}
