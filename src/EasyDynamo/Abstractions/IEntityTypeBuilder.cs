using EasyDynamo.Config;
using System;
using System.Linq.Expressions;

namespace EasyDynamo.Abstractions
{
    public interface IEntityTypeBuilder<TEntity>
        where TEntity : class
    {
        IEntityTypeBuilder<TEntity> HasTable(string tableName);

        IEntityTypeBuilder<TEntity> HasGlobalSecondaryIndex(
            string indexName, 
            Expression<Func<TEntity, object>> hashKeyMemberExpression,
            Expression<Func<TEntity, object>> rangeKeyMemberExpression,
            long readCapacityUnits = 1,
            long writeCapacityUnits = 1);

        IEntityTypeBuilder<TEntity> HasGlobalSecondaryIndex(
            Action<GlobalSecondaryIndexConfiguration> indexAction);

        IEntityTypeBuilder<TEntity> HasPrimaryKey(
            Expression<Func<TEntity, object>> keyExpression);

        IEntityTypeBuilder<TEntity> HasReadCapacityUnits(long readCapacityUnits);

        IEntityTypeBuilder<TEntity> HasWriteCapacityUnits(long writeCapacityUnits);

        IEntityTypeBuilder<TEntity> Ignore(
            Expression<Func<TEntity, object>> propertyExpression);

        IEntityTypeBuilder<TEntity> Ignore(string propertyName);

        IEntityTypeBuilder<TEntity> ValidateOnSave(bool validate = true);

        IPropertyTypeBuilder Property<TProperty>(
            Expression<Func<TEntity, TProperty>> propertyExpression);
    }
}