using System;
using System.Linq.Expressions;

namespace EasyDynamo.Abstractions
{
    public interface IDependencyResolver
    {
        TDependency GetDependency<TDependency>();

        object GetDependency(Type dependencyType);
    }
}
