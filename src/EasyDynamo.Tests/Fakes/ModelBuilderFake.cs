using EasyDynamo.Abstractions;
using EasyDynamo.Builders;
using System;
using System.Collections.Generic;

namespace EasyDynamo.Tests.Fakes
{
    public class ModelBuilderFake : ModelBuilder
    {
        public IDictionary<Type, IEntityConfiguration> EntityConfigurationByEntityTypesFromBase
            => base.EntityConfigurationByEntityTypes;
    }
}
