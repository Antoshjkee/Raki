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
        var currentUserSession = await _storageService.GetUserSessionAsync(partitionKey, sessionId, callbackQuery.From.Id);

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
        }
        else if (currentUserSession.IsPlus)
        {
            currentUserSession.IsPlus = false;
            await _storageService.UpdateUserSessionAsync(currentUserSession);


            var updatedMessage = await _messageConstructor.ConstructEveryoneMessageAsync(partitionKey, session);
            await _telegramBot.Client.EditMessageTextAsync(callbackQuery.Message.Chat.Id,
                session.SessionId + 1, updatedMessage, ParseMode.Html, replyMarkup: keyboard);
        }

        await _telegramBot.Client.AnswerCallbackQueryAsync(callbackQuery.Id);
        return response;
    }
}
