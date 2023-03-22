using Raki.TelegramBot.API.Entities;
using Raki.TelegramBot.API.Services;
using Telegram.Bot.Types;

namespace Raki.TelegramBot.API.Commands;

public class SubscribeCommand : BotCustomCommand
{
    private readonly Services.TelegramBot _telegramBot;
    private readonly StorageService _storageService;

    public SubscribeCommand(Services.TelegramBot telegramBot, StorageService storageService)
    {
        _telegramBot = telegramBot;
        _storageService = storageService;
    }

    public override string Name => "subscribe";
    public override async Task<CommandResponse> ProcessAsync(Message message)
    {
        var commandResponse = new CommandResponse();

        if (message.From == null)
        {
            commandResponse.ResponseMessage = "Что-то пошло не так...";
            return commandResponse;
        }

        var isSubscribeOnSelf = message.Text!.Trim() == $"/{Name}";

        if (isSubscribeOnSelf)
        {
            var userName = message.From.Username;
            var existingUser = await _storageService.GetPlayerAsync(message.Chat.Id.ToString(), userName);
            if (existingUser != null)
            {
                commandResponse.ResponseMessage = $"Юзер '{userName}' уже добавлен в список";
                return commandResponse;
            }

            var playerRecord = new PlayerRecordEntity
            {
                PartitionKey = message.Chat.Id.ToString(),
                UserName = userName,
                RowKey = Guid.NewGuid().ToString()
            };

            await _storageService.AddPlayerAsync(playerRecord);

            commandResponse.ResponseMessage = $"Вы успешно добавили себя в список";
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
                commandResponse.ResponseMessage = "У вас нет прав добавлять новых людей в список";
                return commandResponse;
            }

            var existingUser = await _storageService.GetPlayerAsync(message.Chat.Id.ToString(), userName);
            if (existingUser != null)
            {
                commandResponse.ResponseMessage = $"Юзер '{userName}' уже добавлен в список";
                return commandResponse;
            }

            var playerRecord = new PlayerRecordEntity
            {
                PartitionKey = message.Chat.Id.ToString(),
                UserName = userName,
                RowKey = Guid.NewGuid().ToString()
            };

            await _storageService.AddPlayerAsync(playerRecord);

            commandResponse.ResponseMessage = $"Юзера '{userName}' успешно добавили в список";
        }

        return commandResponse;
    }
}