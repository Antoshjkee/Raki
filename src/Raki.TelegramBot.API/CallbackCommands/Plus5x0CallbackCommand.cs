namespace Raki.TelegramBot.API.CallbackCommands;
using Raki.TelegramBot.API.Entities;
using Raki.TelegramBot.API.Services;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

public class Plus5x0CallbackCommand : BotCustomCallbackCommand
{
    private StorageService _storageService;
    private readonly MessageConstructor _messageConstructor;
    private readonly TelegramBot _telegramBot;

    public Plus5x0CallbackCommand(StorageService storageService, MessageConstructor messageConstructor, TelegramBot telegramBot)
    {
        _storageService = storageService;
        _messageConstructor = messageConstructor;
        _telegramBot = telegramBot;
    }

    public override string Name => "plus5x0";
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

        var messageId = session.SessionId + 1;
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
                IsPlus5x0 = true,
                IsPlus5x5 = false,
            };

            await _storageService.AddUserToSession(userSession);

            var updatedMessage = await _messageConstructor.ConstructEveryoneMessageAsync(partitionKey, session);
            await _telegramBot.Client.EditMessageTextAsync(callbackQuery.Message.Chat.Id,
                messageId, updatedMessage, ParseMode.Html, replyMarkup: keyboard);
        }
        else if (!currentUserSession.IsPlus5x0)
        {
            currentUserSession.IsPlus5x0 = true;
            await _storageService.UpdateUserSessionAsync(currentUserSession);

            var updatedMessage = await _messageConstructor.ConstructEveryoneMessageAsync(partitionKey, session);
            await _telegramBot.Client.EditMessageTextAsync(callbackQuery.Message.Chat.Id,
                messageId, updatedMessage, ParseMode.Html, replyMarkup: keyboard);
        }

        await _telegramBot.Client.AnswerCallbackQueryAsync(callbackQuery.Id);

        return response;
    }
}
