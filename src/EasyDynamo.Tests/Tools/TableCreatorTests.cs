using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using EasyDynamo.Abstractions;
using EasyDynamo.Builders;
using EasyDynamo.Config;
using EasyDynamo.Exceptions;
using EasyDynamo.Tests.Fakes;
using EasyDynamo.Tools;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace EasyDynamo.Tests.Tools
{
    public class TableCreatorTests
    {
        private const string TableName = "Articles";
        private readonly Mock<IAmazonDynamoDB> clientMock;
        private readonly Mock<IIndexFactory> indexFactoryMock;
        private readonly Mock<IAttributeDefinitionFactory> attributeDefinitionFactoryMock;
        private readonly Mock<IIndexConfigurationFactory> indexConfigurationFactoryMock;
        private readonly Mock<IEntityConfigurationProvider> entityConfigurationProviderMock;
        private readonly Mock<IDynamoContextOptions> dynamoContextOptionsMock;
        private readonly TableCreator creator;
        private readonly ModelBuilder modelBuilder;

        public TableCreatorTests()
        {
            this.clientMock = new Mock<IAmazonDynamoDB>();
            this.indexFactoryMock = new Mock<IIndexFactory>();
            this.attributeDefinitionFactoryMock = new Mock<IAttributeDefinitionFactory>();
            this.indexConfigurationFactoryMock = new Mock<IIndexConfigurationFactory>();
            this.entityConfigurationProviderMock = new Mock<IEntityConfigurationProvider>();
            this.dynamoContextOptionsMock = new Mock<IDynamoContextOptions>();
            this.creator = new TableCreator(
                this.clientMock.Object,
                this.indexFactoryMock.Object,
                this.attributeDefinitionFactoryMock.Object,
                this.indexConfigurationFactoryMock.Object,
                this.entityConfigurationProviderMock.Object);
            this.modelBuilder = new ModelBuilderFake(this.dynamoContextOptionsMock.Object);
        }

        [Fact]
        public async Task CreateTableAsync_EntityTypeIsNull_ThrowsException()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(
                    () => this.creator.CreateTableAsync(
                        typeof(FakeDynamoContext), null, TableName));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("    ")]
        public async Task CreateTableAsync_TableNameIsEmpty_ThrowsException(string empty)
        {
            await Assert.ThrowsAsync<ArgumentNullException>(
                    () => this.creator.CreateTableAsync(
                        typeof(FakeDynamoContext), typeof(FakeEntity), empty));
        }

        [Fact]
        public async Task CreateTableAsync_ResponseSuccess_ReturnsCorrectTableName()
        {
            await TestRetrier.RetryAsync(async () =>
            {
                this.modelBuilder.Entity<FakeDynamoContext, FakeEntity>(entity =>
                {
                    entity.HasPrimaryKey(e => e.Id);
                });

                this.clientMock
                    .Setup(cli => cli.CreateTableAsync(
                        It.IsAny<CreateTableRequest>(),
                        It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new CreateTableResponse
                    {
                        HttpStatusCode = System.Net.HttpStatusCode.OK
                    });

                var result = await this.creator.CreateTableAsync(
                    typeof(FakeDynamoContext), typeof(FakeEntity), TableName);

                Assert.Equal(TableName, result);
            });
        }

        [Fact]
        public async Task CreateTableAsync_NoConfiguration_ThrowsException()
        {
            await TestRetrier.RetryAsync(async () =>
            {
                await Assert.ThrowsAsync<DynamoContextConfigurationException>(
                    () => this.creator.CreateTableAsync(
                        typeof(FakeDynamoContext), typeof(FakeEntity), TableName));
            });
        }

        [Fact]
        public async Task CreateTableAsync_NoConfiguration_ThrowsExceptionWithCorrectMessage()
        {
            await TestRetrier.RetryAsync(async () => 
            {
                var expected = string.Format(
                ExceptionMessage.EntityConfigurationNotFound,
                typeof(FakeEntity).FullName);
                var actual = default(string);

                try
                {
                    await this.creator.CreateTableAsync(
                        typeof(FakeDynamoContext), typeof(FakeEntity), TableName);
                }
                catch (Exception ex)
                {
                    actual = ex.Message;
                }

                Assert.Equal(expected, actual);
            });
        }

        [Fact]
        public async Task CreateTableAsync_ValidInput_CallClientWithCorrectTableName()
        {
            await TestRetrier.RetryAsync(async () =>
            {
                this.modelBuilder.Entity<FakeDynamoContext, FakeEntity>(entity =>
                {
                    entity.HasPrimaryKey(e => e.Id);
                });

                this.clientMock
                    .Setup(cli => cli.CreateTableAsync(
                        It.IsAny<CreateTableRequest>(),
                        It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new CreateTableResponse
                    {
                        HttpStatusCode = System.Net.HttpStatusCode.OK
                    });

                await this.creator.CreateTableAsync(
                    typeof(FakeDynamoContext), typeof(FakeEntity), TableName);

                this.clientMock
                    .Verify(cli => cli.CreateTableAsync(
                        It.Is<CreateTableRequest>(r => r.TableName == TableName),
                        It.IsAny<CancellationToken>()));
            });
        }

        [Fact]
        public async Task CreateTableAsync_ValidInput_CallClientWithCorrectAttributeDefinitions()
        {
            await TestRetrier.RetryAsync(async () =>
            {
                var definitions = new List<AttributeDefinition>();

                this.modelBuilder.Entity<FakeDynamoContext, FakeEntity>(entity =>
                {
                    entity.HasPrimaryKey(e => e.Id);
                });

                this.clientMock
                    .Setup(cli => cli.CreateTableAsync(
                        It.IsAny<CreateTableRequest>(),
                        It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new CreateTableResponse
                    {
                        HttpStatusCode = System.Net.HttpStatusCode.OK
                    });

                this.attributeDefinitionFactoryMock
                    .Setup(f => f.CreateAttributeDefinitions(
                        It.IsAny<string>(),
                        It.IsAny<ScalarAttributeType>(),
                        It.IsAny<IEnumerable<GlobalSecondaryIndexConfiguration>>()))
                    .Returns(definitions);

                await this.creator.CreateTableAsync(
                    typeof(FakeDynamoContext), typeof(FakeEntity), TableName);

                this.clientMock
                    .Verify(cli => cli.CreateTableAsync(
                        It.Is<CreateTableRequest>(r => r.AttributeDefinitions == definitions),
                        It.IsAny<CancellationToken>()));
            });
        }

        [Fact]
        public async Task CreateTableAsync_ValidInput_CallClientWithCorrectKeySchema()
        {
            await TestRetrier.RetryAsync(async () =>
            {
                this.modelBuilder.Entity<FakeDynamoContext, FakeEntity>(entity =>
                {
                    entity.HasPrimaryKey(e => e.Id);
                });

                this.clientMock
                    .Setup(cli => cli.CreateTableAsync(
                        It.IsAny<CreateTableRequest>(),
                        It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new CreateTableResponse
                    {
                        HttpStatusCode = System.Net.HttpStatusCode.OK
                    });

                await this.creator.CreateTableAsync(
                    typeof(FakeDynamoContext), typeof(FakeEntity), TableName);

                this.clientMock
                    .Verify(cli => cli.CreateTableAsync(
                        It.Is<CreateTableRequest>(r => 
                            r.KeySchema.Any(ks => ks.AttributeName == nameof(FakeEntity.Id) && 
                            ks.KeyType == KeyType.HASH)),
                        It.IsAny<CancellationToken>()));
            });
        }

        [Fact]
        public async Task CreateTableAsync_NoReadCapacitySpecified_CallClientWithDefaultReadCapacity()
        {
            await TestRetrier.RetryAsync(async () =>
            {
                this.modelBuilder.Entity<FakeDynamoContext, FakeEntity>(entity =>
                {
                    entity.HasPrimaryKey(e => e.Id);
                });

                this.clientMock
                    .Setup(cli => cli.CreateTableAsync(
                        It.IsAny<CreateTableRequest>(),
                        It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new CreateTableResponse
                    {
                        HttpStatusCode = System.Net.HttpStatusCode.OK
                    });

                await this.creator.CreateTableAsync(
                    typeof(FakeDynamoContext), typeof(FakeEntity), TableName);

                this.clientMock
                    .Verify(cli => cli.CreateTableAsync(
                        It.Is<CreateTableRequest>(r =>
                            r.ProvisionedThroughput.ReadCapacityUnits == 
                            Constants.DefaultReadCapacityUnits),
                        It.IsAny<CancellationToken>()));
            });
        }

        [Fact]
        public async Task CreateTableAsync_ReadCapacitySpecified_CallClientWithCorrectReadCapacity()
        {
            await TestRetrier.RetryAsync(async () =>
            {
                const int ReadCapacity = 22;
                this.modelBuilder.Entity<FakeDynamoContext, FakeEntity>(entity =>
                {
                    entity.HasPrimaryKey(e => e.Id);
                    entity.HasReadCapacityUnits(ReadCapacity);
                });

                this.clientMock
                    .Setup(cli => cli.CreateTableAsync(
                        It.IsAny<CreateTableRequest>(),
                        It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new CreateTableResponse
                    {
                        HttpStatusCode = System.Net.HttpStatusCode.OK
                    });

                await this.creator.CreateTableAsync(
                    typeof(FakeDynamoContext), typeof(FakeEntity), TableName);

                this.clientMock
                    .Verify(cli => cli.CreateTableAsync(
                        It.Is<CreateTableRequest>(r =>
                            r.ProvisionedThroughput.ReadCapacityUnits ==
                            ReadCapacity),
                        It.IsAny<CancellationToken>()));
            });
        }

        [Fact]
        public async Task CreateTableAsync_NoWriteCapacitySpecified_CallClientWithDefaultWriteCapacity()
        {
            await TestRetrier.RetryAsync(async () =>
            {
                this.modelBuilder.Entity<FakeDynamoContext, FakeEntity>(entity =>
                {
                    entity.HasPrimaryKey(e => e.Id);
                });

                this.clientMock
                    .Setup(cli => cli.CreateTableAsync(
                        It.IsAny<CreateTableRequest>(),
                        It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new CreateTableResponse
                    {
                        HttpStatusCode = System.Net.HttpStatusCode.OK
                    });

                await this.creator.CreateTableAsync(
                    typeof(FakeDynamoContext), typeof(FakeEntity), TableName);

                this.clientMock
                    .Verify(cli => cli.CreateTableAsync(
                        It.Is<CreateTableRequest>(r =>
                            r.ProvisionedThroughput.WriteCapacityUnits ==
                            Constants.DefaultWriteCapacityUnits),
                        It.IsAny<CancellationToken>()));
            });
        }

        [Fact]
        public async Task CreateTableAsync_WriteCapacitySpecified_CallClientWithCorrectWriteCapacity()
        {
            await TestRetrier.RetryAsync(async () =>
            {
                const int WriteCapacity = 22;
                this.modelBuilder.Entity<FakeDynamoContext, FakeEntity>(entity =>
                {
                    entity.HasPrimaryKey(e => e.Id);
                    entity.HasWriteCapacityUnits(WriteCapacity);
                });

                this.clientMock
                    .Setup(cli => cli.CreateTableAsync(
                        It.IsAny<CreateTableRequest>(),
                        It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new CreateTableResponse
                    {
                        HttpStatusCode = System.Net.HttpStatusCode.OK
                    });

                await this.creator.CreateTableAsync(
                    typeof(FakeDynamoContext), typeof(FakeEntity), TableName);

                this.clientMock
                    .Verify(cli => cli.CreateTableAsync(
                        It.Is<CreateTableRequest>(r =>
                            r.ProvisionedThroughput.WriteCapacityUnits ==
                            WriteCapacity),
                        It.IsAny<CancellationToken>()));
            });
        }

        [Fact]
        public async Task CreateTableAsync_ResponseFail_ThrowsException()
        {
            await TestRetrier.RetryAsync(async () =>
            {
                this.modelBuilder.Entity<FakeDynamoContext, FakeEntity>(entity =>
                {
                    entity.HasPrimaryKey(e => e.Id);
                });

                this.clientMock
                    .Setup(cli => cli.CreateTableAsync(
                        It.IsAny<CreateTableRequest>(),
                        It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new CreateTableResponse
                    {
                        HttpStatusCode = System.Net.HttpStatusCode.BadRequest
                    });

                await Assert.ThrowsAsync<DynamoContextConfigurationException>(
                    () => this.creator.CreateTableAsync(
                        typeof(FakeDynamoContext), typeof(FakeEntity), TableName));
            });
        }

        [Fact]
        public async Task CreateTableAsync_ClientThrows_ThrowsException()
        {
            await TestRetrier.RetryAsync(async () =>
            {
                this.modelBuilder.Entity<FakeDynamoContext, FakeEntity>(entity =>
                {
                    entity.HasPrimaryKey(e => e.Id);
                });

                this.clientMock
                    .Setup(cli => cli.CreateTableAsync(
                        It.IsAny<CreateTableRequest>(),
                        It.IsAny<CancellationToken>()))
                    .ThrowsAsync(new InvalidOperationException());

                await Assert.ThrowsAsync<DynamoContextConfigurationException>(
                    () => this.creator.CreateTableAsync(
                        typeof(FakeDynamoContext), typeof(FakeEntity), TableName));
            });
        }
    }
}
