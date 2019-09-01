using EasyDynamo.Abstractions;
using EasyDynamo.Config;
using EasyDynamo.Core;
using EasyDynamo.Tools.Validators;
using System;
using System.Collections.Generic;

namespace EasyDynamo.Builders
{
    public class ModelBuilder
    {
        private readonly IDynamoContextOptions contextOptions;
        private readonly Dictionary<Type, object> entityBuildersByEntityType;

        protected internal ModelBuilder(IDynamoContextOptions contextOptions)
        {
            this.contextOptions = contextOptions;
            this.entityBuildersByEntityType = new Dictionary<Type, object>();
            this.EntityConfigurations = new Dictionary<Type, IEntityConfiguration>();
        }
        
        protected internal IDictionary<Type, IEntityConfiguration> EntityConfigurations { get; }

        /// <summary>
        /// Applies a specific configuration for a given entity.
        /// Usage: Create a class that implements IEntityTypeConfiguration 
        /// and build all configurations in the Configure method.
        /// Then call ApplyConfiguration with a new instance of that implementation class.
        /// </summary>
        public ModelBuilder ApplyConfiguration<TContext, TEntity>(
            IEntityTypeConfiguration<TContext, TEntity> configuration)
            where TContext : DynamoContext
            where TEntity : class, new()
        {
            InputValidator.ThrowIfNull(configuration, "configuration connot be null.");

            var entityBuilder = this.GetEntityBuilder<TContext, TEntity>();
            
            configuration.Configure(entityBuilder);

            return this;
        }

        /// <summary>
        /// Returns a builder for a specified entity type.
        /// </summary>
        public IEntityTypeBuilder<TContext, TEntity> Entity<TContext, TEntity>()
            where TContext : DynamoContext
            where TEntity : class, new()
        {
            return this.GetEntityBuilder<TContext, TEntity>();
        }

        /// <summary>
        /// Applies a specific configuration for a given entity. 
        /// Insert the entity configuration in the buildAction parameter.
        /// </summary>
        public ModelBuilder Entity<TContext, TEntity>(
            Action<IEntityTypeBuilder<TContext, TEntity>> buildAction)
            where TContext : DynamoContext
            where TEntity : class, new()
        {
            InputValidator.ThrowIfNull(buildAction, "buildAction cannot be null.");

            var entityBuilder = this.GetEntityBuilder<TContext, TEntity>();

            buildAction(entityBuilder);

            return this;
        }

        private IEntityTypeBuilder<TContext, TEntity> GetEntityBuilder<TContext, TEntity>()
            where TContext : DynamoContext
            where TEntity : class, new()
        {
            var entityConfig = default(EntityConfiguration<TContext, TEntity>);

            if (this.EntityConfigurations.ContainsKey(typeof(TEntity)))
            {
                entityConfig = (EntityConfiguration<TContext, TEntity>)
                    this.EntityConfigurations[typeof(TEntity)];
            }

            if (entityConfig == null)
            {
                entityConfig = new EntityConfiguration<TContext, TEntity>();

                this.EntityConfigurations[typeof(TEntity)] = entityConfig;
            }

            var entityBuilder = default(EntityTypeBuilder<TContext, TEntity>);

            if (this.entityBuildersByEntityType.ContainsKey(typeof(TEntity)))
            {
                entityBuilder = (EntityTypeBuilder<TContext, TEntity>)
                    this.entityBuildersByEntityType[typeof(TEntity)];
            }

            if (entityBuilder == null)
            {
                entityBuilder = new EntityTypeBuilder<TContext, TEntity>(
                    entityConfig, this.contextOptions);

                this.entityBuildersByEntityType[typeof(TEntity)] = entityBuilder;
            }

            return entityBuilder;
        }
    }
}
