using Newtonsoft.Json;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace EasyDynamo.Extensions
{
    public static class ObjectExtensions
    {
        public static T Clone<T>(this T source)
        {
            var destination = TryClone(source);

            return destination;
        }

        private static T TryClone<T>(T source)
        {
            try
            {
                using (var ms = new MemoryStream())
                {
                    var formatter = new BinaryFormatter();
                    formatter.Serialize(ms, source);
                    ms.Position = 0;

                    return (T)formatter.Deserialize(ms);
                }
            }
            catch
            {
                return TryCloneUsingJsonSerialization(source);
            }
        }

        private static T TryCloneUsingJsonSerialization<T>(T source)
        {
            try
            {
                var settings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                };
                var serialized = JsonConvert.SerializeObject(source, settings);
                var deserialized = JsonConvert.DeserializeObject<T>(serialized);

                return deserialized;
            }
            catch
            {
                return default(T);
            }
        }
    }
}
