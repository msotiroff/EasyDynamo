using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using EasyDynamo.Abstractions;
using EasyDynamo.Config;
using EasyDynamo.Exceptions;
using System.Linq;

namespace EasyDynamo.Tools.Extractors
{
    public class IndexExtractor : IIndexExtractor
    {
        public string ExtractIndex<TEntity>(
            string memberName, 
            IEntityConfiguration<TEntity> entityConfiguration, 
            Table tableInfo = null)
            where TEntity : class, new()
        {
            var fromModelConfig = entityConfiguration
                .Indexes
                ?.FirstOrDefault(i => i.HashKeyMemberName == memberName)
                ?.IndexName;
            var existInTableInfo = tableInfo
                ?.GlobalSecondaryIndexes
                ?.ContainsKey(memberName) ?? false;
            var fromTableInfo = existInTableInfo
                ? tableInfo.GlobalSecondaryIndexes[memberName].IndexName
                : null;
            var fromAttribute = ((DynamoDBGlobalSecondaryIndexHashKeyAttribute)typeof(TEntity)
                .GetProperty(memberName)
                ?.GetCustomAttributes(true)
                ?.SingleOrDefault(attr =>
                    typeof(DynamoDBGlobalSecondaryIndexHashKeyAttribute)
                        .IsAssignableFrom(attr.GetType())))
                ?.IndexNames
                ?.FirstOrDefault();

            return fromModelConfig
                ?? fromAttribute
                ?? fromTableInfo
                ?? throw new DynamoDbIndexMissingException(
                    string.Format(ExceptionMessage.EntityIndexNotFound,
                    typeof(TEntity).FullName,
                    memberName));
        }

        public string TryExtractIndex<TEntity>(
            string memberName, 
            IEntityConfiguration<TEntity> entityConfiguration, 
            Table tableInfo = null) 
            where TEntity : class, new()
        {
            try
            {
                return this.ExtractIndex(memberName, entityConfiguration, tableInfo);
            }
            catch (System.Exception)
            {
                return default;
            }
        }
    }
}
