using Raki.TelegramBot.API.CallbackCommands;
using System.Reflection;

namespace Raki.TelegramBot.API.Infrastructure.Extensions
{
    public static class BotCallbackCommandRegistrar
    {
        public static IServiceCollection AddBotCallbackCommands(this IServiceCollection services)
        {
            var myServiceBaseType = typeof(BotCustomCallbackCommand);
            var typesToRegister = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(type => myServiceBaseType.IsAssignableFrom(type) && !type.IsAbstract);

            foreach (var type in typesToRegister)
            {
                services.AddScoped(myServiceBaseType, type);
            }

            return services;
        }
    }
}
