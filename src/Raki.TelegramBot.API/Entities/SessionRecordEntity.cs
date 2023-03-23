using Azure;
using Azure.Data.Tables;

namespace Raki.TelegramBot.API.Entities
{

    public class SessionRecordEntity : ITableEntity
    {
        // Default
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        // Custom
        public int SessionId { get; set; }
        public DateTime SessionStart { get; init; }
        public DateTime SessionEnd { get; init; }
    }
}
