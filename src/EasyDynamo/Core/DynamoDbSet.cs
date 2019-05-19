using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using EasyDynamo.Abstractions;
using EasyDynamo.Attributes;
using EasyDynamo.Config;
using EasyDynamo.Exceptions;
using EasyDynamo.Extensions;
using EasyDynamo.Tools.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace EasyDynamo.Core
{
    [DynamoDbSet]
    public class DynamoDbSet<TEntity> where TEntity : class, new()
    {
        private readonly DynamoDBOperationConfig operationConfig;
        private readonly EntityConfiguration<TEntity> entityConfig;
        private readonly Table table;

        private readonly IAmazonDynamoDB client;
        private readonly IIndexExtractor indexExtractor;
        private readonly ITableNameExtractor tableNameExtractor;
        private readonly IPrimaryKeyExtractor primaryKeyExtractor;
        private readonly IEntityValidator<TEntity> validator;

        public DynamoDbSet(
            IAmazonDynamoDB client,
            IDynamoDBContext dbContext,
            IIndexExtractor indexExtractor,
            ITableNameExtractor tableNameExtractor,
            IPrimaryKeyExtractor primaryKeyExtractor,
            IEntityValidator<TEntity> validator)
        {
            this.client = client;
            this.Base = dbContext;
            this.indexExtractor = indexExtractor;
            this.tableNameExtractor = tableNameExtractor;
            this.primaryKeyExtractor = primaryKeyExtractor;
            this.validator = validator;

            this.entityConfig = EntityConfiguration<TEntity>.Instance;
            this.operationConfig = new DynamoDBOperationConfig
            {
                OverrideTableName = tableNameExtractor.ExtractTableName<TEntity>(this.table)
            };
            this.table = this.Base.TryGetTargetTable<TEntity>(this.operationConfig);
        }

        public IDynamoDBContext Base { get; }

        /// <summary>
        /// Adds the given entity to the table.
        /// </summary>
        /// <exception cref="EntityAlreadyExistException"></exception>
        public async Task AddAsync(TEntity entity)
        {
            InputValidator.ThrowIfNull(entity);

            this.ExecuteTasksOnSave(entity);

            await this.EnsureDoesNotExistAsync(entity);

            await this.ExecuteBatchWriteAsync(entity);
        }
        
        /// <summary>
        /// Adds (if not exist) or updates (if exist) the given entity.
        /// </summary>
        public async Task SaveAsync(TEntity entity)
        {
            InputValidator.ThrowIfNull(entity);

            this.ExecuteTasksOnSave(entity);

            await this.ExecuteBatchWriteAsync(entity);
        }

        /// <summary>
        /// Adds (if not exist) or updates (if exist) the given entities.
        /// </summary>
        /// <param name="entities">
        /// The entity that should be added/updated.
        /// </param>
        public async Task SaveManyAsync(IEnumerable<TEntity> entities)
        {
            InputValidator.ThrowIfNull(entities);

            try
            {
                await this.ExecuteBatchWriteAsync(entities.ToArray());
            }
            catch
            {
                var tasks = entities.Select(e => Task.Run(async () =>
                {
                    await this.SaveAsync(e);
                }));

                await Task.WhenAll(tasks);
            }
        }

        /// <summary>
        /// Gets all items from a table. 
        /// If the ReadCapacityUnits are not enough not all items will be retrieved.
        /// </summary>
        public async Task<IEnumerable<TEntity>> GetAsync()
        {
            var conditions = new List<ScanCondition>();
            var asyncSearch = this.Base.ScanAsync<TEntity>(conditions, this.operationConfig);

            return await asyncSearch.GetRemainingAsync();
        }

        /// <summary>
        /// Scan the table starting from the document after the passed pagination token. 
        /// If null passed - starts from the first document.
        /// May be used only if the current profile/user has permissions to describe the table.
        /// </summary>
        /// <param name="itemsPerPage">The limit of items to be retrieved.</param>
        /// <param name="paginationToken">
        /// The last returned pagination token should be 
        /// passed to the next call. If it's the first call - pass null.</param>
        /// <param name="indexName">Optional parameter. Name of the index to scan against.</param>
        public async Task<PaginationResponse<TEntity>> GetAsync(
            int itemsPerPage, string paginationToken, string indexName = null)
        {
            InputValidator.ThrowIfNotPositive(itemsPerPage);

            var search = this.table.Scan(new ScanOperationConfig
            {
                PaginationToken = paginationToken,
                Limit = itemsPerPage,
                IndexName = indexName
            });
            var documents = await search.GetNextSetAsync();
            var items = this.Base.FromDocuments<TEntity>(documents);
            var newToken = search.PaginationToken;
            var response = new PaginationResponse<TEntity>
            {
                NextResultSet = items,
                PaginationToken = newToken
            };

            return response;
        }

        /// <summary>
        /// Retrieve an entity by a given primary key.
        /// </summary>
        public async Task<TEntity> GetAsync(object primaryKey)
        {
            InputValidator.ThrowIfNull(primaryKey);

            var entity = await this.Base.LoadAsync<TEntity>(primaryKey, this.operationConfig);

            return entity;
        }

        /// <summary>
        /// Retrieve an entity by a given primary key and a range(sort) key.
        /// </summary>
        public async Task<TEntity> GetAsync(object primaryKey, object rangeKey)
        {
            InputValidator.ThrowIfAnyNull(primaryKey, rangeKey);

            var entity = await this.Base.LoadAsync<TEntity>(
                primaryKey, rangeKey, this.operationConfig);

            return entity;
        }
        
        /// <summary>
        /// Filters the items in a table by given predicate.
        /// Warning: Can be a very slow operation when using over a big table.
        /// </summary>
        /// <param name="conditionExpression">
        /// The lambda expression that represents the predicate.
        /// </param>
        public async Task<IEnumerable<TEntity>> FilterAsync(
            Expression<Func<TEntity, bool>> conditionExpression)
        {
            InputValidator.ThrowIfNull(conditionExpression);

            var predicate = conditionExpression.Compile();
            var all = await this.GetAsync();
            var allByCondition = all.Where(e => predicate(e));

            return allByCondition;
        }

        /// <summary>
        /// Filters the items in a table by given predicate for a single property.
        /// If there is an index over that property, 
        /// the scan operation will be made against that index.
        /// </summary>
        /// <param name="propertyExpression">
        /// The lambda expression that extracts the property.
        /// </param>
        /// <param name="scanOperator">
        /// A Amazon.DynamoDBv2.DocumentModel.ScanOperator.
        /// </param>
        /// <param name="value">
        /// The value that should be matched by the filter over the given property.
        /// </param>
        public async Task<IEnumerable<TEntity>> FilterAsync<TProperty>(
            Expression<Func<TEntity, TProperty>> propertyExpression, 
            ScanOperator scanOperator, 
            TProperty value)
        {
            InputValidator.ThrowIfAnyNull(propertyExpression, value);

            var memberName = propertyExpression.TryGetMemberName();
            var currentOperationConfig = this.operationConfig.Clone();
            var indexName = this.indexExtractor.ExtractIndex<TEntity>(memberName, this.table);

            if (!string.IsNullOrWhiteSpace(indexName))
            {
                currentOperationConfig.IndexName = indexName;
            }

            var conditions = new List<ScanCondition>
            {
                new ScanCondition(memberName, scanOperator, value)
            };

            var asyncScan = this.Base.ScanAsync<TEntity>(conditions, currentOperationConfig);

            return await asyncScan.GetRemainingAsync();
        }

        /// <summary>
        /// Filters the items by property and returns all that match the given value.
        /// </summary>
        /// <param name="propertyExpression">
        /// The lambda expression that extracts the property.
        /// </param>
        /// <param name="value">
        /// The value that should be matched by the filter over the given property.
        /// </param>
        /// <param name="indexName">
        /// Optional parameter. If null passed, the first index with hash key the given property will be used.
        /// </param>
        public async Task<IEnumerable<TEntity>> FilterAsync<TProperty>(
            Expression<Func<TEntity, TProperty>> propertyExpression,
            TProperty value,
            string indexName = null)
        {
            InputValidator.ThrowIfAnyNull(propertyExpression, value);

            var memberName = propertyExpression.TryGetMemberName();

            return await this.FilterAsync(memberName, value, indexName);
        }

        /// <summary>
        /// Filters the items by property and returns all that match the given value.
        /// </summary>
        /// <param name="memberName">
        /// The name of the property that should be filtering against.
        /// </param>
        /// <param name="value">
        /// The value that should be matched by the filter over the given property.
        /// </param>
        /// <param name="indexName">
        /// Optional parameter. If null passed, the first index with hash key the given property will be used.
        /// </param>
        public async Task<IEnumerable<TEntity>> FilterAsync(
            string memberName, object value, string indexName = null)
        {
            InputValidator.ThrowIfNull(value);

            InputValidator.ThrowIfNullOrWhitespace(memberName);

            var index = indexName
                ?? this.indexExtractor.ExtractIndex<TEntity>(memberName, this.table);
            var configuration = this.operationConfig.Clone();
            configuration.IndexName = index;

            var search = this.Base.QueryAsync<TEntity>(value, configuration);
            var entities = await search.GetRemainingAsync();

            return entities;
        }

        /// <summary>
        /// Updates the given entity if it's in a valid state. 
        /// Throws an exception if an entity with same primary key does not exist.
        /// If you want to 'add or update' the entity - use SaveAsync method.
        /// </summary>
        /// <exception cref="EntityNotFoundException"></exception>
        public async Task UpdateAsync(TEntity entity)
        {
            InputValidator.ThrowIfNull(entity);

            this.ExecuteTasksOnSave(entity);

            await this.EnsureExistAsync(entity);

            await this.Base.SaveAsync(entity, this.operationConfig);
        }
        
        /// <summary>
        /// Removes the given entity.
        /// </summary>
        public async Task RemoveAsync(TEntity entity)
        {
            InputValidator.ThrowIfNull(entity);

            await this.Base.DeleteAsync(entity, this.operationConfig);
        }

        /// <summary>
        /// Removes the entity with the passed primary key.
        /// </summary>
        public async Task RemoveAsync(object primaryKey)
        {
            InputValidator.ThrowIfNull(primaryKey);

            var entity = await this.GetAsync(primaryKey);

            await this.Base.DeleteAsync(entity, this.operationConfig);
        }
        
        private async Task ExecuteBatchWriteAsync(params TEntity[] entities)
        {
            var batchWrite = this.Base.CreateBatchWrite<TEntity>(this.operationConfig);

            batchWrite.AddPutItems(entities);

            await batchWrite.ExecuteAsync();
        }

        private async Task EnsureDoesNotExistAsync(TEntity entity)
        {
            var key = this.primaryKeyExtractor.ExtractPrimaryKey(entity);
            var exist = (await this.GetAsync(key)) != null;

            if (exist)
            {
                throw new EntityAlreadyExistException(ExceptionMessage.EntityAlreadyExist);
            }
        }

        private async Task EnsureExistAsync(TEntity entity)
        {
            var key = this.primaryKeyExtractor.ExtractPrimaryKey(entity);
            var exist = (await this.GetAsync(key)) != null;

            if (!exist)
            {
                throw new EntityNotFoundException(ExceptionMessage.EntityDoesNotExist);
            }
        }

        private void ExecuteTasksOnSave(TEntity entity)
        {
            // Set all specified default values:
            foreach (var memberConfig in this.entityConfig.Properties)
            {
                if (memberConfig.DefaultValue == null)
                {
                    continue;
                }

                var member = typeof(TEntity).GetProperty(memberConfig.MemberName);
                var defaultTypeValue = member.PropertyType.GetDefaultValue();
                var memberCurrentValue = member.GetValue(entity);

                if (memberCurrentValue == defaultTypeValue)
                {
                    member.SetValue(entity, memberConfig.DefaultValue);
                }
            }

            // Validates the entity against its configuration and its attributes:
            if (this.entityConfig.ValidateOnSave)
            {
                this.validator.Validate(entity);
            }

            var entityType = entity.GetType();

            // Set to a default value all ignored members:
            foreach (var name in this.entityConfig.IgnoredMembersNames)
            {
                var propertyInfo = entityType.GetProperty(name);

                propertyInfo?.SetDefaultValue(entity);
            }
        }
    }
}
