using EasyDynamo.Config;
using EasyDynamo.Core;
using System;
using System.Linq.Expressions;

namespace EasyDynamo.Abstractions
{
    public interface IEntityTypeBuilder<TContext, TEntity>
        where TContext : DynamoContext
        where TEntity : class
    {
        IEntityTypeBuilder<TContext, TEntity> HasTable(string tableName);

        IEntityTypeBuilder<TContext, TEntity> HasGlobalSecondaryIndex(
            string indexName, 
            Expression<Func<TEntity, object>> hashKeyMemberExpression,
            Expression<Func<TEntity, object>> rangeKeyMemberExpression,
            long readCapacityUnits = 1,
            long writeCapacityUnits = 1);

        IEntityTypeBuilder<TContext, TEntity> HasGlobalSecondaryIndex(
            Action<GlobalSecondaryIndexConfiguration> indexAction);

        IEntityTypeBuilder<TContext, TEntity> HasPrimaryKey(
            Expression<Func<TEntity, object>> keyExpression);

        IEntityTypeBuilder<TContext, TEntity> HasReadCapacityUnits(long readCapacityUnits);

        IEntityTypeBuilder<TContext, TEntity> HasWriteCapacityUnits(long writeCapacityUnits);

        IEntityTypeBuilder<TContext, TEntity> Ignore(
            Expression<Func<TEntity, object>> propertyExpression);

        IEntityTypeBuilder<TContext, TEntity> Ignore(string propertyName);

        IEntityTypeBuilder<TContext, TEntity> ValidateOnSave(bool validate = true);

        IPropertyTypeBuilder Property<TProperty>(
            Expression<Func<TEntity, TProperty>> propertyExpression);
    }
}