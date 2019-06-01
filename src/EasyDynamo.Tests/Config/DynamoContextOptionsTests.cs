using Amazon;
using EasyDynamo.Exceptions;
using EasyDynamo.Tests.Fakes;
using System;
using Xunit;

namespace EasyDynamo.Tests.Config
{
    public class DynamoContextOptionsTests
    {
        private readonly DynamoContextOptionsFake options;

        public DynamoContextOptionsTests()
        {
            this.options = new DynamoContextOptionsFake
            {
                ServiceUrl = "http://localhost:8000",
                AccessKeyId = Guid.NewGuid().ToString(),
                SecretAccessKey = Guid.NewGuid().ToString(),
                Profile = "SomeProfileName",
                RegionEndpoint = RegionEndpoint.APNortheast1,
                LocalMode = false
            };
        }

        [Fact]
        public void ValidateLocalMode_NotLocalMode_DoesNotThrowException()
        {
            var thrown = false;
            this.options.LocalMode = false;

            try
            {
                this.options.ValidateLocalModeFromBase();
            }
            catch
            {
                thrown = true;
            }

            Assert.False(thrown);
        }

        [Fact]
        public void ValidateLocalMode_AllSet_DoesNotThrowException()
        {
            var thrown = false;
            this.options.LocalMode = true;

            try
            {
                this.options.ValidateLocalModeFromBase();
            }
            catch
            {
                thrown = true;
            }

            Assert.False(thrown);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("     ")]
        public void ValidateLocalMode_ServiceUrlIsEmpty_ThrowsException(string empty)
        {
            this.options.LocalMode = true;
            this.options.ServiceUrl = empty;

            Assert.Throws<DynamoContextConfigurationException>(
                () => this.options.ValidateLocalModeFromBase());
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("     ")]
        public void ValidateLocalMode_AccessKeyIsEmpty_ThrowsException(string empty)
        {
            this.options.LocalMode = true;
            this.options.AccessKeyId = empty;

            Assert.Throws<DynamoContextConfigurationException>(
                () => this.options.ValidateLocalModeFromBase());
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("     ")]
        public void ValidateLocalMode_SecretKeyIsEmpty_ThrowsException(string empty)
        {
            this.options.LocalMode = true;
            this.options.SecretAccessKey = empty;

            Assert.Throws<DynamoContextConfigurationException>(
                () => this.options.ValidateLocalModeFromBase());
        }

        [Fact]
        public void ValidateCloudMode_IsLocalMode_DoesNotThrowException()
        {
            var thrown = false;
            this.options.LocalMode = true;

            try
            {
                this.options.ValidateCloudModeFromBase();
            }
            catch
            {
                thrown = true;
            }

            Assert.False(thrown);
        }

        [Fact]
        public void ValidateCloudMode_AllSet_DoesNotThrowException()
        {
            var thrown = false;
            this.options.LocalMode = false;

            try
            {
                this.options.ValidateCloudModeFromBase();
            }
            catch
            {
                thrown = true;
            }

            Assert.False(thrown);
        }

        [Fact]
        public void ValidateCloudMode_RegionEndpointIsNull_ThrowsException()
        {
            this.options.LocalMode = false;
            this.options.RegionEndpoint = null;

            Assert.Throws<DynamoContextConfigurationException>(
                () => this.options.ValidateCloudModeFromBase());
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("     ")]
        public void ValidateLocalMode_ProfileIsEmpty_ThrowsException(string empty)
        {
            this.options.LocalMode = false;
            this.options.Profile = empty;

            Assert.Throws<DynamoContextConfigurationException>(
                () => this.options.ValidateCloudModeFromBase());
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("     ")]
        public void UseTableNAme_TableNameIsEmpty_ThrowsException(string empty)
        {
            Assert.Throws<ArgumentNullException>(
                () => this.options.UseTableName<FakeEntity>(empty));
        }

        [Fact]
        public void UseTableNAme_ValidTableName_AddsToDictionary()
        {
            const string TableName = "Articles";
            this.options.UseTableName<FakeEntity>(TableName);

            Assert.Single(this.options.TableNameByEntityTypesFromBase);
            Assert.True(this.options.TableNameByEntityTypesFromBase.ContainsKey(typeof(FakeEntity)));
            Assert.Equal(TableName, this.options.TableNameByEntityTypesFromBase[typeof(FakeEntity)]);
        }
    }
}
