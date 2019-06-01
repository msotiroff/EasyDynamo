using EasyDynamo.Config;

namespace EasyDynamo.Tests.Fakes
{
    public class PropertyConfigurationFake : PropertyConfiguration<FakeEntity>
    {
        protected internal PropertyConfigurationFake(string memberName) 
            : base(memberName)
        {
        }
    }
}
