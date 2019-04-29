using System;
using System.Runtime.Serialization;

namespace EasyDynamo.Exceptions
{
    public class RequestFailedException : Exception
    {
        public RequestFailedException()
        {
        }

        public RequestFailedException(string message) 
            : base(message)
        {
        }

        public RequestFailedException(string message, Exception innerException) 
            : base(message, innerException)
        {
        }

        protected RequestFailedException(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {
        }
    }
}
