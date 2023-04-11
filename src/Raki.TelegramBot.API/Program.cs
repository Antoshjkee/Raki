using Raki.TelegramBot.API.Infrastructure.Extensions;
using Raki.TelegramBot.API.Models;
using Raki.TelegramBot.API.Services;

namespace Raki.TelegramBot.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.Configure<BotOptions>(builder.Configuration.GetSection("BotConfig"));
            builder.Services.Configure<StorageOptions>(builder.Configuration.GetSection("Storage"));
            builder.Services.Configure<TimezoneOptions>(builder.Configuration.GetSection("TimeZone"));
            builder.Services.Configure<AppConfigOptions>(builder.Configuration.GetSection("AppConfig"));

            builder.Services.AddSingleton<Services.TelegramBot>();
            builder.Services.AddScoped<StorageService>();

            // Bot registers
            builder.Services.AddBotCommands();
            builder.Services.AddBotCallbackCommands();

            builder.Services.AddScoped<BotCommandService>();
            builder.Services.AddScoped<MessageConstructor>();

            builder.Services.AddControllers();

            var app = builder.Build();
            // app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();
            app.Run();
        }
    }
}