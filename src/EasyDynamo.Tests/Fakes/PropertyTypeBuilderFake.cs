using EasyDynamo.Builders;
using EasyDynamo.Config;

namespace EasyDynamo.Tests.Fakes
{
    public class PropertyTypeBuilderFake : PropertyTypeBuilder<FakeEntity>
    {
        protected internal PropertyTypeBuilderFake(
            PropertyConfiguration<FakeEntity> configuration) 
            : base(configuration)
        {
        }
    }
}
