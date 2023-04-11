namespace Raki.TelegramBot.API;

using ChatGPT.Net;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Raki.TelegramBot.API.Infrastructure.Extensions;
using Raki.TelegramBot.API.Models;
using Raki.TelegramBot.API.Services;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.Configure<BotOptions>(builder.Configuration.GetSection("BotConfig"));
        builder.Services.Configure<StorageOptions>(builder.Configuration.GetSection("Storage"));
        builder.Services.Configure<TimezoneOptions>(builder.Configuration.GetSection("TimeZone"));
        builder.Services.Configure<ChatGptOptions>(builder.Configuration.GetSection("ChatGpt"));


        builder.Services.AddSingleton<TelegramBot>();
        builder.Services.AddScoped<StorageService>();

        // Bot registers
        builder.Services.AddBotCommands();
        builder.Services.AddBotCallbackCommands();

        builder.Services.AddScoped<BotCommandService>();
        builder.Services.AddScoped<MessageConstructor>();
        builder.Services.AddScoped(implementationFactory =>
        new ChatGpt(builder.Configuration.GetSection("ChatGpt").GetValue<string>("ApiKey")));

        // Background service
        //services.AddHostedService<PollBackgroundService>();
        builder.Services.AddControllers();

        var app = builder.Build();
        // app.UseHttpsRedirection();
        app.UseAuthorization();
        app.MapControllers();
        app.Run();
    }
}
