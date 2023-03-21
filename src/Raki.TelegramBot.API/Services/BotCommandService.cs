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

    public bool TryGetCommand(string? name, out BotCustomCommand? command)
    {
        command = default;

        if (name == null) return false;
        if (!name.StartsWith("/")) return false;

        var rawCommand = name.Replace("/", string.Empty)
            .Replace("@", string.Empty);

        var commandFound = _botCommands.FirstOrDefault(x => x == rawCommand);
        if (commandFound == default(BotCustomCommand)) return false;

        command = commandFound;
        return true;
    }
}