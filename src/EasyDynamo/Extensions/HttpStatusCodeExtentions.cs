using System.Net;

namespace EasyDynamo.Extensions
{
    public static class HttpStatusCodeExtentions
    {
        public static bool IsSuccessful(this HttpStatusCode statusCode)
        {
            return (int)statusCode >= 200 && (int)statusCode < 300;
        }
    }
}
