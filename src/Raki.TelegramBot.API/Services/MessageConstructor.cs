using Raki.TelegramBot.API.Commands;
using Raki.TelegramBot.API.Entities;
using Telegram.Bot.Types;

namespace Raki.TelegramBot.API.Services
{
    public class MessageConstructor
    {
        private readonly StorageService _storageService;

        public MessageConstructor(StorageService storageService)
        {
            _storageService = storageService;
        }

        public async Task<string> ConstructEveryoneMessageAsync(string partitionKey, SessionRecordEntity session)
        {
            var messageResult = string.Empty;

            var players = (await _storageService.GetPlayersAsync(partitionKey)).ToList();
            var userTags = GetUserTags(players);

            var getPlayerSession = await _storageService.GetUsersSessionAsync(partitionKey, session.SessionId.ToString());

            var plusPlayersSession = getPlayerSession.Where(x => x.IsPlus).ToList();
            var minusPlayersSession = getPlayerSession.Where(x => !x.IsPlus).ToList();

            messageResult = $"<b>Metadata</b>" + "\n" + "\n" +
                $"Session Id : {session.SessionId}" + "\n" +
                $"Session End Time : {session.SessionEnd.ToString("f")}" + "\n\n" +
                "<b>Players</b>" + "\n" +
                $"{userTags}";

            if (plusPlayersSession.Any())
            {
                var plusPlayers = (await _storageService.GetPlayersAsync(partitionKey,
                    plusPlayersSession.Select(x => x.UserId))).ToList();

                var plusPlayersUserTags = GetUserTags(plusPlayers);

                messageResult += "\n\n" +
                    "<b>ПЛЮС : </b>" + "\n" +
                    $"{plusPlayersUserTags}";
            }

            if (minusPlayersSession.Any())
            {
                var minusPlayer = (await _storageService.GetPlayersAsync(partitionKey, minusPlayersSession.Select(x => x.UserId))).ToList();
                var minusPlayersUserTags = GetUserTags(minusPlayer);

                messageResult += "\n\n" +
                    "<b>МИНУС : </b>" + "\n" +
                    $"{minusPlayersUserTags}";
            }

            return messageResult;
        }

        private string GetUserTags(List<PlayerRecordEntity> players)
        {
            var playersWithUserName = players.Where(x => x.UserName != null).ToList();
            var playersWithName = players.Where(x => x.UserName == null).ToList();
            var resultList = playersWithUserName.Select(x => $"@{x.UserName}").ToList();
            resultList.AddRange(playersWithName.Select(x => $"<a href=\"tg://user?id={x.Id}\">{x.FirstName}</a>"));

            return string.Join(' ', resultList);
        }

        public string? ConstructUserTag(PlayerRecordEntity playerRecord)
        {
            var result = default(string?);
            if (playerRecord != null)
            {
                result = playerRecord.UserName == null 
                    ? $"<a href=\"tg://user?id={playerRecord.Id}\">{playerRecord.FirstName}</a>" 
                    : $"@{playerRecord.UserName}";
            }

            return result;
        }
    }
}
