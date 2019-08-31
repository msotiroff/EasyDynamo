using Amazon.DynamoDBv2.DocumentModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EasyDynamo.Abstractions
{
    public interface ITableNameExtractor
    {
        string ExtractTableName<TEntity>(
            IDynamoContextOptions options,
            IEntityConfiguration<TEntity> entityConfiguration,
            Table tableInfo = null) 
            where TEntity : class, new();

        Task<IEnumerable<string>> ExtractAllTableNamesAsync();
    }
}