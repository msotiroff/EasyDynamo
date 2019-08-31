using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using EasyDynamo.Abstractions;
using EasyDynamo.Config;
using EasyDynamo.Exceptions;
using EasyDynamo.Extensions;
using EasyDynamo.Tools.Validators;
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
        private readonly IIndexFactory indexFactory;
        private readonly IAttributeDefinitionFactory attributeDefinitionFactory;
        private readonly IIndexConfigurationFactory indexConfigurationFactory;
        private readonly IEntityConfigurationProvider entityConfigurationProvider;
        
        public TableCreator(
            IAmazonDynamoDB client,
            IIndexFactory indexFactory,
            IAttributeDefinitionFactory attributeDefinitionFactory,
            IIndexConfigurationFactory indexConfigurationFactory,
            IEntityConfigurationProvider entityConfigurationProvider)
        {
            this.client = client;
            this.indexFactory = indexFactory;
            this.attributeDefinitionFactory = attributeDefinitionFactory;
            this.indexConfigurationFactory = indexConfigurationFactory;
            this.entityConfigurationProvider = entityConfigurationProvider;
        }

        public async Task<string> CreateTableAsync(
            Type contextType, Type entityType, string tableName)
        {
            InputValidator.ThrowIfAnyNullOrWhitespace(entityType, tableName);

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
            var entityConfig = this.entityConfigurationProvider
                .TryGetEntityConfiguration(contextType, entityType);

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
                : this.indexFactory.CreateRequestIndexes(gsisConfiguration).ToList()
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
