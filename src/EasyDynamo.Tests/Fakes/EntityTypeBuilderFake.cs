using EasyDynamo.Abstractions;
using EasyDynamo.Builders;
using EasyDynamo.Config;

namespace EasyDynamo.Tests.Fakes
{
    public class EntityTypeBuilderFake : EntityTypeBuilder<FakeDynamoContext, FakeEntity>
    {
        protected internal EntityTypeBuilderFake(
            EntityConfiguration<FakeDynamoContext, FakeEntity> entityConfig, 
            IDynamoContextOptions contextOptions) : base(entityConfig, contextOptions)
        {
        }
    }
}
