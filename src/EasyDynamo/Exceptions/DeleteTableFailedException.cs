using System;
using System.Runtime.Serialization;

namespace EasyDynamo.Exceptions
{
    public class DeleteTableFailedException : RequestFailedException
    {
        public DeleteTableFailedException()
        {
        }

        public DeleteTableFailedException(string message) : base(message)
        {
        }

        public DeleteTableFailedException(string message, Exception innerException) 
            : base(message, innerException)
        {
        }

        protected DeleteTableFailedException(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {
        }
    }
}
