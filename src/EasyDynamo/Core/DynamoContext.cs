using Amazon.DynamoDBv2.DataModel;
using EasyDynamo.Abstractions;
using EasyDynamo.Attributes;
using EasyDynamo.Builders;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Reflection;

namespace EasyDynamo.Core
{
    public abstract class DynamoContext
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IDependencyResolver dependencyResolver;
        private readonly IDynamoContextOptions options;

        protected DynamoContext(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
            this.options = serviceProvider
                .GetRequiredService<IDynamoContextOptionsProvider>()
                .GetContextOptions(this.GetType());
            this.Database = this.GetDatabaseFacadeInstance();
            this.ListAllTablesByDbSets();
            this.InstantiateAllSets();
        }

        protected DynamoContext(IDependencyResolver dependencyResolver)
        {
            this.dependencyResolver = dependencyResolver;
            this.options = this.dependencyResolver
                .GetDependency<IDynamoContextOptionsProvider>()
                .GetContextOptions(this.GetType());
            this.Database = this.GetDatabaseFacadeInstance();
            this.ListAllTablesByDbSets();
            this.InstantiateAllSets();
        }

        public DatabaseFacade Database { get; }

        /// <summary>
        /// Use to configure the Amazon.DynamoDBv2.IAmazonDynamoDB client.
        /// </summary>
        protected virtual void OnConfiguring(
            DynamoContextOptionsBuilder builder, 
            IConfiguration configuration)
        {
            return;
        }

        /// <summary>
        /// Use to configure the database models.
        /// </summary>
        protected virtual void OnModelCreating<TContext>(
            ModelBuilder<TContext> builder, IConfiguration configuration)
            where TContext : DynamoContext
        {
            return;
        }

        private void InstantiateAllSets()
        {
            var allSets = this.GetType()
                .GetProperties()
                .Where(pi => pi.PropertyType
                    .GetCustomAttribute<DynamoDbSetAttribute>(true) != null)
                .Select(pi => new
                {
                    PropertyInfo = pi,
                    PropertyInstance = this.GetGenericPropertyInstance(pi)
                })
                .ToList();

                allSets.ForEach(a => a.PropertyInfo.SetValue(this, a.PropertyInstance));
        }

        private object GetGenericPropertyInstance(PropertyInfo propertyInfo)
        {
            var propertyType = propertyInfo.PropertyType;

            if (!propertyType.IsGenericType)
            {
                throw new InvalidOperationException($"{propertyType} is not a generic type.");
            }

            var instance = this.serviceProvider?.GetService(propertyType)
                ?? this.dependencyResolver?.GetDependency(propertyType);

            if (instance != null)
            {
                return instance;
            }

            var constructor = propertyType
                .GetConstructors(BindingFlags.Public | BindingFlags.Instance)
                .Single();
            var constructorParams = constructor
                .GetParameters()
                .Select(pi => pi.ParameterType)
                .Select(t => 
                    this.serviceProvider?.GetRequiredService(t) 
                    ?? this.dependencyResolver.GetDependency(t))
                .ToList();

            constructorParams.Add(this.GetType());

            instance = constructor.Invoke(constructorParams.ToArray());

            if (instance != null)
            {
                return instance;
            }

            throw new InvalidOperationException(
                $"{propertyType.FullName} could not be instantiated.");
        }

        private void ListAllTablesByDbSets()
        {
            var allDbSets = this.GetType()
                .GetProperties()
                .Where(pi => pi.PropertyType.GetCustomAttribute<DynamoDbSetAttribute>() != null)
                .ToList();

            foreach (var dbSetInfo in allDbSets)
            {
                var entityType = dbSetInfo.PropertyType.GetGenericArguments().First();

                if (this.options.TableNameByEntityTypes.ContainsKey(entityType))
                {
                    continue;
                }

                var tableNameFromAttribute = entityType
                    .GetCustomAttribute<DynamoDBTableAttribute>()
                    ?.TableName;

                this.options.TableNameByEntityTypes[entityType] = tableNameFromAttribute 
                    ?? entityType.Name;
            }
        }

        private DatabaseFacade GetDatabaseFacadeInstance()
        {
            var constructor = typeof(DatabaseFacade)
                .GetConstructors()
                .First();
            var constructorParameters = constructor
                .GetParameters()
                .Select(pi => 
                    this.serviceProvider?.GetService(pi.ParameterType)
                    ?? this.dependencyResolver?.GetDependency(pi.ParameterType))
                .Where(i => i != null)
                .ToList();

            constructorParameters.Add(this.GetType());

            var instance = constructor.Invoke(constructorParameters.ToArray());

            return instance as DatabaseFacade;
        }
    }
}
