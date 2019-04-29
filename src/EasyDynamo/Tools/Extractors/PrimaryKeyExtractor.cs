using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using EasyDynamo.Abstractions;
using EasyDynamo.Config;
using EasyDynamo.Exceptions;
using System.Linq;
using System.Reflection;

namespace EasyDynamo.Tools.Extractors
{
    public class PrimaryKeyExtractor : IPrimaryKeyExtractor
    {
        public object ExtractPrimaryKey<TEntity>(TEntity entity, Table tableInfo = null)
            where TEntity : class, new()
        {
            var keyExpression = EntityConfiguration<TEntity>.Instance.HashKeyMemberExpression;

            if (keyExpression != null)
            {
                var keyExtractFunction = keyExpression.Compile();

                return keyExtractFunction(entity);
            }

            var keyMember = typeof(TEntity)
                .GetProperties()
                .FirstOrDefault(pi => pi.GetCustomAttribute<DynamoDBHashKeyAttribute>() != null)
                ?? typeof(TEntity)
                .GetProperty(tableInfo?.HashKeys?.FirstOrDefault() ?? string.Empty);

            if (keyMember != null)
            {
                return keyMember.GetValue(entity);
            }

            throw new DynamoContextConfigurationException(
                    ExceptionMessage.HashKeyConfigurationNotFound);
        }

        public object TryExtractPrimaryKey<TEntity>(TEntity entity, Table tableInfo = null)
            where TEntity : class, new()
        {
            try
            {
                return this.ExtractPrimaryKey(entity, tableInfo);
            }
            catch
            {
                return default(object);
            }
        }
    }
}
