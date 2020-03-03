using Amazon.DynamoDBv2.DataModel;
using EasyDynamo.Abstractions;
using EasyDynamo.Attributes;
using EasyDynamo.Builders;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Reflection;

namespace EasyDynamo.Core
{
    public abstract class DynamoContext
    {
        private readonly IDependencyResolver dependencyResolver;
        private readonly IDynamoContextOptions options;

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
        }

        /// <summary>
        /// Use to configure the database models.
        /// </summary>
        protected virtual void OnModelCreating(ModelBuilder builder, IConfiguration configuration)
        {
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
            var implementationType = propertyType;

            if (!propertyType.IsGenericType)
            {
                throw new InvalidOperationException($"{propertyType} is not a generic type.");
            }

            if (propertyType.IsAbstract)
            {
                implementationType = this.dependencyResolver
                    .TryGetDependency(propertyType)
                    ?.GetType()
                    ?? typeof(DynamoDbSet<>)
                        .MakeGenericType(propertyType.GetGenericArguments());
            }

            var instance = this.dependencyResolver.TryGetDependency(implementationType);

            if (instance != null)
            {
                return instance;
            }

            var constructor = implementationType
                .GetConstructors(BindingFlags.Public | BindingFlags.Instance)
                .Single();
            var constructorParams = constructor
                .GetParameters()
                .Select(pi => pi.ParameterType)
                .Select(t => this.dependencyResolver.TryGetDependency(t))
                .Where(dep => dep != null)
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
                .Select(pi => this.dependencyResolver.TryGetDependency(pi.ParameterType))
                .Where(i => i != null)
                .ToList();

            constructorParameters.Add(this.GetType());

            var instance = constructor.Invoke(constructorParameters.ToArray());

            return instance as DatabaseFacade;
        }
    }
}
