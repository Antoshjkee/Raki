using Azure.Data.Tables;
using Microsoft.Extensions.Options;
using Raki.TelegramBot.API.Entities;
using Raki.TelegramBot.API.Models;
using Telegram.Bot.Types;

namespace Raki.TelegramBot.API.Services;

public class StorageService
{
    private readonly TableClient _tableClient;

    public StorageService(IOptions<StorageOptions> storageOptions)
    {
        var serviceClient = new TableServiceClient(storageOptions.Value.ConnectionString);
        _tableClient = serviceClient.GetTableClient(storageOptions.Value.TableName);
    }

    public async Task AddPlayerAsync(PlayerRecordEntity player)
    {
        var user = await GetPlayerByUserNameAsync(player.PartitionKey, player.UserName);

        if (user == null)
        {
            await _tableClient.AddEntityAsync(player);
        }
    }

    public async Task DeletePlayerAsync(string partitionKey, long id)
    {
        var user = await GetPlayerByIdAsync(partitionKey, id);

        if (user != null)
        {
            await _tableClient.DeleteEntityAsync(user.PartitionKey, user.RowKey);
        }
    }

    public async Task<PlayerRecordEntity?> GetPlayerByUserNameAsync(string partitionKey, string userName)
    {
        var filterCondition = $"PartitionKey eq '{partitionKey}' and UserName eq '{userName}'";
        var records = _tableClient.QueryAsync<PlayerRecordEntity>(filterCondition);

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
        var records = _tableClient.QueryAsync<PlayerRecordEntity>(filterCondition);

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
        var records = _tableClient.QueryAsync<PlayerRecordEntity>(filterCondition);

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
        var records = _tableClient.QueryAsync<PlayerRecordEntity>(filterCondition);
        await foreach (var record in records)
        {
            result.Add(record);
        }

        return result;
    }

    public async Task DeletePlayerAsync(PlayerRecordEntity existingUser)
    {
        _ = await _tableClient.DeleteEntityAsync(existingUser.PartitionKey, existingUser.RowKey);
    }
}