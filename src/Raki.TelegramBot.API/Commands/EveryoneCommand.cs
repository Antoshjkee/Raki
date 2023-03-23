using Microsoft.Extensions.Options;
using Raki.TelegramBot.API.Entities;
using Raki.TelegramBot.API.Models;
using Raki.TelegramBot.API.Services;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Raki.TelegramBot.API.Commands;

public class EveryoneCommand : BotCustomCommand
{
    private readonly StorageService _storageService;
    private readonly IOptions<TimezoneOptions> _timeZoneOptions;

    public override string Name => "everyone";

    public EveryoneCommand(StorageService storageService, IOptions<TimezoneOptions> timeZoneOptions)
    {
        _storageService = storageService;
        _timeZoneOptions = timeZoneOptions;
    }
    public override async Task<CommandResponse> ProcessAsync(Message message)
    {
        var commandResponse = new CommandResponse
        {
            Mode = ParseMode.Html

        };
        var players = (await _storageService.GetPlayersAsync(message.Chat.Id.ToString())).ToList();

        var playersWithUserName = players.Where(x => x.UserName != null).ToList();
        var playersWithName = players.Where(x => x.UserName == null).ToList();

        if (players.Any())
        {
            var session = default(SessionRecordEntity);

            var currentSession = await _storageService.GetCurrentSessionAsync(message.Chat.Id.ToString());

            if (currentSession != null)
            {
                session = currentSession;
            }
            else
            {
                var sessionLtTime = DateTime.UtcNow.AddHours(2);

               // var gmtPlus2 = TimeZoneInfo.FindSystemTimeZoneById(_timeZoneOptions.Value.Zone);
               // var nowGmtPlus2 = DateTimeOffset.UtcNow.ToOffset(gmtPlus2.GetUtcOffset(DateTimeOffset.UtcNow));

                var newSession = new SessionRecordEntity
                {
                    RowKey = Guid.NewGuid().ToString(),
                    PartitionKey = message.Chat.Id.ToString(),
                    SessionStart = sessionLtTime,
                    SessionEnd = sessionLtTime.AddHours(1),
                    SessionId = message.MessageId,
                };

                await _storageService.CreateSessionAsync(newSession);
                session = newSession;
            }

            var resultList = playersWithUserName.Select(x => $"@{x.UserName}").ToList();
            resultList.AddRange(playersWithName.Select(x => $"<a href=\"tg://user?id={x.Id}\">{x.FirstName}</a>"));

            commandResponse.ResponseMessage =$"<b>Metadata</b>" + "\n" + "\n" +
                $"Session Id : {session.SessionId}" + "\n" +
                $"Session End Time : {session.SessionEnd.ToString("f")}" + "\n" + "\n" +
                "<b>Players</b>" + "\n" +
                $"{string.Join(' ', resultList)}";

            var keyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Плюс", "plus"),
                    InlineKeyboardButton.WithCallbackData("Минус", "minus")
                },
            });

            commandResponse.Keyboard = keyboard;
        }
        else
        {
            commandResponse.ResponseMessage = "Юзеров нет в списке";
        }




        return commandResponse;
    }
}