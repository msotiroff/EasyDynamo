using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using EasyDynamo.Exceptions;
using EasyDynamo.Tools;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace EasyDynamo.Tests.Tools
{
    public class TableDropperTests
    {
        private readonly Mock<IAmazonDynamoDB> clientMock;
        private readonly TableDropper dropper;

        public TableDropperTests()
        {
            this.clientMock = new Mock<IAmazonDynamoDB>();
            this.dropper = new TableDropper(this.clientMock.Object);
        }

        [Fact]
        public async Task DropTableAsync_CallsClientWithCorrectTableName()
        {
            const string TableName = "Articles";

            this.clientMock
                .Setup(cli => cli.DeleteTableAsync(
                    It.Is<DeleteTableRequest>(dtr => dtr.TableName == TableName),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DeleteTableResponse
                {
                    HttpStatusCode = System.Net.HttpStatusCode.OK
                })
                .Verifiable();

            await this.dropper.DropTableAsync(TableName);

            this.clientMock.Verify();
        }

        [Fact]
        public async Task DropTableAsync_ResponseFail_ThrowsException()
        {
            const string TableName = "Articles";

            this.clientMock
                .Setup(cli => cli.DeleteTableAsync(
                    It.IsAny<DeleteTableRequest>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DeleteTableResponse
                {
                    HttpStatusCode = System.Net.HttpStatusCode.BadRequest
                });

            await Assert.ThrowsAsync<DeleteTableFailedException>(
                () => this.dropper.DropTableAsync(TableName));
        }

        [Fact]
        public async Task DropTableAsync_ClientThrows_ThrowsException()
        {
            const string TableName = "Articles";

            this.clientMock
                .Setup(cli => cli.DeleteTableAsync(
                    It.IsAny<DeleteTableRequest>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException());

            await Assert.ThrowsAsync<DeleteTableFailedException>(
                () => this.dropper.DropTableAsync(TableName));
        }
    }
}
