namespace Raki.TelegramBot.API;

using Quartz;
using Raki.TelegramBot.API.Infrastructure.Extensions;
using Raki.TelegramBot.API.Jobs;
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
        builder.Services.Configure<AppConfigOptions>(builder.Configuration.GetSection("AppConfig"));

        builder.Services.AddSingleton<Services.TelegramBot>();
        builder.Services.AddScoped<StorageService>();

        // Bot registers
        builder.Services.AddBotCommands();
        builder.Services.AddBotCallbackCommands();

        builder.Services.AddScoped<BotCommandService>();
        builder.Services.AddScoped<MessageConstructor>();

        // Quartz
        builder.Services.AddQuartz(config =>
        {
            config.UseMicrosoftDependencyInjectionJobFactory();

            var jobKey = new JobKey("Session cleanup", "Session");
            config.AddJob<SessionCleanupJob>(jobKey);

            config.AddTrigger(t => t
            .WithIdentity("Cron Trigger")
            .ForJob(jobKey)
            .StartAt(DateBuilder.EvenSecondDate(DateTimeOffset.UtcNow.AddSeconds(3)))
            .WithCronSchedule("0/3 * * * * ?")
            .WithDescription("Session cleanup cron job trigger"));
        });

        builder.Services.AddQuartzServer(options =>
        {
            options.WaitForJobsToComplete = true;
        });


        builder.Services.AddControllers();

        var app = builder.Build();

        if (app.Environment.IsProduction())
        {
           app.UseHttpsRedirection();
        }

        app.UseAuthorization();
        app.MapControllers();
        app.Run();
    }
}
