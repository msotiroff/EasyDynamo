using EasyDynamo.Abstractions;
using System;

namespace EasyDynamo.Tests.Fakes
{
    public class EntityTypeConfugurationFake : IEntityTypeConfiguration<FakeEntity>
    {
        public bool ConfigureInvoked { get; private set; }

        public Type ConfigureInvokedWithBuilderType { get; private set; }

        public void Configure(IEntityTypeBuilder<FakeEntity> builder)
        {
            this.ConfigureInvoked = true;
            this.ConfigureInvokedWithBuilderType = builder.GetType();
        }
    }
}
