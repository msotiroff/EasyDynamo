using System.Collections.Generic;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using EasyDynamo.Config;

namespace EasyDynamo.Abstractions
{
    public interface IAttributeDefinitionFactory
    {
        IEnumerable<AttributeDefinition> CreateAttributeDefinitions(
            string primaryKeyMemberName, 
            ScalarAttributeType primaryKeyMemberAttributeType, 
            IEnumerable<GlobalSecondaryIndexConfiguration> gsisConfiguration);
    }
}