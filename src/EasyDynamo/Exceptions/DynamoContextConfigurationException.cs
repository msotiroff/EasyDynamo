using System;
using System.Runtime.Serialization;

namespace EasyDynamo.Exceptions
{
    public class DynamoContextConfigurationException : Exception
    {
        public DynamoContextConfigurationException()
        {
        }

        public DynamoContextConfigurationException(string message) 
            : base(message)
        {
        }

        public DynamoContextConfigurationException(string message, Exception innerException) 
            : base(message, innerException)
        {
        }

        protected DynamoContextConfigurationException(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {
        }
    }
}
