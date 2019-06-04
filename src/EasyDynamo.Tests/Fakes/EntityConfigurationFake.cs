using EasyDynamo.Config;

namespace EasyDynamo.Tests.Fakes
{
    public class EntityConfigurationFake : EntityConfiguration<FakeEntity>
    {
        public EntityConfigurationFake() 
            : base()
        {
        }

        public EntityConfiguration<FakeEntity> BaseInstance = Instance;
    }
}
