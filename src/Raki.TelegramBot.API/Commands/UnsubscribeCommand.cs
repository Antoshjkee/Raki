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
        var commandResponse = new CommandResponse();

        if (message.From == null)
        {
            commandResponse.ResponseMessage = "Что-то пошло не так...";
            return commandResponse;
        }

        var isUnSubscribeOnSelf = message.Text!.Trim() == $"/{Name}";

        if (isUnSubscribeOnSelf)
        {
            var userName = message.From.Username;
            var existingUser = await _storageService.GetPlayerAsync(message.Chat.Id.ToString(), userName);
            if (existingUser == null)
            {
                commandResponse.ResponseMessage = $"Вас нет в списке";
                return commandResponse;
            }

            await _storageService.DeletePlayerAsync(message.Chat.Id.ToString(), existingUser.UserName);
            commandResponse.ResponseMessage = $"Вы удалили себя из списка";
            return commandResponse;
        }
        else
        {
            var userName = message.Text!.Replace($"/{Name}", string.Empty).Trim();
            var isValidUserName = _telegramBot.IsValidTelegramUsername(userName);

            if (!isValidUserName)
            {
                commandResponse.ResponseMessage = "Юзернэйм указан неверно";
                return commandResponse;
            }

            var isAdmin = await _telegramBot.IsAdminAsync(message.Chat.Id, message.From.Id);
            if (!isAdmin)
            {
                commandResponse.ResponseMessage = "У вас нет прав удалять людей из списка";
                return commandResponse;
            }

            var existingUser = await _storageService.GetPlayerAsync(message.Chat.Id.ToString(), userName);

            if (existingUser == null)
            {
                commandResponse.ResponseMessage = $"Юзера '{userName}' нет в списке";
                return commandResponse;
            }

            await _storageService.DeletePlayerAsync(message.Chat.Id.ToString(), existingUser.UserName);
            commandResponse.ResponseMessage = $"Юзер '{userName}' удалён из списка";
            return commandResponse;
        }
    }
}