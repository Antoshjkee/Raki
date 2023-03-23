using Telegram.Bot.Types;

namespace Raki.TelegramBot.API.CallbackCommands
{
    public abstract class BotCustomCallbackCommand
    {
        public abstract string Name { get; }
        protected bool Equals(BotCustomCallbackCommand other)
        {
            return Name == other.Name;
        }

        public abstract Task<CallbackCommandResponse> ProcessAsync(CallbackQuery callbackQuery);

        public static bool operator ==(string inputString, BotCustomCallbackCommand botCommand) => inputString == botCommand.Name;

        public static bool operator !=(string inputString, BotCustomCallbackCommand botCommand) => !(inputString == botCommand);

        public static bool operator ==(BotCustomCallbackCommand botCommand, string inputString) => inputString == botCommand.Name;

        public static bool operator !=(BotCustomCallbackCommand botCommand, string inputString) => !(inputString == botCommand);
        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((BotCustomCallbackCommand)obj);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }
}
