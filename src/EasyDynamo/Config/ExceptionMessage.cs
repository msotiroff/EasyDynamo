namespace EasyDynamo.Config
{
    public class ExceptionMessage
    {
        public const string EntityAlreadyExist = "The entity already exist.";
        public const string EntityDoesNotExist = "The entity does not exist.";
        public const string HashKeyConfigurationNotFound = 
            "HashKey configuration not found. " +
            "Specify a hash key using DynamoDBHashKey attribute or override " +
            "the OnModelCreating of your DynamoContext.";
        public const string EntityIndexNotFound = "Could not find an index for member: {0}.{1}.";
        public const string EntityConfigurationNotFound = 
            "Configuration for {0} not found. " +
            "Use DynamoDBAttributes in your entity class " +
            "or override the OnModelCreating method in your database context class.";
        public const string PositiveIntegerNeeded ="{0} should be a positive integer.";
    }
}
