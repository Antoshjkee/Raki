using Microsoft.Extensions.Options;
using Raki.TelegramBot.API.Models;
using Telegram.Bot;

namespace Raki.TelegramBot.API.Services;

public class TelegramBot
{
    public ITelegramBotClient Client { get; }

    public TelegramBot(IOptions<BotConfig> botConfig)
    {
        Client = new TelegramBotClient(botConfig.Value.BotToken);
    }
}