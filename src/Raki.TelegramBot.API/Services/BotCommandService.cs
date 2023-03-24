using Raki.TelegramBot.API.CallbackCommands;
using Raki.TelegramBot.API.Commands;
using Raki.TelegramBot.API.Models;
namespace Raki.TelegramBot.API.Services;

public class BotCommandService
{
    private readonly IEnumerable<BotCustomCommand> _botCommands;
    private readonly IEnumerable<BotCustomCallbackCommand> _botCallbackCommands;

    public BotCommandService(IEnumerable<BotCustomCommand> botCommands, IEnumerable<BotCustomCallbackCommand> botCallbackCommands)
    {
        _botCommands = botCommands;
        _botCallbackCommands = botCallbackCommands;
    }

    public BotCustomCommand? GetCommand(string name) => _botCommands.FirstOrDefault(x => x == name);

    public bool TryGetCommand(string? messageText, out BotCustomCommand? command)
    {
        command = default;

        if (messageText == null) return false;

        // TODO : integrate it better
        if (messageText.Contains("/everyone") || messageText.Contains("@everyone"))
        {
            command = _botCommands.FirstOrDefault(x => x.Name == "everyone");
            return true;
        }

        if (!messageText.StartsWith("/")) return false;

        var rawCommand = messageText.Replace("/", string.Empty)
            .Replace("@", string.Empty).Split(" ").First();

        var commandFound = _botCommands.FirstOrDefault(x => x == rawCommand);
        if (commandFound == default(BotCustomCommand)) return false;

        command = commandFound;
        return true;
    }

    public bool TryGetCallbackCommand(string? callbackText, out BotCustomCallbackCommand? callbackCommand)
    {
        callbackCommand = default;

        if (callbackText == null) return false;

        var rawCommand = callbackText.Split("-").First();

        var callbackCommandFound = _botCallbackCommands.FirstOrDefault(x => x == rawCommand);
        if (callbackCommandFound == default(BotCustomCallbackCommand)) return false;

        callbackCommand = callbackCommandFound;
        return true;
    }
}