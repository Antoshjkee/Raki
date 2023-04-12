namespace Raki.TelegramBot.API.Jobs;

using System.Threading.Tasks;
using Quartz;
using Raki.TelegramBot.API.Services;
using Telegram.Bot;

[DisallowConcurrentExecution]
public class SessionCleanupJob : IJob
{
    private readonly ILogger<SessionCleanupJob> _logger;
    private readonly StorageService _storageService;
    private readonly TelegramBot _telegramBot;

    public SessionCleanupJob(ILogger<SessionCleanupJob> logger, StorageService storageService, TelegramBot telegramBot)
    {
        _logger = logger;
        _storageService = storageService;
        _telegramBot = telegramBot;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation($"Job executed at {DateTime.Now:F}");

        var activeSessions = await _storageService.GetActiveSessionsAsync();

        foreach (var session in activeSessions)
        {
            if (session.SessionEnd < DateTime.UtcNow)
            {
                var messageId = session.SessionId + 1;
                var chatId = session.PartitionKey;
                session.IsActive = false;

                await _storageService.UpdateSessionAsync(session);
                await _telegramBot.Client.EditMessageReplyMarkupAsync(chatId, messageId, null);
                await _telegramBot.Client.UnpinChatMessageAsync(chatId, messageId, context.CancellationToken);
            }
        }
    }
}
