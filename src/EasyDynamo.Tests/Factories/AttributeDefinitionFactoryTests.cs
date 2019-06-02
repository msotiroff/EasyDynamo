using Amazon.DynamoDBv2;
using EasyDynamo.Config;
using EasyDynamo.Factories;
using EasyDynamo.Tests.Fakes;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace EasyDynamo.Tests.Factories
{
    public class AttributeDefinitionFactoryTests
    {
        private const string HashKeyMemberName = "Id";
        private readonly ScalarAttributeType HashKeyAttributeType = ScalarAttributeType.S;
        private readonly AttributeDefinitionFactory factory;

        public AttributeDefinitionFactoryTests()
        {
            this.factory = new AttributeDefinitionFactory();
        }

        [Fact]
        public void CreateAttributeDefinitions_SetPrimaryKeyNameCorrectly()
        {
            var definitions = this.factory.CreateAttributeDefinitions(
                HashKeyMemberName, HashKeyAttributeType, null);

            Assert.Contains(definitions, d => d.AttributeName == HashKeyMemberName);
        }

        [Fact]
        public void CreateAttributeDefinitions_SetPrimaryKeyTypeCorrectly()
        {
            var definitions = this.factory.CreateAttributeDefinitions(
                HashKeyMemberName, HashKeyAttributeType, null);

            Assert.Contains(definitions, d => d.AttributeType == HashKeyAttributeType);
        }

        [Fact]
        public void CreateAttributeDefinitions_PrimaryKeyAttributeTypeIsNull_DoesNotAddDefinition()
        {
            var definitions = this.factory.CreateAttributeDefinitions(
                HashKeyMemberName, null, null);

            Assert.Empty(definitions);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("    ")]
        public void CreateAttributeDefinitions_PrimaryKeyNameIsEmpty_DoesNotAddDefinition(string empty)
        {
            var definitions = this.factory.CreateAttributeDefinitions(
                empty, HashKeyAttributeType, null);

            Assert.Empty(definitions);
        }

        [Fact]
        public void CreateAttributeDefinitions_GsiConfigurationPassed_CreateDefinitionWithCorrectHashKeyDefinition()
        {
            var gsiConfig = new GlobalSecondaryIndexConfiguration
            {
                HashKeyMemberName = nameof(FakeEntity.Title),
                HashKeyMemberType = typeof(string),
                IndexName = "GSI_Entities_Title_LAstModified",
                RangeKeyMemberName = nameof(FakeEntity.LastModified),
                RangeKeyMemberType = typeof(DateTime),
                ReadCapacityUnits = 3,
                WriteCapacityUnits = 5
            };
            var gsiConfigs = new List<GlobalSecondaryIndexConfiguration> { gsiConfig };
            var definitions = this.factory.CreateAttributeDefinitions(null, null, gsiConfigs);
            
            Assert.Contains(definitions, d => 
                d.AttributeName == gsiConfig.HashKeyMemberName && 
                d.AttributeType == ScalarAttributeType.S);
        }

        [Fact]
        public void CreateAttributeDefinitions_GsiConfigurationPassed_CreateDefinitionWithCorrectHashKeyType()
        {
            var gsiConfig = new GlobalSecondaryIndexConfiguration
            {
                HashKeyMemberName = nameof(FakeEntity.Title),
                HashKeyMemberType = typeof(string),
                IndexName = "GSI_Entities_Title_LAstModified",
                RangeKeyMemberName = nameof(FakeEntity.LastModified),
                RangeKeyMemberType = typeof(DateTime),
                ReadCapacityUnits = 3,
                WriteCapacityUnits = 5
            };
            var gsiConfigs = new List<GlobalSecondaryIndexConfiguration> { gsiConfig };
            var definitions = this.factory.CreateAttributeDefinitions(null, null, gsiConfigs);

            Assert.Contains(definitions, d =>
                d.AttributeName == gsiConfig.RangeKeyMemberName &&
                d.AttributeType == ScalarAttributeType.S);
        }

        [Theory]
        [InlineData(2, 4)]
        [InlineData(5, 10)]
        [InlineData(8, 16)]
        [InlineData(32, 64)]
        [InlineData(200, 400)]
        public void CreateAttributeDefinitions_GsiConfigurationPassed_CreateCorrectCountOfDefinitions(int gsiConfigsCount, int expectedCount)
        {
            var gsiConfigs = Enumerable.Range(1, gsiConfigsCount)
                .Select(index => new GlobalSecondaryIndexConfiguration
                {
                    HashKeyMemberName = $"Hash-{index}",
                    HashKeyMemberType  =typeof(string),
                    IndexName = $"Index_{index}",
                    RangeKeyMemberName = $"Range-{index}",
                    RangeKeyMemberType = typeof(int)
                });
            var definitions = this.factory.CreateAttributeDefinitions(null, null, gsiConfigs);

            Assert.Equal(expectedCount, definitions.Count());
        }
    }
}
