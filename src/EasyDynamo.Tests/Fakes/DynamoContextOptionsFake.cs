using EasyDynamo.Config;
using System;
using System.Collections.Generic;

namespace EasyDynamo.Tests.Fakes
{
    public class DynamoContextOptionsFake : DynamoContextOptions
    {
        public IDictionary<Type, string> TableNameByEntityTypesFromBase
            => base.TableNameByEntityTypes;

        public static DynamoContextOptions BaseInstance => Instance;

        protected internal void ValidateCloudModeFromBase()
        {
            base.ValidateCloudMode();
        }

        protected internal void ValidateLocalModeFromBase()
        {
            base.ValidateLocalMode();
        }
    }
}
