using EasyDynamo.Tests.Fakes;
using EasyDynamo.Tools;
using System;
using System.Linq;
using Xunit;

namespace EasyDynamo.Tests.Factories
{
    public class IndexConfigurationFactoryTests
    {
        private readonly IndexConfigurationFactory factory;

        public IndexConfigurationFactoryTests()
        {
            this.factory = new IndexConfigurationFactory();
        }

        [Fact]
        public void CreateIndexConfigByAttributes_EntityTypeIsNull_ThrowsException()
        {
            Assert.Throws<ArgumentNullException>(
                () => this.factory.CreateIndexConfigByAttributes(null));
        }

        [Fact]
        public void CreateIndexConfigByAttributes_ValidEntityType_ReturnsConfigurationWithCorrectIndexName()
        {
            var configurations = this.factory.CreateIndexConfigByAttributes(typeof(FakeEntity));
            var config = configurations.Single();

            Assert.Equal(FakeEntity.IndexName, config.IndexName);
        }

        [Fact]
        public void CreateIndexConfigByAttributes_ValidEntityType_ReturnsConfigurationWithCorrectHashKeyName()
        {
            var configurations = this.factory.CreateIndexConfigByAttributes(typeof(FakeEntity));
            var config = configurations.Single();

            Assert.Equal(nameof(FakeEntity.Title), config.HashKeyMemberName);
        }

        [Fact]
        public void CreateIndexConfigByAttributes_ValidEntityType_ReturnsConfigurationWithCorrectHashKeyType()
        {
            var configurations = this.factory.CreateIndexConfigByAttributes(typeof(FakeEntity));
            var config = configurations.Single();

            Assert.Equal(typeof(string), config.HashKeyMemberType);
        }

        [Fact]
        public void CreateIndexConfigByAttributes_ValidEntityType_ReturnsConfigurationWithCorrectRangeKeyName()
        {
            var configurations = this.factory.CreateIndexConfigByAttributes(typeof(FakeEntity));
            var config = configurations.Single();

            Assert.Equal(nameof(FakeEntity.LastModified), config.RangeKeyMemberName);
        }

        [Fact]
        public void CreateIndexConfigByAttributes_ValidEntityType_ReturnsConfigurationWithCorrectRangeKeyType()
        {
            var configurations = this.factory.CreateIndexConfigByAttributes(typeof(FakeEntity));
            var config = configurations.Single();

            Assert.Equal(typeof(DateTime), config.RangeKeyMemberType);
        }
    }
}
