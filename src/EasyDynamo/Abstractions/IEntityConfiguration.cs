using EasyDynamo.Config;
using System;
using System.Collections.Generic;

namespace EasyDynamo.Abstractions
{
    public interface IEntityConfiguration
    {
        string HashKeyMemberName { get; set; }
        
        Type HashKeyMemberType { get; set; }
        
        ISet<string> IgnoredMembersNames { get; }

        ISet<GlobalSecondaryIndexConfiguration> Indexes { get; }

        string TableName { get; set; }

        long ReadCapacityUnits { get; set; }

        long WriteCapacityUnits { get; set; }

        bool ValidateOnSave { get; set; }
    }
}