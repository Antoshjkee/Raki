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
    public override async Task<string> ProcessAsync(Message message)
    {
        if (message.From == null) return "Что-то пошло не так...";

        var isAdmin = await _telegramBot.IsAdminAsync(message.Chat.Id, message.From.Id);
        if (!isAdmin) return "У вас нет прав добавлять новых людей в список.";

        var userName = message.Text!.Replace($"/{Name}", string.Empty).Trim();
        var isValidUserName = _telegramBot.IsValidTelegramUsername(userName);

        if (!isValidUserName) return "Юзернэйм указан неверно";

        var existingUser = await _storageService.GetPlayerAsync(message.Chat.Id.ToString(), userName);
        if (existingUser != null) return $"Юзер '{userName}' уже добавлен в список";

        var playerRecord = new PlayerRecordEntity
        {
            PartitionKey = message.Chat.Id.ToString(),
            UserName = userName,
            RowKey = Guid.NewGuid().ToString()
        };

        await _storageService.AddPlayerAsync(playerRecord);

        return $"Юзера '{userName}' успешно добавили в список";
    }
}