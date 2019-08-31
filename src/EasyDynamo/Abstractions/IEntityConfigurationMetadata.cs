using System;

namespace EasyDynamo.Abstractions
{
    public interface IEntityConfigurationMetadata
    {
        Type EntityType { get; }

        Type ContextType { get; }
    }
}
