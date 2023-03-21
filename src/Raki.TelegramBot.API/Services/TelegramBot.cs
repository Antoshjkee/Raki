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

    public async Task<bool> IsAdmin(int chatId, int userId)
    {
        var admins = await Client.GetChatAdministratorsAsync(chatId);
        return admins.Any(x => x.User.Id == userId);
    }

    public static bool IsValidTelegramUsername(string username)
    {
        if (string.IsNullOrEmpty(username)) return false;

        if (username[0] != '@') return false;

        if (username.Length is < 5 or > 32) return false;

        for (var i = 1; i < username.Length; i++)
        {
            var @char = username[i];
            if (!char.IsLetterOrDigit(@char) && @char != '_') return false;
        }

        return true;
    }
}