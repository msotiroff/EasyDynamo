using EasyDynamo.Core;
using System;

namespace EasyDynamo.Abstractions
{
    public interface IDynamoContextOptionsProvider
    {
        IDynamoContextOptions GetContextOptions<TContext>() where TContext : DynamoContext;

        IDynamoContextOptions GetContextOptions(Type contextType);

        IDynamoContextOptions TryGetContextOptions<TContext>() where TContext : DynamoContext;
    }
}
