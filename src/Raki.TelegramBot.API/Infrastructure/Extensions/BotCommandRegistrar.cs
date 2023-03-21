using System.Reflection;
using Raki.TelegramBot.API.Models;

namespace Raki.TelegramBot.API.Infrastructure.Extensions;

public static class BotCommandRegistrar
{
    public static IServiceCollection AddBotCommands(this IServiceCollection services)
    {
        var myServiceBaseType = typeof(BotCustomCommand);
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