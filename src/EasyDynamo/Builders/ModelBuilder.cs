using EasyDynamo.Abstractions;
using EasyDynamo.Config;
using System;
using System.Collections.Generic;

namespace EasyDynamo.Builders
{
    public class ModelBuilder
    {
        private static volatile ModelBuilder instance;
        private static readonly object instanceLoker = new object();
        
        private ModelBuilder()
        {
            this.EntityConfigurationByEntityTypes = new Dictionary<Type, IEntityConfiguration>();
        }
        
        internal static ModelBuilder Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (instanceLoker)
                    {
                        if (instance == null)
                        {
                            instance = new ModelBuilder();
                        }
                    }
                }

                return instance;
            }
        }

        internal IDictionary<Type, IEntityConfiguration> EntityConfigurationByEntityTypes { get; }

        /// <summary>
        /// Applies a specific configuration for a given entity.
        /// Usage: Create a class that implements IEntityTypeConfiguration 
        /// and build all configurations in the Configure method.
        /// Then call ApplyConfiguration with a new instance of that implementation class.
        /// </summary>
        public ModelBuilder ApplyConfiguration<TEntity>(
            IEntityTypeConfiguration<TEntity> configuration) where TEntity : class, new()
        {
            var entityBuilder = EntityTypeBuilder<TEntity>.Instance;
            
            configuration.Configure(entityBuilder);

            return this;
        }

        /// <summary>
        /// Returns a builder for a specified entity type.
        /// </summary>
        public IEntityTypeBuilder<TEntity> Entity<TEntity>() where TEntity : class, new()
        {
            var entityBuilder = EntityTypeBuilder<TEntity>.Instance;

            this.EntityConfigurationByEntityTypes[typeof(TEntity)] =
                EntityConfiguration<TEntity>.Instance;

            return entityBuilder;
        }

        /// <summary>
        /// Applies a specific configuration for a given entity. 
        /// Insert the entity configuration in the buildAction parameter.
        /// </summary>
        public ModelBuilder Entity<TEntity>(
            Action<IEntityTypeBuilder<TEntity>> buildAction) 
            where TEntity : class, new()
        {
            var entityBuilder = EntityTypeBuilder<TEntity>.Instance;

            this.EntityConfigurationByEntityTypes[typeof(TEntity)] = 
                EntityConfiguration<TEntity>.Instance;
            
            buildAction(entityBuilder);

            return this;
        }
    }
}
