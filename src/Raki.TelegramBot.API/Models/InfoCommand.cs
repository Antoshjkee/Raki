namespace Raki.TelegramBot.API.Models
{
    public class InfoCommand : BotCustomCommand
    {
        public override string Name => "info";
        public override string Process() => "**/info** - Информация о командах.\r\n **/plus** - Дать плюс.\r\n **/minus** - Дать минус.";

        public override string AdminProcess()
        {
            throw new NotImplementedException();
        }
    }
}
