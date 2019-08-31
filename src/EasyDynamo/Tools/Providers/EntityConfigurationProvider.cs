using EasyDynamo.Abstractions;
using EasyDynamo.Core;
using EasyDynamo.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EasyDynamo.Tools.Providers
{
    public class EntityConfigurationProvider : IEntityConfigurationProvider
    {
        private readonly IEnumerable<IEntityConfiguration> allEntityConfigurations;

        public EntityConfigurationProvider(
            IEnumerable<IEntityConfiguration> entityConfigurations)
        {
            this.allEntityConfigurations = entityConfigurations;
        }

        public IEntityConfiguration<TEntity> GetEntityConfiguration<TContext, TEntity>()
            where TContext : DynamoContext
            where TEntity : class
        {
            var configuration = this.TryGetEntityConfiguration(typeof(TContext), typeof(TEntity));

            return configuration as IEntityConfiguration<TEntity>
                ?? throw new DynamoContextConfigurationException(
                    $"Could not resolve service of type " +
                    $"{typeof(IEntityConfiguration<TEntity>).FullName} " +
                    $"with context {typeof(TContext).FullName}");
        }

        public IEntityConfiguration GetEntityConfiguration(Type contextType, Type entityType)
        {
            var configuration = this.TryGetEntityConfiguration(contextType, entityType);

            return configuration
                ?? throw new DynamoContextConfigurationException(
                    $"Could not resolve service of type " +
                    $"{entityType.FullName} " +
                    $"with context {contextType.FullName}");
        }

        public IEntityConfiguration<TEntity> TryGetEntityConfiguration<TContext, TEntity>()
            where TContext : DynamoContext
            where TEntity : class
        {
            try
            {
                return this.GetEntityConfiguration<TContext, TEntity>();
            }
            catch
            {
                return default;
            }
        }

        public IEntityConfiguration TryGetEntityConfiguration(
            Type contextType, Type entityType)
        {
            return allEntityConfigurations.FirstOrDefault(ec => 
                ec.ContextType == contextType && ec.EntityType == entityType);
        }
    }
}
