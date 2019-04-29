using System;
using System.Runtime.Serialization;

namespace EasyDynamo.Exceptions
{
    public class EntityValidationFailedException : Exception
    {
        public EntityValidationFailedException()
        {
        }

        public EntityValidationFailedException(string message) : base(message)
        {
        }

        public EntityValidationFailedException(string message, Exception innerException) 
            : base(message, innerException)
        {
        }

        protected EntityValidationFailedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
