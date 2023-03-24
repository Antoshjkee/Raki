using Azure;
using Raki.TelegramBot.API.Commands;
using Raki.TelegramBot.API.Entities;
using Raki.TelegramBot.API.Services;
using System.Collections.Concurrent;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Raki.TelegramBot.API.CallbackCommands
{
    public class PlusCallbackCommand : BotCustomCallbackCommand
    {
        private StorageService _storageService;
        private readonly MessageConstructor _messageConstructor;
        private readonly Services.TelegramBot _telegramBot;

        public PlusCallbackCommand(StorageService storageService, MessageConstructor messageConstructor, Services.TelegramBot telegramBot)
        {
            _storageService = storageService;
            _messageConstructor = messageConstructor;
            _telegramBot = telegramBot;
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
            var currentUser = await _storageService.GetPlayerByIdAsync(partitionKey, callbackQuery.From.Id);

            if (currentUserSession == null)
            {
                var userSession = new PlayerSessionRecordEntity
                {
                    PartitionKey = partitionKey,
                    RowKey = Guid.NewGuid().ToString(),
                    SessionId = int.Parse(sessionId),
                    UserId = callbackQuery.From.Id,
                    IsPlus = true,
                };

                await _storageService.AddUserToSession(userSession);

                response.ResponseMessage = $"'{_messageConstructor.ConstructUserTag(currentUser)}' дал плюса";
                response.ResponseMessage += await AddRespondedPlayersAsync(partitionKey, sessionId);
            }
            else if (!currentUserSession.IsPlus)
            {
                currentUserSession.IsPlus = true;
                await _storageService.UpdateUserSessionAsync(currentUserSession);

                response.ResponseMessage = $"'{_messageConstructor.ConstructUserTag(currentUser)}' передумал и решил плюсануть.";
                response.ResponseMessage += await AddRespondedPlayersAsync(partitionKey, sessionId);
            }

            await _telegramBot.Client.AnswerCallbackQueryAsync(callbackQuery.Id);

            if (response.ResponseMessage != null)
            {
                await _telegramBot.Client.SendTextMessageAsync(callbackQuery.Message.Chat.Id,
                    response.ResponseMessage,
                    parseMode: response.Mode);
            }

            return response;
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
}
