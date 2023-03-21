namespace Raki.TelegramBot.API.Models;

public class EveryoneCommand : BotCustomCommand
{
    public override string Name => "everyone";
    public override string Process() => "@geraskrip @Antoshjke @AntonioPankov";

    public override string AdminProcess()
    {
        throw new NotImplementedException();
    }
}