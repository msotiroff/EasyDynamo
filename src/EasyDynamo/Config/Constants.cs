using System;
using System.Collections.Generic;

namespace EasyDynamo.Config
{
    public static class Constants
    {
        public const long DefaultReadCapacityUnits = 1;
        public const long DefaultWriteCapacityUnits = 1;

        public static readonly IDictionary<Type, string> AttributeTypesMap = 
            new Dictionary<Type, string>
            {
                [typeof(string)] = "S",
                [typeof(int)] = "N",
                [typeof(byte)] = "B",
                [typeof(DateTime)] = "S",
                [typeof(Enum)] = "N",
                [typeof(byte[])] = "BS",
                [typeof(string[])] = "SS",
            };
    }
}
