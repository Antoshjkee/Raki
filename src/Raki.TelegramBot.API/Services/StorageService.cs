using Azure.Data.Tables;
using Microsoft.Extensions.Options;
using Raki.TelegramBot.API.Entities;
using Raki.TelegramBot.API.Models;

namespace Raki.TelegramBot.API.Services;

public class StorageService
{
    private readonly IOptions<TimezoneOptions> _timeZoneOptions;

    private static readonly string UserTableClientName = "users";
    private static readonly string SessionsTableClientName = "sessions";
    private static readonly string UsersSessionsTableClientName = "usersSessions";
    private readonly Lazy<TableClient> _userTableClient;
    private readonly Lazy<TableClient> _sessionTableClient;
    private readonly Lazy<TableClient> _usersSessionsTableClient;
    public TableClient UserTableClient => _userTableClient.Value;
    public TableClient SessionsTableClient => _sessionTableClient.Value;
    public TableClient UsersSessionsTableClient => _usersSessionsTableClient.Value;

    public StorageService(IOptions<StorageOptions> storageOptions, IOptions<TimezoneOptions> timeZoneOptions)
    {
        _userTableClient = new Lazy<TableClient>(ClientFactory(storageOptions.Value.ConnectionString, UserTableClientName));
        _sessionTableClient = new Lazy<TableClient>(ClientFactory(storageOptions.Value.ConnectionString, SessionsTableClientName));
        _usersSessionsTableClient = new Lazy<TableClient>(ClientFactory(storageOptions.Value.ConnectionString, UsersSessionsTableClientName));
        _timeZoneOptions = timeZoneOptions;
    }

    public async Task AddPlayerAsync(PlayerRecordEntity player)
    {
        var playerData = player.UserName ?? player.FirstName;
        var user = await GetPlayerAsync(player.PartitionKey, playerData);

        if (user == null)
        {
            await UserTableClient.AddEntityAsync(player);
        }
    }

    public async Task DeletePlayerAsync(string partitionKey, long id)
    {
        var user = await GetPlayerAsync(partitionKey, id.ToString());

        if (user != null)
        {
            await UserTableClient.DeleteEntityAsync(user.PartitionKey, user.RowKey);
        }
    }

    public Task<PlayerRecordEntity?> GetPlayerAsync(string partitionKey, string playerData)
    {
        var filterCondition = $"PartitionKey eq '{partitionKey}'";

        var records = UserTableClient.Query<PlayerRecordEntity>(filterCondition);
        long.TryParse(playerData, out var parsedId);

        var foundUser = records.FirstOrDefault(x => (x.FirstName == playerData && x.UserName == null)
        || x.UserName == $"@{playerData}"
        || x.UserName == playerData.Replace("@", string.Empty)
        || x.Id == parsedId);

        return Task.FromResult(foundUser);
    }

    public async Task<PlayerRecordEntity?> GetPlayerByIdAsync(string partitionKey, long id)
    {
        var filterCondition = $"PartitionKey eq '{partitionKey}' and Id eq '{id}'";
        var records = UserTableClient.QueryAsync<PlayerRecordEntity>(filterCondition);

        await foreach (var record in records)
        {
            // Assuming its only one
            return record;
        }

        return default;
    }

    public async Task<IEnumerable<PlayerRecordEntity>> GetPlayersAsync(string partitionKey)
    {
        var result = new List<PlayerRecordEntity>();

        var filterCondition = $"PartitionKey eq '{partitionKey}'";
        var records = UserTableClient.QueryAsync<PlayerRecordEntity>(filterCondition);
        await foreach (var record in records)
        {
            result.Add(record);
        }

        return result;
    }

    public async Task DeletePlayerAsync(PlayerRecordEntity existingUser)
    {
        _ = await UserTableClient.DeleteEntityAsync(existingUser.PartitionKey, existingUser.RowKey);
    }

    // Session
    public async Task<SessionRecordEntity?> GetSesssion(string partitionKey, string sessionId)
    {
        var filterCondition = $"PartitionKey eq '{partitionKey}' and SessionId eq '{sessionId}'";
        var records = SessionsTableClient.QueryAsync<SessionRecordEntity>(filterCondition);

        await foreach (var record in records)
        {
            return record;
        }

        return default;
    }


    public Task<SessionRecordEntity?> GetSessionByIdAsync(string partitionKey, string sessionId)
    {
        var filterCondition = $"PartitionKey eq '{partitionKey}' and SessionId eq '{sessionId}'";
        var records = SessionsTableClient.Query<SessionRecordEntity>(filterCondition);
        return Task.FromResult(records.FirstOrDefault());
    }

    public async Task CreateSessionAsync(SessionRecordEntity sessionRecordEntity)
    {
        var session = await GetSesssion(sessionRecordEntity.PartitionKey, sessionRecordEntity.SessionId.ToString());

        if (session == null)
        {
            await SessionsTableClient.AddEntityAsync(sessionRecordEntity);
        }
    }

    public Task<SessionRecordEntity?> GetCurrentSessionAsync(string partitionKey)
    {
        var sessionLtTime = DateTime.UtcNow.AddHours(2);
        var filterCondition = $"PartitionKey eq '{partitionKey}'";

        var records = SessionsTableClient.Query<SessionRecordEntity>(filterCondition);
        return Task.FromResult(records.FirstOrDefault(x => x.SessionEnd >= sessionLtTime));
    }

    // User Sessions
    public async Task AddUserToSession(PlayerSessionRecordEntity playerSessionRecord)
    {
        var currentUserSession = await GetUserSessionAsync(playerSessionRecord.PartitionKey,
            playerSessionRecord.SessionId.ToString(), playerSessionRecord.UserId);

        if (currentUserSession == null)
        {
            await UsersSessionsTableClient.AddEntityAsync(playerSessionRecord);

        }
    }

    public Task<PlayerSessionRecordEntity?> GetUserSessionAsync(string partitionKey, string sessionId, long userId)
    {
        var filterCondition = $"PartitionKey eq '{partitionKey}' and SessionId eq '{sessionId}' and UserId eq '{userId}'";
        var records = UsersSessionsTableClient.Query<PlayerSessionRecordEntity>(filterCondition);
        return Task.FromResult(records.FirstOrDefault());
    }

    // Factories
    private static TableClient ClientFactory(string connectionString, string tableName)
    {
        var serviceClient = new TableServiceClient(connectionString);
        return serviceClient.GetTableClient(tableName);
    }

}