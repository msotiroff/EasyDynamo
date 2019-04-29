using Amazon.DynamoDBv2.DataModel;
using EasyDynamo.Attributes;
using EasyDynamo.Builders;
using EasyDynamo.Config;
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
        private readonly DynamoContextOptions options;

        protected DynamoContext(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
            this.options = DynamoContextOptions.Instance;
            this.Database = this.serviceProvider.GetRequiredService<DatabaseFacade>();
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
        protected virtual void OnModelCreating(
            ModelBuilder builder, IConfiguration configuration)
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

            var constructor = propertyType
                .GetConstructors(BindingFlags.Public | BindingFlags.Instance)
                .Single();
            var constructorParams = constructor
                .GetParameters()
                .Select(pi => pi.ParameterType)
                .Select(t => this.serviceProvider.GetRequiredService(t))
                .ToArray();
            var instance = constructor.Invoke(constructorParams);

            return instance;
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
    }
}
