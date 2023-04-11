namespace Raki.TelegramBot.API.Services;

using System.Threading;
using System.Threading.Tasks;

public class PollBackgroundService : BackgroundService
{
    private readonly StorageService _storageService;
    private readonly TelegramBot _telegramBot;

    public PollBackgroundService(StorageService storageService, TelegramBot telegramBot)
    {
        _storageService = storageService;
        _telegramBot = telegramBot;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            // TODO : Check polls that active and make a descision based on the poll
            // This will be executed every 1 minute
            await Task.Delay(60000, stoppingToken);
        };
    }
}
