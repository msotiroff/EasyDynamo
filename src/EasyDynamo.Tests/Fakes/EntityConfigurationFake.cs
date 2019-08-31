using EasyDynamo.Config;

namespace EasyDynamo.Tests.Fakes
{
    public class EntityConfigurationFake : EntityConfiguration<FakeDynamoContext, FakeEntity>
    {
        public EntityConfigurationFake() 
            : base()
        {
        }
    }
}
