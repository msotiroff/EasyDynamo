using Amazon.DynamoDBv2.DocumentModel;

namespace EasyDynamo.Abstractions
{
    public interface IIndexExtractor
    {
        string ExtractIndex<TEntity>(
            string memberName, 
            IEntityConfiguration<TEntity> entityConfiguration, 
            Table tableInfo = null)
            where TEntity : class, new();

        string TryExtractIndex<TEntity>(
            string memberName,
            IEntityConfiguration<TEntity> entityConfiguration,
            Table tableInfo = null)
            where TEntity : class, new();
    }
}