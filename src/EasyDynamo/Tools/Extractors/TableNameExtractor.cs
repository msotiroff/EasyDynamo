using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using EasyDynamo.Abstractions;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EasyDynamo.Tools.Extractors
{
    public class TableNameExtractor : ITableNameExtractor
    {
        private readonly IAmazonDynamoDB client;

        public TableNameExtractor(IAmazonDynamoDB client)
        {
            this.client = client;
        }

        public string ExtractTableName<TEntity>(
            IDynamoContextOptions options, 
            IEntityConfiguration<TEntity> entityConfiguration,
            Table tableInfo = null) 
            where TEntity : class, new()
        {
            var fromTableInfo = tableInfo?.TableName;
            var fromEntityConfig = entityConfiguration.TableName;
            var tableNamesByEntityTypes = options.TableNameByEntityTypes;
            var fromContextConfig = tableNamesByEntityTypes.ContainsKey(typeof(TEntity))
                ? tableNamesByEntityTypes[typeof(TEntity)]
                : default(string);

            return fromEntityConfig
                ?? fromContextConfig
                ?? fromTableInfo
                ?? typeof(TEntity).Name;
        }

        public async Task<IEnumerable<string>> ExtractAllTableNamesAsync()
        {
            var allTableNames = new List<string>();
            var lastEvaluatedTableName = default(string);

            do
            {
                var request = new ListTablesRequest
                {
                    ExclusiveStartTableName = lastEvaluatedTableName
                };

                var response = await this.client.ListTablesAsync(request);

                allTableNames.AddRange(response.TableNames);

                lastEvaluatedTableName = response.LastEvaluatedTableName;
            }
            while (lastEvaluatedTableName != null);

            return allTableNames;
        }
    }
}
