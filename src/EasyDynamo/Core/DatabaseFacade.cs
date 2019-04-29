using EasyDynamo.Abstractions;
using EasyDynamo.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EasyDynamo.Core
{
    public class DatabaseFacade
    {
        private readonly DynamoContextOptions options;
        private readonly ITableNameExtractor tableNameExtractor;
        private readonly ITableCreator tableCreator;
        private readonly ITableDropper tableDropper;

        public DatabaseFacade(
            ITableNameExtractor tableNameExtractor, 
            ITableCreator tableCreator, 
            ITableDropper tableDropper)
        {
            this.options = DynamoContextOptions.Instance;
            this.tableNameExtractor = tableNameExtractor;
            this.tableCreator = tableCreator;
            this.tableDropper = tableDropper;
        }

        /// <summary>
        /// Creates all tables (if not exist), declared as 
        /// DynamoDbSet in the application's DynamoContext class.
        /// </summary>
        public async Task EnsureCreatedAsync(
            CancellationToken cancellationToken = default(CancellationToken))
        {
            await Task.Run(async () =>
            {
                var allTableNames = await this.tableNameExtractor.ExtractAllTableNamesAsync();
                var newTablesByEntityType = new Dictionary<Type, string>();

                foreach (var kvp in this.options.TableNameByEntityTypes)
                {
                    var optionsTableName = kvp.Value;

                    if (allTableNames.Contains(optionsTableName))
                    {
                        continue;
                    }

                    var newTableName = await this.tableCreator
                        .CreateTableAsync(kvp.Key, kvp.Value);

                    newTablesByEntityType[kvp.Key] = newTableName;
                }

                foreach (var kvp in newTablesByEntityType)
                {
                    this.options.TableNameByEntityTypes[kvp.Key] = kvp.Value;
                }
            },
            cancellationToken);
        }

        /// <summary>
        /// Drops all tables for the configured AmazonDynamoDB client.
        /// DO NOT use with profiles with access to production tables.
        /// </summary>
        public async Task<bool> EnsureDeletedAsync(
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var result = await Task.Run(async () =>
            {
                try
                {
                    var allTableNames = await this.tableNameExtractor.ExtractAllTableNamesAsync();

                    foreach (var kvp in this.options.TableNameByEntityTypes)
                    {
                        var tableName = kvp.Value;

                        if (!allTableNames.Contains(tableName))
                        {
                            continue;
                        }

                        await this.tableDropper.DropTableAsync(tableName);
                    }

                    return true;
                }
                catch
                {
                    return false;
                }
            },
            cancellationToken);

            return result;
        }
    }
}
