using System;
using System.Runtime.Serialization;

namespace EasyDynamo.Exceptions
{
    public class DynamoDbIndexMissingException : Exception
    {
        public DynamoDbIndexMissingException()
        {
        }

        public DynamoDbIndexMissingException(string message) 
            : base(message)
        {
        }

        public DynamoDbIndexMissingException(string message, Exception innerException) 
            : base(message, innerException)
        {
        }

        protected DynamoDbIndexMissingException(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {
        }
    }
}
