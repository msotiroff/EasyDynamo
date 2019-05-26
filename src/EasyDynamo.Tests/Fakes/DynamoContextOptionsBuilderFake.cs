using EasyDynamo.Abstractions;
using EasyDynamo.Builders;

namespace EasyDynamo.Tests.Fakes
{
    public class DynamoContextOptionsBuilderFake : DynamoContextOptionsBuilder
    {
        protected internal DynamoContextOptionsBuilderFake(IDynamoContextOptions options) 
            : base(options)
        {
        }
    }
}
