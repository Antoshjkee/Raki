using Azure;
using Azure.Data.Tables;

namespace Raki.TelegramBot.API.Entities;

public class PlayerRecordEntity : ITableEntity
{
    // Default
    public string PartitionKey { get; set; }
    public string RowKey { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
    // Custom
    public string? UserName { get; set; }
    public long Id { get; set; }
    public string? FirstName { get; set; }
}