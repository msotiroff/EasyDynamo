using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using EasyDynamo.Abstractions;
using EasyDynamo.Config;
using System.Collections.Generic;
using System.Linq;

namespace EasyDynamo.Factories
{
    public class IndexFactory : IIndexFactory
    {
        public IEnumerable<GlobalSecondaryIndex> CreateRequestIndexes(
            IEnumerable<GlobalSecondaryIndexConfiguration> indexes)
        {
            indexes = indexes ?? Enumerable.Empty<GlobalSecondaryIndexConfiguration>();
            var gsis = new List<GlobalSecondaryIndex>();

            foreach (var index in indexes)
            {
                if (string.IsNullOrWhiteSpace(index.IndexName) || 
                    string.IsNullOrWhiteSpace(index.HashKeyMemberName))
                {
                    continue;
                }

                var gsi = new GlobalSecondaryIndex
                {
                    IndexName = index.IndexName,
                    Projection = new Projection
                    {
                        ProjectionType = ProjectionType.ALL
                    },
                    ProvisionedThroughput = new ProvisionedThroughput
                    {
                        ReadCapacityUnits = index.ReadCapacityUnits,
                        WriteCapacityUnits = index.WriteCapacityUnits
                    },
                    KeySchema = new List<KeySchemaElement>
                    {
                        new KeySchemaElement(index.HashKeyMemberName, KeyType.HASH)
                    }
                };

                if (!string.IsNullOrWhiteSpace(index.RangeKeyMemberName))
                {
                    gsi.KeySchema.Add(
                        new KeySchemaElement(index.RangeKeyMemberName, KeyType.RANGE));
                }

                gsis.Add(gsi);
            }

            return gsis;
        }
    }
}
