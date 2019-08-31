using Amazon;
using Amazon.DynamoDBv2;
using System;
using System.Collections.Generic;

namespace EasyDynamo.Abstractions
{
    public interface IDynamoContextOptions
    {
        Type ContextType { get; }

        string AccessKeyId { get; set; }

        bool LocalMode { get; set; }

        string Profile { get; set; }

        RegionEndpoint RegionEndpoint { get; set; }

        string SecretAccessKey { get; set; }

        string ServiceUrl { get; set; }

        DynamoDBEntryConversion Conversion { get; set; }

        IDictionary<Type, string> TableNameByEntityTypes { get; }

        void UseTableName<TEntity>(string tableName) where TEntity : class, new();

        void ValidateCloudMode();

        void ValidateLocalMode();
    }
}