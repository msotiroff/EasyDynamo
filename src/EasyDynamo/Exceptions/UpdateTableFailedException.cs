using System;
using System.Runtime.Serialization;

namespace EasyDynamo.Exceptions
{
    public class UpdateTableFailedException : RequestFailedException
    {
        public UpdateTableFailedException()
        {
        }

        public UpdateTableFailedException(string message) : base(message)
        {
        }

        public UpdateTableFailedException(string message, Exception innerException) 
            : base(message, innerException)
        {
        }

        protected UpdateTableFailedException(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {
        }
    }
}
