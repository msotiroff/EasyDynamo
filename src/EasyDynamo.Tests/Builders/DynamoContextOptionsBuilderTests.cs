using Amazon;
using EasyDynamo.Abstractions;
using EasyDynamo.Tests.Fakes;
using Moq;
using System;
using Xunit;

namespace EasyDynamo.Tests.Builders
{
    public class DynamoContextOptionsBuilderTests
    {
        private readonly Mock<IDynamoContextOptions> contextOptionsMock;
        private readonly DynamoContextOptionsBuilderFake builder;

        public DynamoContextOptionsBuilderTests()
        {
            this.contextOptionsMock = new Mock<IDynamoContextOptions>();
            this.builder = new DynamoContextOptionsBuilderFake(contextOptionsMock.Object);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("    ")]
        public void UseTableName_EmptyTableNamePassed_ThrowsException(string emptyTableName)
        {
            Assert.Throws<ArgumentNullException>(
                () => this.builder.UseTableName<FakeEntity>(emptyTableName));
        }

        [Fact]
        public void UseTableName_ValidTableNamePassed_CallsContextOptionsUseTableNameWithCorrectArgument()
        {
            const string TableName = "FakeEntity_dev";
            this.builder.UseTableName<FakeEntity>(TableName);

            this.contextOptionsMock
                .Verify(co => co.UseTableName<FakeEntity>(TableName));
        }

        [Fact]
        public void UseTableName_ReturnsSameBuilderInstance()
        {
            var returnedBuilder = this.builder.UseTableName<FakeEntity>("some-table");

            Assert.Equal(this.builder, returnedBuilder);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("    ")]
        public void UseAccessKeyId_EmptyKeyIdPassed_ThrowsException(string emptyKey)
        {
            Assert.Throws<ArgumentNullException>(
                () => this.builder.UseSecretAccessKey(emptyKey));
        }

        [Fact]
        public void UseAccessKeyId_ValidKeyPassed_SetContextOptionsAccessKeyIdWithCorrectValue()
        {
            const string Key = "some-key";

            this.builder.UseAccessKeyId(Key);

            this.contextOptionsMock
                .VerifySet(opt => opt.AccessKeyId = Key);
        }

        [Fact]
        public void UseAccessKeyId_ReturnsSameBuilderInstance()
        {
            var returnedBuilder = this.builder.UseAccessKeyId("some-key");

            Assert.Equal(this.builder, returnedBuilder);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("    ")]
        public void UseSecretAccessKey_EmptySecretPassed_ThrowsException(string emptySecret)
        {
            Assert.Throws<ArgumentNullException>(
                () => this.builder.UseSecretAccessKey(emptySecret));
        }

        [Fact]
        public void UseSecretAccessKey_ValidSecretPassed_SetContextOptionsSecretAccessKeyWithCorrectValue()
        {
            const string Secret = "some-secret";

            this.builder.UseSecretAccessKey(Secret);

            this.contextOptionsMock
                .VerifySet(opt => opt.SecretAccessKey = Secret);
        }

        [Fact]
        public void UseSecretAccessKey_ReturnsSameBuilderInstance()
        {
            var returnedBuilder = this.builder.UseSecretAccessKey("some-secret");

            Assert.Equal(this.builder, returnedBuilder);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("    ")]
        public void UseLocalMode_EmptyUrlPassed_ThrowsException(string emptyServiceUrl)
        {
            Assert.Throws<ArgumentNullException>(
                () => this.builder.UseLocalMode(emptyServiceUrl));
        }

        [Fact]
        public void UseLocalMode_ValidLocalModePassed_SetContextOptionsServiceUrlWithCorrectValue()
        {
            const string Url = "http://localhost:8000";

            this.builder.UseLocalMode(Url);

            this.contextOptionsMock
                .VerifySet(opt => opt.ServiceUrl = Url);
        }

        [Fact]
        public void UseLocalMode_ReturnsSameBuilderInstance()
        {
            const string Url = "http://localhost:8000";
            var returnedBuilder = this.builder.UseLocalMode(Url);

            Assert.Equal(this.builder, returnedBuilder);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("    ")]
        public void UseServiceUrl_EmptyUrlPassed_ThrowsException(string emptyServiceUrl)
        {
            Assert.Throws<ArgumentNullException>(
                () => this.builder.UseServiceUrl(emptyServiceUrl));
        }

        [Fact]
        public void UseServiceUrl_ValidLocalModePassed_SetContextOptionsServiceUrlWithCorrectValue()
        {
            const string Url = "http://localhost:8000";

            this.builder.UseLocalMode(Url);

            this.contextOptionsMock
                .VerifySet(opt => opt.ServiceUrl = Url);
        }

        [Fact]
        public void UseServiceUrl_ReturnsSameBuilderInstance()
        {
            const string Url = "http://localhost:8000";
            var returnedBuilder = this.builder.UseServiceUrl(Url);

            Assert.Equal(this.builder, returnedBuilder);
        }

        [Fact]
        public void UseRegionEndpoint_NullRegionPassed_ThrowsException()
        {
            Assert.Throws<ArgumentNullException>(
                () => this.builder.UseRegionEndpoint(null));
        }

        [Fact]
        public void UseRegionEndpoint_ValidRegionPassed_SetContextOptionsRegionwithCorrectValue()
        {
            var region = RegionEndpoint.APNortheast1;

            this.builder.UseRegionEndpoint(region);

            this.contextOptionsMock
                .VerifySet(co => co.RegionEndpoint = region);
        }

        [Fact]
        public void UseRegionEndpoint_ReturnsSameBuilderInstance()
        {
            var returnedBuilder = this.builder.UseRegionEndpoint(RegionEndpoint.APNortheast1);

            Assert.Equal(this.builder, returnedBuilder);
        }
    }
}
