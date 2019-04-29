using Amazon.DynamoDBv2.DocumentModel;

namespace EasyDynamo.Abstractions
{
    public interface IPrimaryKeyExtractor
    {
        object ExtractPrimaryKey<TEntity>(TEntity entity, Table tableInfo = null) 
            where TEntity : class, new();

        object TryExtractPrimaryKey<TEntity>(TEntity entity, Table tableInfo = null) 
            where TEntity : class, new();
    }
}