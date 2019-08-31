using System;
using EasyDynamo.Core;

namespace EasyDynamo.Abstractions
{
    public interface IEntityConfigurationProvider
    {
        IEntityConfiguration<TEntity> GetEntityConfiguration<TContext, TEntity>()
            where TContext : DynamoContext
            where TEntity : class;

        IEntityConfiguration<TEntity> TryGetEntityConfiguration<TContext, TEntity>()
            where TContext : DynamoContext
            where TEntity : class;

        IEntityConfiguration GetEntityConfiguration(Type contextType, Type entityType);

        IEntityConfiguration TryGetEntityConfiguration(Type contextType, Type entityType);
    }
}
