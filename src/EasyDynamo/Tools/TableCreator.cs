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
using System.Threading;
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
          
            var entityConfig = this.entityConfigurationProvider
                .TryGetEntityConfiguration(contextType, entityType);
            
            var hashKeyInfo = GetHashKeyInfo(entityConfig, entityType);
            
            var readCapacityUnits = entityConfig?.ReadCapacityUnits
                                    ?? Constants.DefaultReadCapacityUnits;
            var writeCapacityUnits = entityConfig?.WriteCapacityUnits
                                     ?? Constants.DefaultWriteCapacityUnits;

            var gsisConfiguration = GetGlobalSecondaryIndices(entityConfig, entityType);

            var request = new CreateTableRequest
            {
                TableName = tableName,
                AttributeDefinitions = this.attributeDefinitionFactory.CreateAttributeDefinitions(
                    hashKeyInfo.HashKeyMemberName, hashKeyInfo.HashKeyMemberAttributeType, gsisConfiguration).ToList(),
                KeySchema = new List<KeySchemaElement>
                {
                    new KeySchemaElement(hashKeyInfo.HashKeyMemberName, KeyType.HASH)
                },
                GlobalSecondaryIndexes = gsisConfiguration.Count() == 0
                    ? null
                    : this.indexFactory.CreateRequestIndexes(gsisConfiguration).ToList(),
                
            };
            
            if(entityConfig?.HasDynamicBilling  ?? false)
            {
                request.BillingMode = BillingMode.PAY_PER_REQUEST;    
            }
            else
            {
                request.ProvisionedThroughput = new ProvisionedThroughput
                {
                    ReadCapacityUnits = readCapacityUnits,
                    WriteCapacityUnits = writeCapacityUnits
                };
            }
            

            try
            {
                var response = await this.client.CreateTableAsync(request);

                if (!response.HttpStatusCode.IsSuccessful())
                {
                    throw new CreateTableFailedException(
                        response.ResponseMetadata.Metadata.JoinByNewLine());
                }

                if (string.IsNullOrWhiteSpace(entityConfig.TTLMemberName))
                {
                    return request.TableName;
                }
                // Before we can set the TTL we need the table fully created
                while (true)
                {
                    var checkTables = await this.client.DescribeTableAsync(tableName);
                    if (checkTables.Table.TableStatus == TableStatus.ACTIVE)
                    {
                        break;
                    }
                    Thread.Sleep(500);
                }
                await this.client.UpdateTimeToLiveAsync(new UpdateTimeToLiveRequest
                {
                    TableName = tableName,
                    TimeToLiveSpecification = new TimeToLiveSpecification
                    {
                        AttributeName = entityConfig.TTLMemberName,
                        Enabled = true
                    }
                });
                return request.TableName;
            }
            catch (Exception ex)
            {
                throw new CreateTableFailedException("Failed to create a table.", ex);
            }
        }

        public async Task UpdateTableAsync(Type contextType, Type entityType, string tableName)
        {
            var tableDescription = this.client.DescribeTableAsync(tableName);
            var secondaryIndexes = tableDescription.Result.Table.GlobalSecondaryIndexes;

            var entityConfig = this.entityConfigurationProvider
                .TryGetEntityConfiguration(contextType, entityType);
            
         

            var gsisConfiguration = GetGlobalSecondaryIndices(entityConfig, entityType);


            foreach (var index in secondaryIndexes)
            {
                var existingIndex = (from x in gsisConfiguration
                    where x.IndexName == index.IndexName
                    select x).FirstOrDefault();
                // There is an index in Dynamo that is not in our config, we need to delete it
                if (existingIndex == null)
                {
                    await this.client.UpdateTableAsync(new UpdateTableRequest
                    {
                        TableName = tableName,
                        GlobalSecondaryIndexUpdates = new List<GlobalSecondaryIndexUpdate>
                        {
                            new GlobalSecondaryIndexUpdate
                            {
                                Delete = new DeleteGlobalSecondaryIndexAction
                                {
                                    IndexName = index.IndexName
                                }
                            }
                        }

                    });
                }
            }
            var hashKeyInfo = GetHashKeyInfo(entityConfig, entityType);

            foreach (var index in gsisConfiguration)
            {
                var existingIndex = (from x in secondaryIndexes
                    where x.IndexName == index.IndexName
                    select x).FirstOrDefault();
                // There is an index in schema that is not in DynamoDb
                if (existingIndex != null)
                {
                    continue;
                }
                var gsisConfig =this.indexFactory.CreateRequestIndexes(new[] {index}).ElementAt(0);
                await this.client.UpdateTableAsync(new UpdateTableRequest
                {
                    TableName = tableName,
                    GlobalSecondaryIndexUpdates = new List<GlobalSecondaryIndexUpdate>
                    {
                        new GlobalSecondaryIndexUpdate
                        {
                            Create = new CreateGlobalSecondaryIndexAction
                            {
                                IndexName = index.IndexName,
                                KeySchema = gsisConfig.KeySchema,
                                Projection = gsisConfig.Projection
                            }
                        }
                    },
                    AttributeDefinitions = this.attributeDefinitionFactory.CreateAttributeDefinitions(
                        hashKeyInfo.HashKeyMemberName, hashKeyInfo.HashKeyMemberAttributeType, gsisConfiguration).ToList(),

                });
            }

            if (!string.IsNullOrWhiteSpace(entityConfig.TTLMemberName))
            {
                var result = await this.client.DescribeTimeToLiveAsync(new DescribeTimeToLiveRequest
                {
                    TableName = tableName
                });
                if(result.TimeToLiveDescription.TimeToLiveStatus == TimeToLiveStatus.ENABLED || result.TimeToLiveDescription.TimeToLiveStatus == TimeToLiveStatus.ENABLING)
                {
                    return;
                }
                await this.client.UpdateTimeToLiveAsync(new UpdateTimeToLiveRequest
                {
                    TableName = tableName,
                    TimeToLiveSpecification = new TimeToLiveSpecification
                    {
                        AttributeName = entityConfig.TTLMemberName,
                        Enabled = true
                    }
                });
            }
        }

        private List<GlobalSecondaryIndexConfiguration> GetGlobalSecondaryIndices(IEntityConfiguration entityConfig, Type entityType)
        {
            return ((entityConfig?.Indexes?.Count ?? 0) > 0
              ? entityConfig.Indexes
              : this.indexConfigurationFactory.CreateIndexConfigByAttributes(entityType)).ToList();
        }

        private (string HashKeyMemberName, Type HashKeyMemberType, ScalarAttributeType HashKeyMemberAttributeType ) GetHashKeyInfo(IEntityConfiguration entityConfig,Type entityType)
        {
            var hashKeyMember = entityType
             .GetProperties()
             .SingleOrDefault(pi => pi.GetCustomAttributes()
                 .Any(attr => attr.GetType() == typeof(DynamoDBHashKeyAttribute)));
            if (hashKeyMember == null && entityConfig == null)
            {
                throw new DynamoContextConfigurationException(string.Format(
                    ExceptionMessage.EntityConfigurationNotFound, entityType.FullName));
            }
            var hashKeyMemberAttribute = entityType
                .GetProperty(hashKeyMember?.Name ?? string.Empty)
                ?.GetCustomAttribute<DynamoDBHashKeyAttribute>(true);
            var hashKeyMemberType = entityType
                .GetProperty(hashKeyMember?.Name ?? string.Empty)
                ?.PropertyType;

            var hashKeyMemberName = entityConfig?.HashKeyMemberName
                                    ?? hashKeyMemberAttribute.AttributeName
                                    ?? hashKeyMember.Name;
            var hashKeyMemberAttributeType = new ScalarAttributeType(
                hashKeyMemberType != null
                    ? Constants.AttributeTypesMap[hashKeyMemberType]
                    : Constants.AttributeTypesMap[entityConfig.HashKeyMemberType]);
            return (hashKeyMemberName, hashKeyMemberType, hashKeyMemberAttributeType);
        }
    }
}

