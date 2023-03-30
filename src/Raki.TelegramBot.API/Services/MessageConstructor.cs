using Raki.TelegramBot.API.Entities;

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

            messageResult = $"{userTags}";
            if (plusPlayersSession.Any())
            {
                var plusPlayers = (await _storageService.GetPlayersAsync(partitionKey,
                    plusPlayersSession.Select(x => x.UserId))).ToList();

                var plusPlayersUserTags = GetUserTags(plusPlayers);


                //👍
                messageResult += "\n\n" +
                    $"Плюс {char.ConvertFromUtf32(0x1F44D)}" + "\n" +
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

        public async Task<string?> GetRespondedPlayersTagsAsync(string partitionKey, string sessionId, bool responseStatus = true)
        {
            var getPlayerSession = await _storageService.GetUsersSessionAsync(partitionKey, sessionId);
            var respondedPlayers = getPlayerSession.Where(x => x.IsPlus == responseStatus).ToList();

            var players = (await _storageService.GetPlayersAsync(partitionKey, respondedPlayers.Select(x => x.UserId))).ToList();

            return GetUserTags(players);
        }
    }
}
