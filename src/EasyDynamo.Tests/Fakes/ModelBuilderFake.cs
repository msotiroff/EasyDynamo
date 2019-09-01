using EasyDynamo.Abstractions;
using EasyDynamo.Builders;
using System;
using System.Collections.Generic;

namespace EasyDynamo.Tests.Fakes
{
    public class ModelBuilderFake : ModelBuilder
    {
        protected internal ModelBuilderFake(IDynamoContextOptions contextOptions) 
            : base(contextOptions)
        {
        }

        public IDictionary<Type, IEntityConfiguration> EntityConfigurationsFromBase 
            => base.EntityConfigurations;
    }
}
