using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using EasyDynamo.Abstractions;
using EasyDynamo.Config;
using System.Collections.Generic;
using System.Linq;

namespace EasyDynamo.Factories
{
    public class AttributeDefinitionFactory : IAttributeDefinitionFactory
    {
        public IEnumerable<AttributeDefinition> CreateAttributeDefinitions(
            string primaryKeyMemberName,
            ScalarAttributeType primaryKeyMemberAttributeType,
            IEnumerable<GlobalSecondaryIndexConfiguration> gsisConfiguration)
        {
            gsisConfiguration = gsisConfiguration 
                ?? Enumerable.Empty<GlobalSecondaryIndexConfiguration>();

            var definitions = new List<AttributeDefinition>();

            if (!string.IsNullOrWhiteSpace(primaryKeyMemberName) && 
                primaryKeyMemberAttributeType != null)
            {
                definitions.Add(new AttributeDefinition
                {
                    AttributeName = primaryKeyMemberName,
                    AttributeType = primaryKeyMemberAttributeType
                });
            }

            foreach (var gsiConfig in gsisConfiguration)
            {
                if (definitions.All(def => def.AttributeName != gsiConfig.HashKeyMemberName))
                {
                    definitions.Add(new AttributeDefinition
                    {
                        AttributeName = gsiConfig.HashKeyMemberName,
                        AttributeType = Constants.AttributeTypesMap[gsiConfig.HashKeyMemberType]
                    });
                }

                if (!string.IsNullOrWhiteSpace(gsiConfig.RangeKeyMemberName) &&
                    definitions.All(def => def.AttributeName != gsiConfig.RangeKeyMemberName))
                {
                    definitions.Add(new AttributeDefinition
                    {
                        AttributeName = gsiConfig.RangeKeyMemberName,
                        AttributeType = Constants.AttributeTypesMap[gsiConfig.RangeKeyMemberType]
                    });
                }
            }

            return definitions;
        }
    }
}
