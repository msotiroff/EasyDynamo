using System.Runtime.Serialization;

namespace EasyDynamo.Tools
{
    internal static class Instantiator
    {
        internal static T GetConstructorlessInstance<T>() where T : class
        {
            return (T)FormatterServices.GetUninitializedObject(typeof(T));
        }
    }
}
