using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using EasyDynamo.Attributes;
using EasyDynamo.Core;

namespace EasyDynamo.Abstractions
{
    [DynamoDbSet]
    public interface IDynamoDbSet<TEntity> where TEntity : class, new()
    {
        IDynamoDBContext Base { get; }

        IAmazonDynamoDB Client { get; }

        string TableName { get; }

        Task AddAsync(TEntity entity);

        Task<IEnumerable<TEntity>> FilterAsync(
            Expression<Func<TEntity, bool>> conditionExpression);

        Task<IEnumerable<TEntity>> FilterAsync(
            string memberName, object value, string indexName = null);

        Task<IEnumerable<TEntity>> FilterAsync<TProperty>(
            Expression<Func<TEntity, TProperty>> propertyExpression, 
            ScanOperator scanOperator, 
            TProperty value);

        Task<IEnumerable<TEntity>> FilterAsync<TProperty>(
            Expression<Func<TEntity, TProperty>> propertyExpression, 
            TProperty value, 
            string indexName = null);

        Task<IEnumerable<TEntity>> GetAsync();

        Task<PaginationResponse<TEntity>> GetAsync(
            int itemsPerPage, string paginationToken, string indexName = null);

        Task<TEntity> GetAsync(object primaryKey);

        Task<TEntity> GetAsync(object primaryKey, object rangeKey);

        Task RemoveAsync(object primaryKey);

        Task RemoveAsync(TEntity entity);

        Task SaveAsync(TEntity entity);

        Task SaveManyAsync(IEnumerable<TEntity> entities);

        Task UpdateAsync(TEntity entity);
    }
}