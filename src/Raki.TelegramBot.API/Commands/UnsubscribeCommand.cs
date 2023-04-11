namespace Raki.TelegramBot.API.Commands;

using Raki.TelegramBot.API.Entities;
using Raki.TelegramBot.API.Services;
using Telegram.Bot.Types;

public class UnsubscribeCommand : BotCustomCommand
{
    private readonly StorageService _storageService;
    private readonly TelegramBot _telegramBot;

    public UnsubscribeCommand(StorageService storageService, TelegramBot telegramBot) : base(telegramBot)
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
                existingUser = await _storageService.GetPlayerAsync(message.Chat.Id.ToString(), userName);
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
            var existingUser = await _storageService.GetPlayerAsync(message.Chat.Id.ToString(), userName);

            if (existingUser == null)
            {
                commandResponse.ResponseMessage = $"Юзера '{userName}' нет в списке";
                return commandResponse;
            }

            await _storageService.DeletePlayerAsync(existingUser);
            commandResponse.ResponseMessage = $"Юзер '{userName}' удалён из списка";
            return commandResponse;
        }
    }
}
