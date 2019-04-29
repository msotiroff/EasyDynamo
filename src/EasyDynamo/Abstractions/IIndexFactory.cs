using System.Collections.Generic;
using Amazon.DynamoDBv2.Model;
using EasyDynamo.Config;

namespace EasyDynamo.Abstractions
{
    public interface IIndexFactory
    {
        IEnumerable<GlobalSecondaryIndex> CreateRequestIndexes(
            IEnumerable<GlobalSecondaryIndexConfiguration> indexes);
    }
}