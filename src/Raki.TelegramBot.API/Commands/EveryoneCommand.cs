namespace Raki.TelegramBot.API.Commands;

using ChatGPT.Net;
using Raki.TelegramBot.API.Services;
using Newtonsoft.Json;
using Raki.TelegramBot.API.Entities;
using Raki.TelegramBot.API.Models;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot;

public class EveryoneCommand : BotCustomCommand
{
    private readonly StorageService _storageService;
    private readonly MessageConstructor _messageConstructor;
    private readonly ChatGpt _chatGpt;
    private readonly TelegramBot _telegramBot;

    public override string Name => "everyone";

    public EveryoneCommand(StorageService storageService, MessageConstructor messageConstructor, ChatGpt chatGpt, TelegramBot telegramBot)
    {
        _storageService = storageService;
        _messageConstructor = messageConstructor;
        _chatGpt = chatGpt;
        _telegramBot = telegramBot;
    }
    public override async Task<CommandResponse> ProcessAsync(Message message)
    {
        var commandResponse = new CommandResponse
        {
            Mode = ParseMode.Html
        };

        //var chatGptResponse = await _chatGpt.Ask("Extract time from this message in JSON format with field name 'Time' which is going to be an array of strings, in ISO 8601 format. " +
        //     "Replace day, month, and year wirh default value" +
        //     $"and another boolean field name 'IsSuccess' to indicate extraction was successfull or not. And if multiple dates are presented," +
        //     $"write to another boolean field called 'IsMultipleDates' if this is the case. Here is message : {message.Text}");

        //var charGtpObjectReponse = JsonConvert.DeserializeObject<ChatGptTimeResponse>(chatGptResponse);

        //if (charGtpObjectReponse?.IsSuccess != null && charGtpObjectReponse.IsSuccess)
        //{
        //    if (charGtpObjectReponse.IsMultipleDates)
        //    {
        //        var pollQuestion = "Какой время лучше?";
        //        var pollOptions = charGtpObjectReponse.Time.Select(x => x.ToString("HH:mm"));

        //        await _telegramBot.Client.SendPollAsync(
        //            chatId: message.Chat.Id,
        //            question: pollQuestion,
        //            options: pollOptions,
        //            type: PollType.Regular,
        //            isAnonymous: true,
        //            allowsMultipleAnswers: false
        //        );
        //    }
        //    else
        //    {
        //        var chatGptTime = charGtpObjectReponse.Time.First();

        //        var currentDateTime = DateTime.Now;
        //        var newDateTime = new DateTime(currentDateTime.Year, currentDateTime.Month, currentDateTime.Day,
        //            chatGptTime.Hour, chatGptTime.Minute, default);

        //        commandResponse.ResponseMessage = $"Вы выбрали время на {newDateTime:F}";

        //        await _telegramBot.Client.SendTextMessageAsync(message.Chat.Id,
        //            commandResponse.ResponseMessage,
        //            parseMode: commandResponse.Mode,
        //            replyMarkup: commandResponse.Keyboard,
        //            replyToMessageId: commandResponse.ReplyToId);

        //        // TODO : REWRITE
        //        return commandResponse;
        //    }
        //}


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

        var responseMessage = await _telegramBot.Client.SendTextMessageAsync(message.Chat.Id,
            commandResponse.ResponseMessage,
            parseMode: commandResponse.Mode,
            replyMarkup: commandResponse.Keyboard,
            replyToMessageId: commandResponse.ReplyToId);

        await _telegramBot.Client.PinChatMessageAsync(message.Chat.Id, responseMessage.MessageId, disableNotification: true);
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
