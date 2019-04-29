using System;
using System.Runtime.Serialization;

namespace EasyDynamo.Exceptions
{
    public class EntityAlreadyExistException : Exception
    {
        public EntityAlreadyExistException()
        {
        }

        public EntityAlreadyExistException(string message) 
            : base(message)
        {
        }

        public EntityAlreadyExistException(string message, Exception innerException) 
            : base(message, innerException)
        {
        }

        protected EntityAlreadyExistException(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {
        }
    }
}
