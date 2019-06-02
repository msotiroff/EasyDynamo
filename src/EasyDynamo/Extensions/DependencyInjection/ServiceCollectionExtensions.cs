using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Extensions.NETCore.Setup;
using EasyDynamo.Abstractions;
using EasyDynamo.Builders;
using EasyDynamo.Config;
using EasyDynamo.Core;
using EasyDynamo.Exceptions;
using EasyDynamo.Tools;
using EasyDynamo.Tools.Validators;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Reflection;

namespace EasyDynamo.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        private static bool dynamoContextAdded = false;

        /// <summary>
        /// Adds your database context class to the service provider.
        /// Use to configure the DynamoContext options.
        /// </summary>
        /// <typeparam name="TContext">A custom type that implements EasyDynamo.Core.DynamoContext</typeparam>
        /// <param name="services">The service collection.</param>
        /// <param name="configuration">The application configuration.</param>
        /// <param name="optionsExpression">Action over EasyDynamo.Config.DynamoContextOptions.</param>
        public static IServiceCollection AddDynamoContext<TContext>(
            this IServiceCollection services,
            IConfiguration configuration,
            Action<DynamoContextOptions> optionsExpression)
            where TContext : DynamoContext
        {
            InputValidator.ThrowIfNull(optionsExpression);

            var contextOptions = DynamoContextOptions.Instance;

            optionsExpression(contextOptions);

            services.AddDynamoContext<TContext>(configuration);
            
            return services;
        }

        /// <summary>
        /// Adds your database context class to the service provider.
        /// </summary>
        /// <typeparam name="TContext">
        /// A custom type that implements EasyDynamo.Core.DynamoContext.
        /// </typeparam>
        /// <param name="services">The service collection.</param>
        /// <param name="configuration">The application configuration.</param>
        public static IServiceCollection AddDynamoContext<TContext>(
            this IServiceCollection services, IConfiguration configuration) 
            where TContext : DynamoContext
        {
            services.AddCoreServices(configuration);

            var awsOptions = configuration.GetAWSOptions();

            services.AddDefaultAWSOptions(awsOptions);

            services.EnsureContextNotAdded<TContext>();

            var contextInstance = Instantiator.GetConstructorlessInstance<TContext>();

            BuildModels(contextInstance, configuration);

            BuildConfiguration(contextInstance, configuration);

            services.AddSingleton<TContext>();

            services.AddSingleton<IDynamoDBContext>(
                sp => new DynamoDBContext(sp.GetRequiredService<IAmazonDynamoDB>()));

            services.AddDynamoClient(awsOptions);
            
            dynamoContextAdded = true;

            return services;
        }
        
        private static IServiceCollection AddDynamoClient(
            this IServiceCollection services, AWSOptions awsOptions)
        {
            var options = DynamoContextOptions.Instance;
            var awsCredentials = awsOptions?.Credentials?.GetCredentials();

            options.Profile = options.Profile ?? awsOptions?.Profile;
            options.AccessKeyId = options.AccessKeyId ?? awsCredentials?.AccessKey;
            options.SecretAccessKey = options.SecretAccessKey ?? awsCredentials?.SecretKey;

            if (options.LocalMode)
            {
                AddDynamoLocalClient(services, options);

                return services; ;
            }

            options.RegionEndpoint = options.RegionEndpoint ?? awsOptions?.Region;

            AddDynamoCloudClient(services, options, awsOptions);

            return services;
        }

        private static void AddDynamoCloudClient(
            IServiceCollection services, 
            DynamoContextOptions contextOptions,
            AWSOptions awsOptions)
        {
            contextOptions.ValidateCloudMode();

            awsOptions.Profile = awsOptions?.Profile ?? contextOptions.Profile;
            awsOptions.Region = awsOptions?.Region ?? contextOptions.RegionEndpoint;


            services.AddAWSService<IAmazonDynamoDB>(awsOptions);
        }

        private static void AddDynamoLocalClient(
            IServiceCollection services, DynamoContextOptions options)
        {
            options.ValidateLocalMode();

            var clientConfig = new AmazonDynamoDBConfig
            {
                ServiceURL = options.ServiceUrl
            };
            
            services.AddSingleton<IAmazonDynamoDB>(
                sp => new AmazonDynamoDBClient(
                        options.AccessKeyId, options.SecretAccessKey, clientConfig));
        }

        private static IServiceCollection EnsureContextNotAdded<TContext>(
            this IServiceCollection services)
        {
            var isAlreadyAddedInServices = services
                .Any(s => s.ImplementationType == typeof(TContext));

            if (dynamoContextAdded || isAlreadyAddedInServices)
            {
                throw new DynamoContextConfigurationException(
                    "You can add only one DynamoContext.");
            }

            return services;
        }
        
        private static void BuildConfiguration<TContext>(
            TContext contextInstance, 
            IConfiguration configuration) 
            where TContext : DynamoContext
        {
            var dynamoOptionsBuilder = DynamoContextOptionsBuilder.Instance;
            var configuringMethod = typeof(TContext)
                .GetMethod("OnConfiguring", BindingFlags.Instance | BindingFlags.NonPublic);

            configuringMethod.Invoke(
                contextInstance, new object[] { dynamoOptionsBuilder, configuration });
        }

        private static void BuildModels<TContext>(
            TContext contextInstance, IConfiguration configuration) 
            where TContext : DynamoContext
        {
            var modelBuilder = ModelBuilder.Instance;
            var modelCreatingMethod = typeof(TContext)
                .GetMethod("OnModelCreating", BindingFlags.Instance | BindingFlags.NonPublic);

            modelCreatingMethod.Invoke(contextInstance, new object[] { modelBuilder, configuration });
        }

        private static IServiceCollection AddCoreServices(
            this IServiceCollection services, IConfiguration configuration)
        {
            services.AddTransient(typeof(IEntityValidator<>), typeof(EntityValidator<>));
            services.AddSingleton<DatabaseFacade>();

            typeof(ServiceCollectionExtensions)
                .Assembly
                .GetTypes()
                .Where(t => t.IsClass &&
                    !t.IsAbstract &&
                    !t.IsGenericType &&
                    t.GetInterfaces()
                        .Any(i => i.Name == $"I{t.Name}"))
                .Select(t => new
                {
                    Interface = t.GetInterface($"I{t.Name}"),
                    Implementation = t
                })
                .ToList()
                .ForEach(s => services.AddTransient(s.Interface, s.Implementation));

            return services;
        }
    }
}
