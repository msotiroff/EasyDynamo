using System;

namespace EasyDynamo.Attributes
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class IgnoreAutoResolvingAttribute : Attribute
    {
    }
}
