using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using EasyDynamo.Abstractions;
using EasyDynamo.Exceptions;
using EasyDynamo.Extensions;
using System;
using System.Threading.Tasks;

namespace EasyDynamo.Tools
{
    public class TableDropper : ITableDropper
    {
        private readonly IAmazonDynamoDB client;

        public TableDropper(IAmazonDynamoDB client)
        {
            this.client = client;
        }

        public async Task DropTableAsync(string tableName)
        {
            var request = new DeleteTableRequest(tableName);

            try
            {
                var response = await this.client.DeleteTableAsync(request);

                if (!response.HttpStatusCode.IsSuccessful())
                {
                    throw new DeleteTableFailedException(
                        response.ResponseMetadata.Metadata.JoinByNewLine());
                }
            }
            catch (Exception ex)
            {
                throw new DeleteTableFailedException("Failed to delete a table.", ex);
            }
        }
    }
}
