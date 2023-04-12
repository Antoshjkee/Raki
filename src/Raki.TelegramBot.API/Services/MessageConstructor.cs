namespace Raki.TelegramBot.API.Services;

using Raki.TelegramBot.API.Entities;
using Telegram.Bot.Types.ReplyMarkups;

public class MessageConstructor
{
    private readonly StorageService _storageService;

    public MessageConstructor(StorageService storageService) => _storageService = storageService;

    public async Task<string> ConstructEveryoneMessageAsync(string partitionKey, SessionRecordEntity session)
    {
        var messageResult = string.Empty;

        if (session.UniqueLetter != "A")
        {
            messageResult += "\n\n" + $"Вторая сессия : '<strong>{session.UniqueLetter}</strong>'" + "\n\n";
        }

        var players = (await _storageService.GetPlayersAsync(partitionKey)).ToList();
        var userTags = GetUserTags(players);

        var getPlayerSession = await _storageService.GetUsersSessionAsync(partitionKey, session.SessionId.ToString());

        var plus5x0PlayersSession = getPlayerSession.Where(x => x.IsPlus5x0).ToList();
        var plus5x5PlayersSession = getPlayerSession.Where(x => x.IsPlus5x5).ToList();
        var minusPlayersSession = getPlayerSession.Where(x => !x.IsPlus5x0 && !x.IsPlus5x5).ToList();

        messageResult += $"{userTags}";

        if (plus5x0PlayersSession.Any())
        {
            var plusPlayers = (await _storageService.GetPlayersAsync(partitionKey,
                plus5x0PlayersSession.Select(x => x.UserId))).ToList();

            var plusPlayersUserTags = GetUserTags(plusPlayers);


            //👍
            messageResult += "\n\n" +
                $"Плюс 5x0 {char.ConvertFromUtf32(0x1F44D)}" + "\n" +
                $"{plusPlayersUserTags}";
        }

        if (plus5x5PlayersSession.Any())
        {
            var plusPlayers = (await _storageService.GetPlayersAsync(partitionKey,
                plus5x5PlayersSession.Select(x => x.UserId))).ToList();

            var plusPlayersUserTags = GetUserTags(plusPlayers);

            //👍
            messageResult += "\n\n" +
                $"Плюс 5x5 {char.ConvertFromUtf32(0x1F44D)}" + "\n" +
                $"{plusPlayersUserTags}";
        }

        if (minusPlayersSession.Any())
        {
            var minusPlayer = (await _storageService.GetPlayersAsync(partitionKey, minusPlayersSession.Select(x => x.UserId))).ToList();
            var minusPlayersUserTags = GetUserTags(minusPlayer);

            //👎
            messageResult += "\n\n" +
                $"Минус {char.ConvertFromUtf32(0x1F44E)}" + "\n" +
                $"{minusPlayersUserTags}";
        }

        return messageResult;
    }

    public string GetUserTags(List<PlayerRecordEntity> players)
    {
        var playersWithUserName = players.Where(x => x.UserName != null).ToList();
        var playersWithName = players.Where(x => x.UserName == null).ToList();
        var resultList = playersWithUserName.Select(x => $"@{x.UserName}").ToList();
        resultList.AddRange(playersWithName.Select(x => $"<a href=\"tg://user?id={x.Id}\">{x.FirstName}</a>"));

        return string.Join(' ', resultList);
    }

    public InlineKeyboardMarkup GetKeyboardMarkup(long sessionId)
    {

        var keyboard = new InlineKeyboardMarkup(new[]
        {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Плюс (5x0)", $"plus5x0-{sessionId}"),
                    InlineKeyboardButton.WithCallbackData("Плюс (5x5)", $"plus5x5-{sessionId}"),
                    InlineKeyboardButton.WithCallbackData("Минус", $"minus-{sessionId}")
                },
        });

        return keyboard;
    }
}
