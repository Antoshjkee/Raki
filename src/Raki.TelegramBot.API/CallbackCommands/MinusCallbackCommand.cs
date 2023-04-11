namespace Raki.TelegramBot.API.CallbackCommands;

using Raki.TelegramBot.API.Entities;
using Raki.TelegramBot.API.Services;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

public class MinusCallbackCommand : BotCustomCallbackCommand
{
    private StorageService _storageService;
    private readonly MessageConstructor _messageConstructor;
    private readonly TelegramBot _telegramBot;

    public MinusCallbackCommand(StorageService storageService, MessageConstructor messageConstructor, TelegramBot telegramBot)
    {
        _storageService = storageService;
        _messageConstructor = messageConstructor;
        _telegramBot = telegramBot;
    }

    public override string Name => "minus";
    public override async Task<CallbackCommandResponse> ProcessAsync(CallbackQuery callbackQuery)
    {
        var response = new CallbackCommandResponse();

        var sessionId = callbackQuery.Data!.Split("-").Last();
        var partitionKey = callbackQuery.Message.Chat.Id.ToString();

        var session = await _storageService.GetSessionByIdAsync(partitionKey, sessionId);

        // TODO : Send messages inline instead of return response
        if (session == null)
        {
            response.ResponseMessage = "Этой сессии больше не существует";
            return response;
        }

        if (!session.IsActive)
        {
            response.ResponseMessage = $"Сессия '{session.UniqueLetter}' больше не активна.";
            return response;
        }

        var keyboard = _messageConstructor.GetKeyboardMarkup(session.SessionId);


        //var currentSession = await _storageService.GetCurrentSessionAsync(callbackQuery.Message.Chat.Id.ToString());
        //if (currentSession == null)
        //{
        //    response.ResponseMessage = "Сейчас нет активной сессии";
        //    return response;
        //}

        //if (currentSession.SessionId != session.SessionId)
        //{
        //    response.ResponseMessage = "Эта сессия устарела";
        //    return response;
        //}

        var currentUserSession = await _storageService.GetUserSessionAsync(partitionKey, sessionId, callbackQuery.From.Id);
        //var currentUser = await _storageService.GetPlayerByIdAsync(partitionKey, callbackQuery.From.Id);

        if (currentUserSession == null)
        {
            var userSession = new PlayerSessionRecordEntity
            {
                PartitionKey = partitionKey,
                RowKey = Guid.NewGuid().ToString(),
                SessionId = int.Parse(sessionId),
                UserId = callbackQuery.From.Id,
                IsPlus = false,
            };

            await _storageService.AddUserToSession(userSession);

            var updatedMessage = await _messageConstructor.ConstructEveryoneMessageAsync(partitionKey, session);
            await _telegramBot.Client.EditMessageTextAsync(callbackQuery.Message.Chat.Id,
                session.SessionId + 1, updatedMessage, ParseMode.Html, replyMarkup: keyboard);

            //response.ResponseMessage = $"'{_messageConstructor.ConstructUserTag(currentUser)}' дал минуса";
            //response.ResponseMessage += await AddRespondedPlayersAsync(partitionKey, sessionId);
        }
        else if (currentUserSession.IsPlus)
        {
            currentUserSession.IsPlus = false;
            await _storageService.UpdateUserSessionAsync(currentUserSession);


            var updatedMessage = await _messageConstructor.ConstructEveryoneMessageAsync(partitionKey, session);
            await _telegramBot.Client.EditMessageTextAsync(callbackQuery.Message.Chat.Id,
                session.SessionId + 1, updatedMessage, ParseMode.Html, replyMarkup: keyboard);

            //response.ResponseMessage = $"'{_messageConstructor.ConstructUserTag(currentUser)}' передумал и решил минусануть.";
            //response.ResponseMessage += await AddRespondedPlayersAsync(partitionKey, sessionId);
        }

        await _telegramBot.Client.AnswerCallbackQueryAsync(callbackQuery.Id);

        //if (response.ResponseMessage != null)
        //{
        //    await _telegramBot.Client.SendTextMessageAsync(callbackQuery.Message.Chat.Id,
        //        response.ResponseMessage,
        //        parseMode: response.Mode);
        //}

        return response;
    }

    //private async Task<string> AddRespondedPlayersAsync(string partitionKey, string sessionId)
    //{
    //    var result = string.Empty;

    //    var plusPlayersUserTags = await _messageConstructor.GetRespondedPlayersTagsAsync(partitionKey, sessionId, true);
    //    var minusPlayersUserTags = await _messageConstructor.GetRespondedPlayersTagsAsync(partitionKey, sessionId, false);

    //    if (!string.IsNullOrEmpty(plusPlayersUserTags))
    //    {
    //        //👍
    //        result += "\n\n" +
    //            $"Плюс {char.ConvertFromUtf32(0x1F44D)}" + "\n" +
    //            $"{plusPlayersUserTags}";
    //    }

    //    if (!string.IsNullOrEmpty(minusPlayersUserTags))
    //    {
    //        //👎
    //        result += "\n\n" +
    //            $"Минус {char.ConvertFromUtf32(0x1F44E)}" + "\n" +
    //            $"{minusPlayersUserTags}";
    //    }

    //    return result;
    //}
}
