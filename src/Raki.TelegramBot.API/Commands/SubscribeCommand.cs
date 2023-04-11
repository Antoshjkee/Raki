namespace Raki.TelegramBot.API.Commands;
using Raki.TelegramBot.API.Entities;
using Raki.TelegramBot.API.Services;
using Telegram.Bot;
using Telegram.Bot.Types;

public class SubscribeCommand : BotCustomCommand
{
    private readonly TelegramBot _telegramBot;
    private readonly StorageService _storageService;

    public SubscribeCommand(TelegramBot telegramBot, StorageService storageService) : base(telegramBot)
    {
        _telegramBot = telegramBot;
        _storageService = storageService;
    }

    public override string Name => "subscribe";
    public override async Task<CommandResponse> ProcessAsync(Message message)
    {
        var commandResponse = new CommandResponse
        {
            ReplyToId = message.MessageId
        };

        if (message.From == null)
        {
            commandResponse.ResponseMessage = "Что-то пошло не так...";
            await SendMessageAsync(commandResponse, message.Chat.Id);
            return commandResponse;
        }

        var isSubscribeOnSelf = message.Text!.Trim() == $"/{Name}";

        if (isSubscribeOnSelf)
        {
            var userName = message.From.Username;

            if (userName == null)
            {
                var existingUser = await _storageService.GetPlayerByIdAsync(message.Chat.Id.ToString(), message.From.Id);
                if (existingUser != null)
                {
                    commandResponse.ResponseMessage = $"Юзер '{message.From.FirstName}' уже добавлен в список";
                    await SendMessageAsync(commandResponse, message.Chat.Id);
                    return commandResponse;
                }
            }
            else
            {
                var existingUser = await _storageService.GetPlayerAsync(message.Chat.Id.ToString(), userName);
                if (existingUser != null)
                {
                    commandResponse.ResponseMessage = $"Юзер '@{userName}' уже добавлен в список";
                    await SendMessageAsync(commandResponse, message.Chat.Id);
                    return commandResponse;
                }
            }

            var playerRecord = new PlayerRecordEntity
            {
                PartitionKey = message.Chat.Id.ToString(),
                UserName = message.From.Username,
                FirstName = message.From.FirstName,
                Id = message.From.Id,
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
                await SendMessageAsync(commandResponse, message.Chat.Id);
                return commandResponse;
            }

            var isAdmin = await _telegramBot.IsAdminAsync(message.Chat.Id, message.From.Id);
            if (!isAdmin)
            {
                commandResponse.ResponseMessage = "У вас нет прав добавлять новых людей в список";
                await SendMessageAsync(commandResponse, message.Chat.Id);
                return commandResponse;
            }

            var existingUser = await _storageService.GetPlayerAsync(message.Chat.Id.ToString(), userName);
            if (existingUser != null)
            {
                commandResponse.ResponseMessage = $"Юзер '{userName}' уже добавлен в список";
                await SendMessageAsync(commandResponse, message.Chat.Id);
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

        await SendMessageAsync(commandResponse, message.Chat.Id);
        return commandResponse;
    }

    private async Task SendMessageAsync(CommandResponse commandResponse, long chatId) =>
        await _telegramBot.Client.SendTextMessageAsync(chatId, commandResponse.ResponseMessage,
            parseMode: commandResponse.Mode, replyMarkup:
            commandResponse.Keyboard,
            replyToMessageId: commandResponse.ReplyToId);
}
