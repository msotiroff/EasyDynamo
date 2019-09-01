using EasyDynamo.Abstractions;
using EasyDynamo.Core;
using EasyDynamo.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EasyDynamo.Tools.Providers
{
    public class DynamoContextOptionsProvider : IDynamoContextOptionsProvider
    {
        private readonly IEnumerable<IDynamoContextOptions> allOptions;

        public DynamoContextOptionsProvider(IEnumerable<IDynamoContextOptions> options = null)
        {
            this.allOptions = options ?? new List<IDynamoContextOptions>();
        }

        public IDynamoContextOptions GetContextOptions<TContext>() 
            where TContext : DynamoContext
        {
            return this.GetContextOptions(typeof(TContext));
        }

        public IDynamoContextOptions GetContextOptions(Type contextType)
        {
            if (!typeof(DynamoContext).IsAssignableFrom(contextType))
            {
                throw new DynamoContextConfigurationException(
                    $"{contextType.Name} does not inherit from {nameof(DynamoContext)}.");
            }

            var options = this.allOptions.FirstOrDefault(o => o.ContextType == contextType);

            return options ??
                throw new DynamoContextConfigurationException(
                    $"Could not find any options for {contextType.Name}.");
        }

        public IDynamoContextOptions TryGetContextOptions<TContext>() 
            where TContext : DynamoContext
        {
            try
            {
                return this.GetContextOptions<TContext>();
            }
            catch
            {
                return default;
            }
        }
    }
}
