using Amazon.DynamoDBv2.DataModel;
using EasyDynamo.Abstractions;
using EasyDynamo.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace EasyDynamo.Tools
{
    public class IndexConfigurationFactory : IIndexConfigurationFactory
    {
        public IEnumerable<GlobalSecondaryIndexConfiguration> CreateIndexConfigByAttributes(
            Type entityType)
        {
            var configs = new List<GlobalSecondaryIndexConfiguration>();

            var hashAttributesByProperty = entityType
                .GetProperties()
                .Where(pi => pi.GetCustomAttribute<
                    DynamoDBGlobalSecondaryIndexHashKeyAttribute>(true) != null)
                .Select(pi => new
                {
                    PropertyInfo = pi,
                    HashAttribute = pi.GetCustomAttribute<
                        DynamoDBGlobalSecondaryIndexHashKeyAttribute>(true)
                })
                .ToDictionary(kvp => kvp.PropertyInfo, kvp => kvp.HashAttribute);
            var rangeAttributesByProperty = entityType
                .GetProperties()
                .Where(pi => pi.GetCustomAttribute<
                    DynamoDBGlobalSecondaryIndexRangeKeyAttribute>(true) != null)
                .Select(pi => new
                {
                    PropertyInfo = pi,
                    HashAttribute = pi.GetCustomAttribute<
                        DynamoDBGlobalSecondaryIndexRangeKeyAttribute>(true)
                })
                .ToDictionary(kvp => kvp.PropertyInfo, kvp => kvp.HashAttribute);

            foreach (var kvp in hashAttributesByProperty)
            {
                var indexName = kvp.Value.IndexNames.FirstOrDefault();

                var currentConfig = new GlobalSecondaryIndexConfiguration
                {
                    IndexName = indexName,
                    ReadCapacityUnits = Constants.DefaultReadCapacityUnits,
                    WriteCapacityUnits = Constants.DefaultWriteCapacityUnits,
                    HashKeyMemberName = kvp.Key.Name,
                    HashKeyMemberType = kvp.Key.PropertyType
                };

                if (rangeAttributesByProperty.Any(a => a.Value.IndexNames?[0] == indexName))
                {
                    var rangeKeyProperty = rangeAttributesByProperty
                        .First(rap => rap.Value.IndexNames[0] == indexName)
                        .Key;
                    currentConfig.RangeKeyMemberName = rangeKeyProperty.Name;
                    currentConfig.RangeKeyMemberType = rangeKeyProperty.PropertyType;
                }

                configs.Add(currentConfig);
            }

            return configs;
        }
    }
}
