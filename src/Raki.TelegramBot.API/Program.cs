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

            builder.Services.Configure<BotConfig>(builder.Configuration.GetSection("BotConfig"));
            builder.Services.AddSingleton<Services.TelegramBot>();
            // Bot registers
            builder.Services.AddBotCommands();
            builder.Services.AddScoped<BotCommandService>();
            
            builder.Services.AddControllers();

            var app = builder.Build();
           // app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();
            app.Run();
        }
    }
}