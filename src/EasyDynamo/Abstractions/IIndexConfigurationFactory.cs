using System;
using System.Collections.Generic;
using EasyDynamo.Config;

namespace EasyDynamo.Abstractions
{
    public interface IIndexConfigurationFactory
    {
        IEnumerable<GlobalSecondaryIndexConfiguration> CreateIndexConfigByAttributes(
            Type entityType);
    }
}