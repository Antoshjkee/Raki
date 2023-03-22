using Azure.Data.Tables;
using Microsoft.Extensions.Options;
using Raki.TelegramBot.API.Entities;
using Raki.TelegramBot.API.Models;

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
        var user = await GetPlayerAsync(player.PartitionKey, player.UserName);

        if (user == null)
        {
            await _tableClient.AddEntityAsync(player);
        }
    }

    public async Task DeletePlayerAsync(string partitionKey, string userName)
    {
        var user = await GetPlayerAsync(partitionKey, userName);

        if (user != null)
        {
            await _tableClient.DeleteEntityAsync(user.PartitionKey, user.RowKey);
        }
    }

    public async Task<PlayerRecordEntity?> GetPlayerAsync(string partitionKey, string userName)
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
}