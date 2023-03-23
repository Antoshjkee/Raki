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
        var user = await GetPlayerByUserNameAsync(player.PartitionKey, player.UserName);

        if (user == null)
        {
            await UserTableClient.AddEntityAsync(player);
        }
    }

    public async Task DeletePlayerAsync(string partitionKey, long id)
    {
        var user = await GetPlayerByIdAsync(partitionKey, id);

        if (user != null)
        {
            await UserTableClient.DeleteEntityAsync(user.PartitionKey, user.RowKey);
        }
    }

    public async Task<PlayerRecordEntity?> GetPlayerByUserNameAsync(string partitionKey, string userName)
    {
        var filterCondition = $"PartitionKey eq '{partitionKey}' and UserName eq '{userName}'";
        var records = UserTableClient.QueryAsync<PlayerRecordEntity>(filterCondition);

        await foreach (var record in records)
        {
            // Assuming its only one
            return record;
        }

        return default;
    }

    public async Task<PlayerRecordEntity?> GetPlayerByFirsNameAsync(string partitionKey, string firstName)
    {
        var filterCondition = $"PartitionKey eq '{partitionKey}' and FirstName eq '{firstName}'";
        var records = UserTableClient.QueryAsync<PlayerRecordEntity>(filterCondition);

        await foreach (var record in records)
        {
            // Assuming its only one
            return record;
        }

        return default;
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
    public async Task<SessionRecordEntity?> GetSesssion(string partitionKey, int sessionId)
    {
        var filterCondition = $"PartitionKey eq '{partitionKey}' and SessionId eq '{sessionId}'";
        var records = SessionsTableClient.QueryAsync<SessionRecordEntity>(filterCondition);

        await foreach (var record in records)
        {
            return record;
        }

        return default;
    }

    public async Task CreateSessionAsync(SessionRecordEntity sessionRecordEntity)
    {
        var session = await GetSesssion(sessionRecordEntity.PartitionKey, sessionRecordEntity.SessionId);

        if (session == null)
        {
            await SessionsTableClient.AddEntityAsync(sessionRecordEntity);
        }
    }

    public async Task<(bool, SessionRecordEntity?)> IsSessionAvailableAsync(string partitionKey, int sessionId)
    {
        var session = await GetSesssion(partitionKey, sessionId);

        var gmtPlus2 = TimeZoneInfo.FindSystemTimeZoneById(_timeZoneOptions.Value.Zone);
        var nowGmtPlus2 = DateTimeOffset.UtcNow.ToOffset(gmtPlus2.GetUtcOffset(DateTimeOffset.UtcNow));

        if (session != null && session.SessionEnd > nowGmtPlus2)
        {
            return (true, session);
        }

        return (false, default);
    }

    public Task<SessionRecordEntity?> GetCurrentSessionAsync(string partitionKey)
    {
        var sessionLtTime = DateTime.UtcNow.AddHours(2);
        var filterCondition = $"PartitionKey eq '{partitionKey}'";

        var records = SessionsTableClient.Query<SessionRecordEntity>(filterCondition);
        return Task.FromResult(records.FirstOrDefault(x => x.SessionEnd >= sessionLtTime));
    }

    // Factories
    private static TableClient ClientFactory(string connectionString, string tableName)
    {
        var serviceClient = new TableServiceClient(connectionString);
        return serviceClient.GetTableClient(tableName);
    }
}