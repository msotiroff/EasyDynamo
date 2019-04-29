using System.Reflection;

namespace EasyDynamo.Extensions
{
    public static class PropertyInfoExtensions
    {
        public static void SetDefaultValue(this PropertyInfo propertyInfo, object instance)
        {
            propertyInfo.SetValue(instance, propertyInfo.PropertyType.GetDefaultValue());
        }
    }
}
