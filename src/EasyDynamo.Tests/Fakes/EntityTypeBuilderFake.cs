using EasyDynamo.Abstractions;
using EasyDynamo.Builders;
using EasyDynamo.Config;

namespace EasyDynamo.Tests.Fakes
{
    public class EntityTypeBuilderFake : EntityTypeBuilder<FakeEntity>
    {
        protected internal EntityTypeBuilderFake(
            EntityConfiguration<FakeEntity> entityConfig, 
            IDynamoContextOptions contextOptions) : base(entityConfig, contextOptions)
        {
        }
    }
}
