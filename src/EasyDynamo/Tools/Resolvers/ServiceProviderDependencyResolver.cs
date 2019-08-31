using EasyDynamo.Abstractions;
using System;

namespace EasyDynamo.Tools.Resolvers
{
    public class ServiceProviderDependencyResolver : IDependencyResolver
    {
        private readonly IServiceProvider serviceProvider;

        public ServiceProviderDependencyResolver(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public TDependency GetDependency<TDependency>()
        {
            return (TDependency)this.GetDependency(typeof(TDependency));
        }

        public object GetDependency(Type dependencyType)
        {
            return this.serviceProvider.GetService(dependencyType);
        }
    }
}
