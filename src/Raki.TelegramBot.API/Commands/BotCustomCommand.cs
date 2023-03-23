﻿using Telegram.Bot.Types;

namespace Raki.TelegramBot.API.Commands
{
    public abstract class BotCustomCommand
    {
        public abstract string Name { get;}
        protected bool Equals(BotCustomCommand other)
        {
            return Name == other.Name;
        }

        public abstract Task<CommandResponse> ProcessAsync(Message message);

        public static bool operator ==(string inputString, BotCustomCommand botCommand) => inputString == botCommand.Name;

        public static bool operator !=(string inputString, BotCustomCommand botCommand) => !(inputString == botCommand);

        public static bool operator ==(BotCustomCommand botCommand, string inputString) => inputString == botCommand.Name;

        public static bool operator !=(BotCustomCommand botCommand, string inputString) => !(inputString == botCommand);
        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((BotCustomCommand)obj);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }
}