using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using EasyDynamo.Abstractions;
using EasyDynamo.Builders;
using EasyDynamo.Config;
using EasyDynamo.Exceptions;
using EasyDynamo.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace EasyDynamo.Tools
{
    public class TableCreator : ITableCreator
    {
        private readonly IAmazonDynamoDB client;
        private readonly IIndexFactory indexCreator;
        private readonly IAttributeDefinitionFactory attributeDefinitionFactory;
        private readonly IIndexConfigurationFactory indexConfigurationFactory;
        
        public TableCreator(
            IAmazonDynamoDB client,
            IIndexFactory indexCreator,
            IAttributeDefinitionFactory attributeDefinitionFactory,
            IIndexConfigurationFactory indexConfigurationFactory)
        {
            this.client = client;
            this.indexCreator = indexCreator;
            this.attributeDefinitionFactory = attributeDefinitionFactory;
            this.indexConfigurationFactory = indexConfigurationFactory;
        }

        public async Task<string> CreateTableAsync(Type entityType, string tableName)
        {
            var configurationsByEntityTypes = ModelBuilder
                .Instance
                .EntityConfigurationByEntityTypes;
            var dynamoDbTableAttribute = entityType.GetCustomAttribute<DynamoDBTableAttribute>(true);
            tableName = dynamoDbTableAttribute?.TableName ?? tableName;
            var hashKeyMember = entityType
                .GetProperties()
                .SingleOrDefault(pi => pi.GetCustomAttributes()
                    .Any(attr => attr.GetType() == typeof(DynamoDBHashKeyAttribute)));
            var hashKeyMemberAttribute = entityType
                .GetProperty(hashKeyMember?.Name ?? string.Empty)
                ?.GetCustomAttribute<DynamoDBHashKeyAttribute>(true);
            var hashKeyMemberType = entityType
                .GetProperty(hashKeyMember?.Name ?? string.Empty)
                ?.PropertyType;
            var entityConfigRequired = hashKeyMember == null;

            var entityConfig = configurationsByEntityTypes.ContainsKey(entityType)
                ? configurationsByEntityTypes[entityType]
                : null;

            if (entityConfigRequired && entityConfig == null)
            {
                throw new DynamoContextConfigurationException(string.Format(
                            ExceptionMessage.EntityConfigurationNotFound, entityType.FullName));
            }

            var hashKeyMemberName = entityConfig?.HashKeyMemberName
                ?? hashKeyMemberAttribute.AttributeName
                ?? hashKeyMember.Name;
            var hashKeyMemberAttributeType = new ScalarAttributeType(
                hashKeyMemberType != null
                ? Constants.AttributeTypesMap[hashKeyMemberType]
                : Constants.AttributeTypesMap[entityConfig.HashKeyMemberType]);
            var readCapacityUnits = entityConfig?.ReadCapacityUnits
                ?? Constants.DefaultReadCapacityUnits;
            var writeCapacityUnits = entityConfig?.WriteCapacityUnits
                ?? Constants.DefaultWriteCapacityUnits;
            var gsisConfiguration = (entityConfig?.Indexes?.Count ?? 0) > 0
                ? entityConfig.Indexes
                : this.indexConfigurationFactory.CreateIndexConfigByAttributes(entityType);

            var request = new CreateTableRequest
            {
                TableName = tableName,
                AttributeDefinitions = this.attributeDefinitionFactory.CreateAttributeDefinitions(
                    hashKeyMemberName, hashKeyMemberAttributeType, gsisConfiguration).ToList(),
                KeySchema = new List<KeySchemaElement>
                {
                    new KeySchemaElement(hashKeyMemberName, KeyType.HASH)
                },
                ProvisionedThroughput = new ProvisionedThroughput
                {
                    ReadCapacityUnits = readCapacityUnits,
                    WriteCapacityUnits = writeCapacityUnits
                },
                GlobalSecondaryIndexes = gsisConfiguration.Count() == 0
                ? null
                : this.indexCreator.CreateRequestIndexes(gsisConfiguration).ToList()
            };

            try
            {
                var response = await this.client.CreateTableAsync(request);

                if (!response.HttpStatusCode.IsSuccessful())
                {
                    throw new CreateTableFailedException(
                        response.ResponseMetadata.Metadata.JoinByNewLine());
                }

                return request.TableName;
            }
            catch (Exception ex)
            {
                throw new CreateTableFailedException("Failed to create a table.", ex);
            }
        }
    }
}
