using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;

namespace EasyDynamo.Extensions
{
    public static class DynamoDBContextExtensions
    {
        public static Table TryGetTargetTable<T>(
            this IDynamoDBContext dynamoDBContext, 
            DynamoDBOperationConfig operationConfig = null)
        {
            try
            {
                return dynamoDBContext.GetTargetTable<T>(operationConfig);
            }
            catch(System.Exception ex)
            {
                return default(Table);
            }
        }
    }
}
