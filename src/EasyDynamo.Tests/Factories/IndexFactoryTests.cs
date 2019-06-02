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
    public class IndexFactoryTests
    {
        private readonly IndexFactory factory;

        public IndexFactoryTests()
        {
            this.factory = new IndexFactory();
        }

        [Fact]
        public void CreateRequestIndexes_NullPassed_ReturnsEmptyCollection()
        {
            var requestIndexes = this.factory.CreateRequestIndexes(null);

            Assert.Empty(requestIndexes);
        }

        [Fact]
        public void CreateRequestIndexes_EmptyCollectionPassed_ReturnsEmptyCollection()
        {
            var requestIndexes = this.factory.CreateRequestIndexes(null);

            Assert.Empty(requestIndexes);
        }

        [Fact]
        public void CreateRequestIndexes_OneConfig_ReturnsSingleRequestIndex()
        {
            var gsiConfig = new GlobalSecondaryIndexConfiguration
            {
                HashKeyMemberName = nameof(FakeEntity.Title),
                HashKeyMemberType = typeof(string),
                IndexName = "GSI_Entities_Title_LastModified",
                RangeKeyMemberName = nameof(FakeEntity.LastModified),
                RangeKeyMemberType = typeof(DateTime),
                ReadCapacityUnits = 3,
                WriteCapacityUnits = 5
            };
            var gsiConfigs = new List<GlobalSecondaryIndexConfiguration> { gsiConfig };
            var requestIndexes = this.factory.CreateRequestIndexes(gsiConfigs);

            Assert.Single(requestIndexes);
        }

        [Fact]
        public void CreateRequestIndexes_OneConfig_ReturnsRequestIndexWithCorrectIndexName()
        {
            var gsiConfig = new GlobalSecondaryIndexConfiguration
            {
                HashKeyMemberName = nameof(FakeEntity.Title),
                HashKeyMemberType = typeof(string),
                IndexName = "GSI_Entities_Title_LastModified",
                RangeKeyMemberName = nameof(FakeEntity.LastModified),
                RangeKeyMemberType = typeof(DateTime),
                ReadCapacityUnits = 3,
                WriteCapacityUnits = 5
            };
            var gsiConfigs = new List<GlobalSecondaryIndexConfiguration> { gsiConfig };
            var requestIndexes = this.factory.CreateRequestIndexes(gsiConfigs);
            var resultIndex = requestIndexes.Single();

            Assert.Equal(gsiConfig.IndexName, resultIndex.IndexName);
        }

        [Fact]
        public void CreateRequestIndexes_OneConfig_ReturnsRequestIndexWithProjectionTypeAll()
        {
            var gsiConfig = new GlobalSecondaryIndexConfiguration
            {
                HashKeyMemberName = nameof(FakeEntity.Title),
                HashKeyMemberType = typeof(string),
                IndexName = "GSI_Entities_Title_LastModified",
                RangeKeyMemberName = nameof(FakeEntity.LastModified),
                RangeKeyMemberType = typeof(DateTime),
                ReadCapacityUnits = 3,
                WriteCapacityUnits = 5
            };
            var gsiConfigs = new List<GlobalSecondaryIndexConfiguration> { gsiConfig };
            var requestIndexes = this.factory.CreateRequestIndexes(gsiConfigs);
            var resultIndex = requestIndexes.Single();

            Assert.Equal(ProjectionType.ALL, resultIndex.Projection.ProjectionType);
        }

        [Fact]
        public void CreateRequestIndexes_OneConfig_ReturnsRequestIndexWithCorrectReadCapacityUnits()
        {
            var gsiConfig = new GlobalSecondaryIndexConfiguration
            {
                HashKeyMemberName = nameof(FakeEntity.Title),
                HashKeyMemberType = typeof(string),
                IndexName = "GSI_Entities_Title_LastModified",
                RangeKeyMemberName = nameof(FakeEntity.LastModified),
                RangeKeyMemberType = typeof(DateTime),
                ReadCapacityUnits = 3,
                WriteCapacityUnits = 5
            };
            var gsiConfigs = new List<GlobalSecondaryIndexConfiguration> { gsiConfig };
            var requestIndexes = this.factory.CreateRequestIndexes(gsiConfigs);
            var resultIndex = requestIndexes.Single();

            Assert.Equal(
                gsiConfig.ReadCapacityUnits, 
                resultIndex.ProvisionedThroughput.ReadCapacityUnits);
        }

        [Fact]
        public void CreateRequestIndexes_OneConfig_ReturnsRequestIndexWithCorrectWriteCapacityUnits()
        {
            var gsiConfig = new GlobalSecondaryIndexConfiguration
            {
                HashKeyMemberName = nameof(FakeEntity.Title),
                HashKeyMemberType = typeof(string),
                IndexName = "GSI_Entities_Title_LastModified",
                RangeKeyMemberName = nameof(FakeEntity.LastModified),
                RangeKeyMemberType = typeof(DateTime),
                ReadCapacityUnits = 3,
                WriteCapacityUnits = 5
            };
            var gsiConfigs = new List<GlobalSecondaryIndexConfiguration> { gsiConfig };
            var requestIndexes = this.factory.CreateRequestIndexes(gsiConfigs);
            var resultIndex = requestIndexes.Single();

            Assert.Equal(
                gsiConfig.WriteCapacityUnits,
                resultIndex.ProvisionedThroughput.WriteCapacityUnits);
        }

        [Fact]
        public void CreateRequestIndexes_OneConfig_ReturnsRequestIndexWithHashKeySchemaElement()
        {
            var gsiConfig = new GlobalSecondaryIndexConfiguration
            {
                HashKeyMemberName = nameof(FakeEntity.Title),
                HashKeyMemberType = typeof(string),
                IndexName = "GSI_Entities_Title_LastModified",
                RangeKeyMemberName = nameof(FakeEntity.LastModified),
                RangeKeyMemberType = typeof(DateTime),
                ReadCapacityUnits = 3,
                WriteCapacityUnits = 5
            };
            var gsiConfigs = new List<GlobalSecondaryIndexConfiguration> { gsiConfig };
            var requestIndexes = this.factory.CreateRequestIndexes(gsiConfigs);
            var resultIndex = requestIndexes.Single();

            Assert.Contains(resultIndex.KeySchema, ks => 
                ks.AttributeName == gsiConfig.HashKeyMemberName &&
                ks.KeyType == KeyType.HASH);
        }

        [Fact]
        public void CreateRequestIndexes_OneConfig_ReturnsRequestIndexWithRangeKeySchemaElement()
        {
            var gsiConfig = new GlobalSecondaryIndexConfiguration
            {
                HashKeyMemberName = nameof(FakeEntity.Title),
                HashKeyMemberType = typeof(string),
                IndexName = "GSI_Entities_Title_LastModified",
                RangeKeyMemberName = nameof(FakeEntity.LastModified),
                RangeKeyMemberType = typeof(DateTime),
                ReadCapacityUnits = 3,
                WriteCapacityUnits = 5
            };
            var gsiConfigs = new List<GlobalSecondaryIndexConfiguration> { gsiConfig };
            var requestIndexes = this.factory.CreateRequestIndexes(gsiConfigs);
            var resultIndex = requestIndexes.Single();

            Assert.Contains(resultIndex.KeySchema, ks =>
                ks.AttributeName == gsiConfig.RangeKeyMemberName &&
                ks.KeyType == KeyType.RANGE);
        }

        [Theory]
        [InlineData(3)]
        [InlineData(9)]
        [InlineData(27)]
        [InlineData(81)]
        public void CreateRequestIndexes_ManyConfigs_ReturnsAsManyRequestIndexesAsConfigurationsPassed(int expected)
        {
            var gsiConfigs = Enumerable.Range(1, expected)
                .Select(i => new GlobalSecondaryIndexConfiguration
                {
                    HashKeyMemberName = $"Hash-{i}",
                    HashKeyMemberType = typeof(string),
                    IndexName = $"GSI_{i}",
                    RangeKeyMemberName = $"Range-{i}",
                    RangeKeyMemberType = typeof(int),
                    ReadCapacityUnits = 3,
                    WriteCapacityUnits = 5
                });
            var requestIndexes = this.factory.CreateRequestIndexes(gsiConfigs);

            Assert.Equal(expected, requestIndexes.Count());
        }
    }
}
