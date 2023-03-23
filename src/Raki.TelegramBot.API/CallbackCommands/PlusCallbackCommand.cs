using Raki.TelegramBot.API.Entities;
using Raki.TelegramBot.API.Services;
using Telegram.Bot.Types;

namespace Raki.TelegramBot.API.CallbackCommands
{
    public class PlusCallbackCommand : BotCustomCallbackCommand
    {
        private StorageService _storageService;

        public PlusCallbackCommand(StorageService storageService)
        {
            _storageService = storageService;
        }

        public override string Name => "plus";
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

            var currentSession = await _storageService.GetCurrentSessionAsync(callbackQuery.Message.Chat.Id.ToString());
            if (currentSession == null)
            {
                response.ResponseMessage = "Сейчас нет активной сессии";
                return response;
            }

            if (currentSession.SessionId != session.SessionId)
            {
                response.ResponseMessage = "Эта сессия устарела";
                return response;
            }

            var currentUserSession = await _storageService.GetUserSessionAsync(partitionKey, sessionId, callbackQuery.From.Id);

            if (currentUserSession == null)
            {
                var userSession = new PlayerSessionRecordEntity
                {
                    PartitionKey = partitionKey,
                    RowKey = Guid.NewGuid().ToString(),
                    SessionId = int.Parse(sessionId),
                    UserId = callbackQuery.From.Id,
                };

                await _storageService.AddUserToSession(userSession);

                // TODO : Construct message for /everyone command
            }

            return response;
        }
    }
}
