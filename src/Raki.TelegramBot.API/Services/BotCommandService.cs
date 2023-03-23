using Raki.TelegramBot.API.Commands;
using Raki.TelegramBot.API.Models;
namespace Raki.TelegramBot.API.Services;

public class BotCommandService
{
    private readonly IEnumerable<BotCustomCommand> _botCommands;

    public BotCommandService(IEnumerable<BotCustomCommand> botCommands)
    {
        _botCommands = botCommands;
    }

    public BotCustomCommand? GetCommand(string name) => _botCommands.FirstOrDefault(x => x == name);

    public bool TryGetCommand(string? messageText, out BotCustomCommand? command)
    {
        command = default;

        if (messageText == null) return false;

        // TODO : integrate it better
        if (messageText.Contains("/everyone"))
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
}