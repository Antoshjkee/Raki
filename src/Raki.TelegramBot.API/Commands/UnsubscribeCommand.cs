using Raki.TelegramBot.API.Entities;
using Raki.TelegramBot.API.Services;
using Telegram.Bot.Types;

namespace Raki.TelegramBot.API.Commands;

public class UnsubscribeCommand : BotCustomCommand
{
    private readonly StorageService _storageService;
    private readonly Services.TelegramBot _telegramBot;

    public UnsubscribeCommand(StorageService storageService, Services.TelegramBot telegramBot)
    {
        _storageService = storageService;
        _telegramBot = telegramBot;
    }

    public override string Name => "unsubscribe";
    public override async Task<CommandResponse> ProcessAsync(Message message)
    {
        var commandResponse = new CommandResponse
        {
            ReplyToId = message.MessageId
        };

        if (message.From == null)
        {
            commandResponse.ResponseMessage = "Что-то пошло не так...";
            return commandResponse;
        }

        var isUnSubscribeOnSelf = message.Text!.Trim() == $"/{Name}";

        if (isUnSubscribeOnSelf)
        {
            var existingUser = default(PlayerRecordEntity);

            var userName = message.From.Username;
            if (userName == null)
            {
                existingUser = await _storageService.GetPlayerByIdAsync(message.Chat.Id.ToString(), message.From.Id);
                if (existingUser == null)
                {
                    commandResponse.ResponseMessage = $"Вас нет в списке";
                    return commandResponse;
                }
            }
            else
            {
                existingUser = await _storageService.GetPlayerByUserNameAsync(message.Chat.Id.ToString(), userName);
                if (existingUser == null)
                {
                    commandResponse.ResponseMessage = $"Вас нет в списке";
                    return commandResponse;
                }
            }

            await _storageService.DeletePlayerAsync(existingUser);
            commandResponse.ResponseMessage = $"Вы удалили себя из списка";
            return commandResponse;
        }
        else
        {
            var isAdmin = await _telegramBot.IsAdminAsync(message.Chat.Id, message.From.Id);
            if (!isAdmin)
            {
                commandResponse.ResponseMessage = "У вас нет прав удалять людей из списка. Только себя.";
                return commandResponse;
            }

            var userName = message.Text!.Replace($"/{Name}", string.Empty).Trim();
            var isValidUserName = _telegramBot.IsValidTelegramUsername(userName);

            var existingUser = !isValidUserName
                ? await _storageService.GetPlayerByFirsNameAsync(message.Chat.Id.ToString(), userName)
                : await _storageService.GetPlayerByUserNameAsync(message.Chat.Id.ToString(), userName);

            if (existingUser == null)
            {
                commandResponse.ResponseMessage = $"Юзера '{userName}' нет в списке";
                return commandResponse;
            }

            await _storageService.DeletePlayerAsync(message.Chat.Id.ToString(), existingUser.Id);
            commandResponse.ResponseMessage = $"Юзер '{userName}' удалён из списка";
            return commandResponse;
        }
    }
}