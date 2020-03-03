using EasyDynamo.Config;
using System;
using System.Collections.Generic;

namespace EasyDynamo.Abstractions
{
    public interface IEntityConfiguration : IEntityConfigurationMetadata
    {
        string HashKeyMemberName { get; set; }

        Type HashKeyMemberType { get; set; }

        string SortKeyMemberName { get; }
        
        Type SortKeyMemberType { get; set; }
        
        ISet<string> IgnoredMembersNames { get; }

        ISet<GlobalSecondaryIndexConfiguration> Indexes { get; }

        string TableName { get; set; }

        long ReadCapacityUnits { get; set; }

        long WriteCapacityUnits { get; set; }

        bool ValidateOnSave { get; set; }

        string TTLMemberName { get; set; }
        
        bool HasDynamicBilling { get; set; }
    }
}