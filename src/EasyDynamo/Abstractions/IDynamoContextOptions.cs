using Amazon;
using Amazon.DynamoDBv2;

namespace EasyDynamo.Abstractions
{
    public interface IDynamoContextOptions
    {
        string AccessKeyId { get; set; }

        bool LocalMode { get; set; }

        string Profile { get; set; }

        RegionEndpoint RegionEndpoint { get; set; }

        string SecretAccessKey { get; set; }

        string ServiceUrl { get; set; }

        DynamoDBEntryConversion Conversion { get; set; }

        void UseTableName<TEntity>(string tableName) where TEntity : class, new();
    }
}