using System;

namespace EasyDynamo.Abstractions
{
    public interface IDependencyResolver
    {
        TDependency GetDependency<TDependency>();

        object GetDependency(Type dependencyType);

        object TryGetDependency(Type dependencyType);
    }
}
