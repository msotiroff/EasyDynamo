using System;
using System.Runtime.Serialization;

namespace EasyDynamo.Exceptions
{
    public class CreateTableFailedException : RequestFailedException
    {
        public CreateTableFailedException()
        {
        }

        public CreateTableFailedException(string message) : base(message)
        {
        }

        public CreateTableFailedException(string message, Exception innerException) 
            : base(message, innerException)
        {
        }

        protected CreateTableFailedException(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {
        }
    }
}
