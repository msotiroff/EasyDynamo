using EasyDynamo.Abstractions;
using EasyDynamo.Config;
using EasyDynamo.Core;
using EasyDynamo.Extensions;
using EasyDynamo.Tools.Validators;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace EasyDynamo.Builders
{
    public class EntityTypeBuilder<TContext, TEntity> : IEntityTypeBuilder<TContext, TEntity> 
        where TContext : DynamoContext
        where TEntity : class, new()
    {
        private readonly EntityConfiguration<TContext, TEntity> entityConfig;
        private readonly IDynamoContextOptions contextOptions;

        protected internal EntityTypeBuilder(
            EntityConfiguration<TContext, TEntity> entityConfig,
            IDynamoContextOptions contextOptions)
        {
            this.entityConfig = entityConfig;
            this.contextOptions = contextOptions;
        }

        /// <summary>
        /// Adds a table name for a specific entity type.
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        public IEntityTypeBuilder<TContext, TEntity> HasTable(string tableName)
        {
            InputValidator.ThrowIfNullOrWhitespace(
                tableName, 
                $"Parameter cannot be empty: {nameof(tableName)}.");

            this.entityConfig.TableName = tableName;
            this.contextOptions.UseTableName<TEntity>(tableName);

            return this;
        }

        /// <summary>
        /// Adds info for a Global Secondary Index to the configuration.
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        public IEntityTypeBuilder<TContext, TEntity> HasGlobalSecondaryIndex(
            string indexName,
            Expression<Func<TEntity, object>> hashKeyMemberExpression,
            Expression<Func<TEntity, object>> rangeKeyMemberExpression,
            long readCapacityUnits = 1,
            long writeCapacityUnits = 1)
        {
            InputValidator.ThrowIfAnyNullOrWhitespace(
                indexName, hashKeyMemberExpression, rangeKeyMemberExpression);

            var indexConfig = this.entityConfig
                .Indexes
                .FirstOrDefault(i => i.IndexName == indexName)
                ?? new GlobalSecondaryIndexConfiguration();
            indexConfig.IndexName = indexName;
            indexConfig.HashKeyMemberName = hashKeyMemberExpression.TryGetMemberName();
            indexConfig.HashKeyMemberType = hashKeyMemberExpression.TryGetMemberType();
            indexConfig.RangeKeyMemberName = rangeKeyMemberExpression.TryGetMemberName();
            indexConfig.RangeKeyMemberType = rangeKeyMemberExpression.TryGetMemberType();
            indexConfig.ReadCapacityUnits = readCapacityUnits;
            indexConfig.WriteCapacityUnits = writeCapacityUnits;

            this.entityConfig.Indexes.Add(indexConfig);

            return this;
        }

        /// <summary>
        /// Adds info for a Global Secondary Index to the configuration.
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        public IEntityTypeBuilder<TContext, TEntity> HasGlobalSecondaryIndex(
            Action<GlobalSecondaryIndexConfiguration> indexAction)
        {
            InputValidator.ThrowIfNull(indexAction, "indexAction cannot be null.");

            var newIndexConfig = new GlobalSecondaryIndexConfiguration();

            indexAction(newIndexConfig);

            InputValidator.ThrowIfAnyNullOrWhitespace(
                newIndexConfig.HashKeyMemberName,
                newIndexConfig.IndexName);

            if (newIndexConfig.HashKeyMemberType == null)
            {
                newIndexConfig.HashKeyMemberType = typeof(TEntity)
                    .GetProperty(newIndexConfig.HashKeyMemberName)
                    .PropertyType;
            }

            if (!string.IsNullOrWhiteSpace(newIndexConfig.RangeKeyMemberName) &&
                newIndexConfig.RangeKeyMemberType == null)
            {
                newIndexConfig.RangeKeyMemberType = typeof(TEntity)
                    .GetProperty(newIndexConfig.RangeKeyMemberName)
                    .PropertyType;
            }

            var existingIndexConfig = this.entityConfig
                .Indexes
                .FirstOrDefault(i => i.IndexName == newIndexConfig.IndexName);

            if (existingIndexConfig != null)
            {
                indexAction(existingIndexConfig);

                return this;
            }

            this.entityConfig.Indexes.Add(newIndexConfig);
            
            return this;
        }

        /// <summary>
        /// Specify the primary key for that entity type.
        /// </summary>
        public IEntityTypeBuilder<TContext, TEntity> HasPrimaryKey(
            Expression<Func<TEntity, object>> keyExpression)
        {
            InputValidator.ThrowIfNull(keyExpression, "keyExpression cannot be null.");

            this.entityConfig.HashKeyMemberExpression = keyExpression;
            this.entityConfig.HashKeyMemberName = keyExpression.TryGetMemberName();
            this.entityConfig.HashKeyMemberType = keyExpression.TryGetMemberType();

            return this;
        }

        public IEntityTypeBuilder<TContext, TEntity> HasSortKey(Expression<Func<TEntity, object>> keyExpression)
        {
            InputValidator.ThrowIfNull(keyExpression, "keyExpression cannot be null.");

            this.entityConfig.SortKeyMemberExpression = keyExpression;
            this.entityConfig.SortKeyMemberName = keyExpression.TryGetMemberName();
            this.entityConfig.SortKeyMemberType = keyExpression.TryGetMemberType();

            return this;
        }

        public IEntityTypeBuilder<TContext, TEntity> HasDynamicBilling()
        {
            this.entityConfig.HasDynamicBilling = true;
            return this;
        }

        /// <summary>
        /// Ignore that property when save the entity to the database. 
        /// All ignored members will be set to its default value before saving in the database.
        /// </summary>
        public IEntityTypeBuilder<TContext, TEntity> Ignore(
            Expression<Func<TEntity, object>> propertyExpression)
        {
            InputValidator.ThrowIfNull(propertyExpression);

            var memberName = propertyExpression.TryGetMemberName();

            this.entityConfig.IgnoredMembersNames.Add(memberName);
            this.entityConfig.IgnoredMembersExpressions.Add(propertyExpression);

            return this;
        }

        /// <summary>
        /// Ignore that property when save the entity to the database. 
        /// All ignored members will be set to its default value before saving in the database.
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        public IEntityTypeBuilder<TContext, TEntity> Ignore(string propertyName)
        {
            InputValidator.ThrowIfNullOrWhitespace(propertyName);

            this.entityConfig.IgnoredMembersNames.Add(propertyName);

            return this;
        }

        /// <summary>
        /// Specifies either the entity should be validated against its 
        /// configuration and its attributes before saving in the database ot not.
        /// True by default.
        /// </summary>
        public IEntityTypeBuilder<TContext, TEntity> ValidateOnSave(bool validate = true)
        {
            this.entityConfig.ValidateOnSave = validate;

            return this;
        }

        /// <summary>
        /// Returns a property builder for that entity member.
        /// </summary>
        public IPropertyTypeBuilder Property<TProperty>(
            Expression<Func<TEntity, TProperty>> propertyExpression)
        {
            InputValidator.ThrowIfNull(propertyExpression);

            var memberName = propertyExpression.TryGetMemberName();
            var propertyConfig = this.entityConfig
                .Properties
                .SingleOrDefault(p => p.MemberName == memberName);

            if (propertyConfig == null)
            {
                propertyConfig = new PropertyConfiguration<TEntity>(memberName);

                this.entityConfig.Properties.Add(propertyConfig);
            }

            return new PropertyTypeBuilder<TEntity>(propertyConfig);
        }

        /// <summary>
        /// Specify the ReadCapacityUnits for the entity's corresponding dynamo table.
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        public IEntityTypeBuilder<TContext, TEntity> HasReadCapacityUnits(long readCapacityUnits)
        {
            InputValidator.ThrowIfNotPositive(
                readCapacityUnits,
                "ReadCapacityUnits should be a positive integer.");

            this.entityConfig.ReadCapacityUnits = readCapacityUnits;

            return this;
        }

        /// <summary>
        /// Specify the WriteCapacityUnits for the entity's corresponding dynamo table.
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        public IEntityTypeBuilder<TContext, TEntity> HasWriteCapacityUnits(long writeCapacityUnits)
        {
            InputValidator.ThrowIfNotPositive(
                writeCapacityUnits,
                "WriteCapacityUnits should be a positive integer.");

            this.entityConfig.WriteCapacityUnits = writeCapacityUnits;

            return this;
        }

        public IEntityTypeBuilder<TContext, TEntity> HasTTL(Expression<Func<TEntity, object>> keyExpression)
        {
            this.entityConfig.TTLMemberName = keyExpression.TryGetMemberName();
            return this;
        }
    }
}
