using EasyDynamo.Builders;
using EasyDynamo.Core;
using Microsoft.Extensions.Configuration;
using System;

namespace EasyDynamo.Tests.Fakes
{
    public class FakeDynamoContext : DynamoContext
    {
        public FakeDynamoContext(IServiceProvider serviceProvider) 
            : base(serviceProvider)
        {
        }

        public static bool OnConfiguringInvoked { get; set; }

        public static bool OnModelCreatingInvoked { get; set; }

        protected override void OnConfiguring(
            DynamoContextOptionsBuilder builder, IConfiguration configuration)
        {
            OnConfiguringInvoked = true;

            base.OnConfiguring(builder, configuration);
        }

        protected override void OnModelCreating<FakeDynamoContext>(
            ModelBuilder<FakeDynamoContext> builder, IConfiguration configuration)
        {
            OnModelCreatingInvoked = true;

            base.OnModelCreating(builder, configuration);
        }
    }
}
